using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sklep.Data;
using Sklep.Models;

namespace Sklep.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ShopContext _context;

        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ShopContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        // GET: Products
        public async Task<ActionResult> Index(int? categoryId)
        {
            // Pobierz tylko kategorie, które są przypisane do produktów
            var categoriesWithProducts = await _context.Products
                .Include(p => p.Category) // Pobierz powiązane kategorie
                .Where(p => p.Category != null) // Upewnij się, że produkt ma kategorię
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue && categoryId.Value != 0)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await productsQuery.ToListAsync();

            var viewModel = new ProductIndexViewModel
            {
                Products = products,
                Categories = new SelectList(categoriesWithProducts, "Id", "Name", categoryId),
                SelectedCategoryId = categoryId
            };

            return View(viewModel);
        }


        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            var viewModel = new ProductCreateViewModel
            {
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
            };
            return View(viewModel);
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel viewModel)
        {
            _logger.LogInformation("Próba dodania produktu.");
            _logger.LogInformation("Otrzymana wartość CategoryId: {CategoryId}", viewModel.CategoryId);

            // Usuń wymóg pola NewCategoryName, jeśli wybrano istniejącą kategorię
            if (viewModel.CategoryId != -1)
            {
                ModelState.Remove(nameof(viewModel.NewCategoryName));
            }

            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState jest ważny.");

                // Jeśli użytkownik wybrał dodanie nowej kategorii
                if (viewModel.CategoryId == -1)
                {
                    _logger.LogInformation("Użytkownik wybrał dodanie nowej kategorii.");

                    if (string.IsNullOrWhiteSpace(viewModel.NewCategoryName))
                    {
                        _logger.LogError("Nazwa nowej kategorii jest pusta.");
                        ModelState.AddModelError("NewCategoryName", "Nazwa nowej kategorii jest wymagana.");
                        await PopulateCategories(viewModel);
                        return View(viewModel);
                    }

                    var existingCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name.ToLower() == viewModel.NewCategoryName.Trim().ToLower());

                    if (existingCategory != null)
                    {
                        _logger.LogInformation($"Kategoria już istnieje: {existingCategory.Name} (ID: {existingCategory.Id}).");
                        viewModel.CategoryId = existingCategory.Id;
                    }
                    else
                    {
                        var newCategory = new Category { Name = viewModel.NewCategoryName.Trim() };
                        _context.Categories.Add(newCategory);
                        await _context.SaveChangesAsync();
                        viewModel.CategoryId = newCategory.Id;
                        _logger.LogInformation($"Utworzono nową kategorię: {newCategory.Name} (ID: {newCategory.Id}).");
                    }
                }

                var product = new Product
                {
                    Name = viewModel.Name,
                    Description = viewModel.Description,
                    CategoryId = viewModel.CategoryId.Value,
                    ImageUrl = viewModel.ImageUrl
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Dodano produkt: {product.Name} (ID: {product.Id}) z kategorią ID: {product.CategoryId}.");
                return RedirectToAction(nameof(Index));
            }

            // Jeśli ModelState nie jest ważny, loguj błędy
            _logger.LogError("ModelState jest nieprawidłowy.");
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    _logger.LogError($"ModelState error in {state.Key}: {error.ErrorMessage}");
                }
            }

            // Ponownie załaduj listę kategorii
            await PopulateCategories(viewModel);
            return View(viewModel);
        }



        // GET: Products/Edit/5


        // Pomocnicza metoda do załadowania kategorii
        private async Task PopulateCategories(ProductCreateViewModel viewModel)
        {
            var categories = await _context.Categories.ToListAsync();
            viewModel.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            });
        }




        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Pobierz produkt wraz z kategorią
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Stwórz ViewModel na podstawie produktu
            var viewModel = new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,

                CategoryId = product.CategoryId.Value,
                ImageUrl = product.ImageUrl,
                Categories = await _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync()
            };

            return View(viewModel);
        }



        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            _logger.LogInformation("Próba edycji produktu ID: {ProductId}", id);

            // Usuń wymóg pola NewCategoryName, jeśli wybrano istniejącą kategorię
            if (viewModel.CategoryId != -1)
            {
                ModelState.Remove(nameof(viewModel.NewCategoryName));
            }

            if (ModelState.IsValid)
            {
                if (viewModel.CategoryId == -1)
                {
                    if (string.IsNullOrWhiteSpace(viewModel.NewCategoryName))
                    {
                        ModelState.AddModelError("NewCategoryName", "Nazwa nowej kategorii jest wymagana.");
                        _logger.LogError("Pole NewCategoryName jest puste.");
                        await PopulateCategories(viewModel);
                        return View(viewModel);
                    }

                    var existingCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name.ToLower() == viewModel.NewCategoryName.Trim().ToLower());

                    if (existingCategory != null)
                    {
                        viewModel.CategoryId = existingCategory.Id;
                        _logger.LogInformation($"Użyto istniejącej kategorii: {existingCategory.Name} (ID: {existingCategory.Id})");
                    }
                    else
                    {
                        var newCategory = new Category
                        {
                            Name = viewModel.NewCategoryName.Trim()
                        };
                        _context.Categories.Add(newCategory);
                        await _context.SaveChangesAsync();
                        viewModel.CategoryId = newCategory.Id;
                        _logger.LogInformation($"Utworzono nową kategorię: {newCategory.Name} (ID: {newCategory.Id})");
                    }
                }

                if (viewModel.CategoryId <= 0)
                {
                    ModelState.AddModelError("CategoryId", "Kategoria jest wymagana.");
                    _logger.LogError("CategoryId jest nieprawidłowy.");
                    await PopulateCategories(viewModel);
                    return View(viewModel);
                }

                try
                {
                    var productToUpdate = await _context.Products.FindAsync(id);
                    if (productToUpdate == null)
                    {
                        return NotFound();
                    }

                    productToUpdate.Name = viewModel.Name;
                    productToUpdate.Description = viewModel.Description;
                    productToUpdate.CategoryId = viewModel.CategoryId;
                    productToUpdate.ImageUrl = viewModel.ImageUrl;

                    _context.Update(productToUpdate);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Zaktualizowano produkt ID: {productToUpdate.Id}");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            _logger.LogError("ModelState jest nieprawidłowy podczas edycji produktu.");
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    _logger.LogError($"ModelState error in {state.Key}: {error.ErrorMessage}");
                }
            }

            await PopulateCategories(viewModel);
            return View(viewModel);
        }



        // Pomocnicza metoda do załadowania kategorii
        private async Task PopulateCategories(ProductEditViewModel viewModel)
        {
            var categories = await _context.Categories.ToListAsync();
            viewModel.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            });
        }



        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        // POST: Products/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
