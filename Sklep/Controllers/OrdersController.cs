using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sklep.Data;
using Sklep.Models;

namespace Sklep.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ShopContext _context;

        public OrdersController(ShopContext context)
        {
            _context = context;
        }

        // GET: Orders/Index
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

            // Jeżeli to admin -> widzi wszystkie zamówienia
            // Jeżeli nieadmin -> widzi tylko swoje
            IQueryable<Order> ordersQuery = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderProducts).ThenInclude(op => op.Product);

            if (!isAdmin)
            {
                // userId może być nullem, ale jeśli tu trafiamy, to raczej jest zalogowany
                ordersQuery = ordersQuery.Where(o => o.UserId == userId);
            }

            var orders = await ordersQuery.ToListAsync();
            return View(orders);
        }


        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            var users = await _context.Users.ToListAsync();
            var products = await _context.Products.ToListAsync();

            var model = new OrderEditViewModel
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                SelectedProductIds = order.OrderProducts.Select(op => op.ProductId).ToList(),
                UsersSelectList = users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName}"
                }).ToList(),
                ProductsSelectList = products.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToList()
            };

            return View(model);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.OrderProducts)
                        .FirstOrDefaultAsync(o => o.Id == id);

                    if (order == null)
                    {
                        return NotFound();
                    }

                    order.UserId = model.UserId;
                    order.OrderDate = model.OrderDate;

                   
                    if (model.SelectedProductIds != null && model.SelectedProductIds.Any())
                    {
                        
                        _context.OrderProducts.RemoveRange(order.OrderProducts);

                        
                        foreach (var productId in model.SelectedProductIds)
                        {
                            order.OrderProducts.Add(new OrderProduct
                            {
                                ProductId = productId
                            });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Musisz wybrać co najmniej jeden produkt.");
                        await PopulateSelectLists(model);
                        return View(model);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Wystąpił problem podczas aktualizacji zamówienia. Spróbuj ponownie.");
                        await PopulateSelectLists(model);
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    
                    Console.WriteLine($"Error: {ex.Message}");
                    ModelState.AddModelError("", "Wystąpił nieoczekiwany błąd.");
                    await PopulateSelectLists(model);
                    return View(model);
                }

                return RedirectToAction(nameof(Index));
            }

            // Jeśli ModelState nie jest prawidłowy, ponownie wypełnij listy
            await PopulateSelectLists(model);
            return View(model);
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // Brak zalogowania -> przekieruj do logowania
                return RedirectToAction("Login", "Auth");
            }
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

            // jeśli admin, pobieramy listę userów do selekta
            var users = isAdmin
                ? await _context.Users.ToListAsync()
                : await _context.Users.Where(u => u.Id == userId).ToListAsync();

            var products = await _context.Products.ToListAsync();

            var model = new OrderCreateViewModel
            {
                UsersSelectList = users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName}"
                }).ToList(),
                ProductsSelectList = products.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToList()
            };

            return View(model);
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

            if (ModelState.IsValid)
            {
                try
                {
                    var order = new Order
                    {
                        UserId = model.UserId,
                        OrderDate = DateTime.Now
                    };

                    // Dodanie wybranych produktów do zamówienia
                    if (model.SelectedProductIds != null && model.SelectedProductIds.Any())
                    {
                        foreach (var productId in model.SelectedProductIds)
                        {
                            order.OrderProducts.Add(new OrderProduct
                            {
                                ProductId = productId
                            });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Musisz wybrać co najmniej jeden produkt.");
                        await PopulateSelectLists(model);
                        return View(model);
                    }
                    var finalUserId = (isAdmin && model.UserId > 0) ? model.UserId : userId.Value;
                     order = new Order
                    {
                        UserId = finalUserId,
                        OrderDate = DateTime.Now
                    };

                    if (model.SelectedProductIds == null || !model.SelectedProductIds.Any())
                    {
                        ModelState.AddModelError("", "Musisz wybrać co najmniej jeden produkt.");
                        await PopulateSelectLists(model, isAdmin, userId);
                        return View(model);
                    }

                    foreach (var productId in model.SelectedProductIds)
                    {
                        order.OrderProducts.Add(new OrderProduct
                        {
                            ProductId = productId
                        });
                    }
                    // Zapis zamówienia do bazy danych
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Wystąpił błąd podczas zapisu: {ex.Message}");
                }
            }

            // Ponowne wypełnienie list w przypadku błędów walidacji
            await PopulateSelectLists(model);
            return View(model);
        }

        private async Task PopulateSelectLists(OrderCreateViewModel model, bool isAdmin, int? userId)
        {
            var users = isAdmin
                ? await _context.Users.ToListAsync()
                : await _context.Users.Where(u => u.Id == userId).ToListAsync();

            var products = await _context.Products.ToListAsync();

            model.UsersSelectList = users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            model.ProductsSelectList = products.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();
        }


        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderProducts)
                    .FirstOrDefaultAsync(o => o.Id == id);
                if (order != null)
                {
                    // Usunięcie powiązanych produktów w zamówieniu
                    _context.OrderProducts.RemoveRange(order.OrderProducts);

                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                // Logowanie wyjątku
                Console.WriteLine($"Database delete error: {ex.Message}");
                ModelState.AddModelError("", "Wystąpił błąd podczas usuwania zamówienia.");
                var order = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);
                if (order == null)
                {
                    return NotFound();
                }
                return View(order);
            }
            catch (Exception ex)
            {
                // Logowanie wyjątku
                Console.WriteLine($"General delete error: {ex.Message}");
                ModelState.AddModelError("", "Wystąpił nieoczekiwany błąd.");
                var order = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);
                if (order == null)
                {
                    return NotFound();
                }
                return View(order);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }

        private async Task PopulateSelectLists(OrderCreateViewModel model)
        {
            var usersList = await _context.Users.ToListAsync();
            var productsList = await _context.Products.ToListAsync();

            model.UsersSelectList = usersList.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            model.ProductsSelectList = productsList.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User) // Ładuje dane użytkownika
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product) // Ładuje produkty w zamówieniu
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        private async Task PopulateSelectLists(OrderEditViewModel model)
        {
            var usersList = await _context.Users.ToListAsync();
            var productsList = await _context.Products.ToListAsync();

            model.UsersSelectList = usersList.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            model.ProductsSelectList = productsList.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();
        }
    }
}
