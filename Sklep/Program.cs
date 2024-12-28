using Microsoft.EntityFrameworkCore;
using Sklep.Data;
using Sklep.Models;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ShopDB"))
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ShopContext>();

    if (!context.Users.Any(u => u.Login == "Admin"))
    {
        var salt = GenerateSalt();
        var hashedPass = HashPassword("Admin", salt);

        var adminUser = new User
        {
            Login = "Admin",
            FirstName = "Admin",
            LastName = "Administrator",
            Email = "admin@admin.com",
            Address = "Admin HQ",
            IsAdmin = true,
            Salt = salt,
            PasswordHash = hashedPass
        };

        context.Users.Add(adminUser);
        context.SaveChanges();
    }
}


app.MapRazorPages();


app.Run();


string GenerateSalt()
{
    byte[] saltBytes = new byte[16];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(saltBytes);
    }
    return Convert.ToBase64String(saltBytes);
}

string HashPassword(string password, string salt)
{
    using (var sha256 = SHA256.Create())
    {
        var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
        var hash = sha256.ComputeHash(combinedBytes);
        return Convert.ToBase64String(hash);
    }
}