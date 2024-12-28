using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sklep.Data;
using Sklep.Models;
using Microsoft.Extensions.Logging;

namespace Sklep.Controllers
{
    public class UsersController : Controller
    {
        private readonly ShopContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ShopContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Users/Index
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details: Id nie został podany.");
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                _logger.LogWarning($"Details: Użytkownik o Id {id} nie został znaleziony.");
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Email,Address")] User user)
        {
            _logger.LogInformation("Tworzenie nowego użytkownika.");
            if (ModelState.IsValid)
            {
                try
                {
                    // Sprawdzenie unikalności Email
                    if (_context.Users.Any(u => u.Email == user.Email))
                    {
                        ModelState.AddModelError("Email", "Użytkownik z tym adresem email już istnieje.");
                        _logger.LogWarning("Próba dodania użytkownika z istniejącym adresem email.");
                        return View(user);
                    }

                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Użytkownik {user.FirstName} {user.LastName} został dodany.");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas dodawania użytkownika.");
                    ModelState.AddModelError("", "Wystąpił błąd podczas dodawania użytkownika.");
                }
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit: Id nie został podany.");
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"Edit: Użytkownik o Id {id} nie został znaleziony.");
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,Address")] User user)
        {
            if (id != user.Id)
            {
                _logger.LogWarning("Edit: Id w URL nie zgadza się z Id w modelu.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Sprawdzenie unikalności Email
                    if (_context.Users.Any(u => u.Email == user.Email && u.Id != user.Id))
                    {
                        ModelState.AddModelError("Email", "Użytkownik z tym adresem email już istnieje.");
                        _logger.LogWarning("Próba edycji użytkownika z istniejącym adresem email.");
                        return View(user);
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Użytkownik {user.FirstName} {user.LastName} został zaktualizowany.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!UserExists(user.Id))
                    {
                        _logger.LogWarning($"Edit: Użytkownik o Id {id} nie został znaleziony podczas aktualizacji.");
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Błąd podczas aktualizacji użytkownika.");
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete: Id nie został podany.");
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                _logger.LogWarning($"Delete: Użytkownik o Id {id} nie został znaleziony.");
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        // POST: Users/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Użytkownik {user.FirstName} {user.LastName} został usunięty.");
            }
            else
            {
                _logger.LogWarning($"DeleteConfirmed: Użytkownik o Id {id} nie został znaleziony.");
            }

            return RedirectToAction(nameof(Index));
        }


        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}