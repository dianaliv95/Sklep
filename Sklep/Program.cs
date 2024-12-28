using Microsoft.EntityFrameworkCore;
using Sklep.Data;
using Sklep.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ShopDB"))
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));



var app = builder.Build();

// Configure the HTTP request pipeline.




app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.UseRouting();


app.UseAuthorization();

app.MapRazorPages();

app.Run();
