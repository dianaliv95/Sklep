using Microsoft.AspNetCore.Mvc;
using Sklep.Data;
using Sklep.Models;
using System.Security.Cryptography;
using System.Text;

namespace Sklep.Controllers
{
    public class AuthController : Controller
    {
        private readonly ShopContext _context;

        public AuthController(ShopContext context)
        {
            _context = context;
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Sprawdzamy czy login jest już zajęty
            var existingUser = _context.Users.FirstOrDefault(u => u.Login == model.Login);
            if (existingUser != null)
            {
                ModelState.AddModelError("Login", "Ten login jest już zajęty.");
                return View(model);
            }

            // Generujemy sól
            var salt = GenerateSalt();
            // Hashujemy hasło
            var hashedPassword = HashPassword(model.Password, salt);

            // Tworzymy encję User i zapisujemy do bazy
            var newUser = new User
            {
                Login = model.Login,
                PasswordHash = hashedPassword,
                Salt = salt,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Address = model.Address
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            // Po rejestracji – przekierowanie do strony logowania lub od razu zalogowanie
            return RedirectToAction("Login");
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Szukamy użytkownika po loginie
            var user = _context.Users.FirstOrDefault(u => u.Login == model.Login);
            if (user == null)
            {
                ModelState.AddModelError("", "Nieprawidłowy login lub hasło.");
                return View(model);
            }

            // Sprawdzamy hasło
            var hashedInput = HashPassword(model.Password, user.Salt);
            if (hashedInput != user.PasswordHash)
            {
                ModelState.AddModelError("", "Nieprawidłowy login lub hasło.");
                return View(model);
            }

            // Logowanie na sesji – zapisujemy ID użytkownika w sesji
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("IsAdmin", user.IsAdmin ? "true" : "false");

            // Można również trzymać w sesji np. login/imię/nazwisko
            // HttpContext.Session.SetString("UserLogin", user.Login);
            // HttpContext.Session.SetString("FullName", user.FirstName + " " + user.LastName);

            // Przekieruj gdzie chcesz – np. do strony głównej
            return RedirectToAction("Index", "Home");
        }

        // /Auth/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            // Czyścimy sesję
            HttpContext.Session.Clear();

            // Przekierowanie na stronę główną lub do logowania
            return RedirectToAction("Index", "Home");
        }


        // ------------------------------
        // Pomocnicze metody szyfrujące:
        // ------------------------------

        /// <summary>
        /// Generuje losową sól (np. 16-bajtową)
        /// </summary>
        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// Hashuje hasło za pomocą SHA256 + sól
        /// </summary>
        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
                var hash = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
