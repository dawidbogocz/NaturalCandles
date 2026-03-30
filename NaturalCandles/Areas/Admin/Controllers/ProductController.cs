using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NaturalCandles.DataAccess.Data;
using NaturalCandles.DataAccess.Repository.IRepository;
using NaturalCandles.Models;
using NaturalCandles.Models.ViewModels;
using NaturalCandles.Utility;

namespace NaturalCandles.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _db;

        public ProductController(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _db = db;
        }

        public IActionResult Index()
        {
            var objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return View(objProductList);
        }

        public IActionResult Upsert(int? id)
        {
            var productVM = new ProductVM
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.CategoryName,
                    Value = u.CategoryName
                }),
                Product = new Product()
            };

            if (id == null || id == 0)
            {
                productVM.PriceTiers = new List<ProductPriceTier>
                {
                    new() { MinQuantity = 1, Price = 0 }
                };

                return View(productVM);
            }

            productVM.Product = _unitOfWork.Product.Get(
                u => u.ProductId == id,
                includeProperties: "PriceTiers");

            if (productVM.Product == null)
            {
                return NotFound();
            }

            productVM.PriceTiers = productVM.Product.PriceTiers?
                .OrderBy(x => x.MinQuantity)
                .ToList() ?? new List<ProductPriceTier>();

            // Backward compatibility for older products with no tiers yet
            if (!productVM.PriceTiers.Any())
            {
                productVM.PriceTiers = new List<ProductPriceTier>
                {
                    new() { MinQuantity = 1, Price = productVM.Product.BasePrice },
                    new() { MinQuantity = 4, Price = productVM.Product.Price4 },
                    new() { MinQuantity = 10, Price = productVM.Product.Price10 }
                };
            }

            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
            {
                Text = u.CategoryName,
                Value = u.CategoryName
            });

            productVM.PriceTiers ??= new List<ProductPriceTier>();

            productVM.PriceTiers = productVM.PriceTiers
                .Where(x => x.MinQuantity > 0 && x.Price > 0)
                .OrderBy(x => x.MinQuantity)
                .ToList();

            if (!productVM.PriceTiers.Any())
            {
                ModelState.AddModelError("", "Add at least one valid pricing tier.");
            }

            if (productVM.PriceTiers.FirstOrDefault()?.MinQuantity != 1)
            {
                ModelState.AddModelError("", "The first pricing tier must start at quantity 1.");
            }

            var duplicatedMinQty = productVM.PriceTiers
                .GroupBy(x => x.MinQuantity)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicatedMinQty != null)
            {
                ModelState.AddModelError("", $"Duplicate pricing tier start quantity: {duplicatedMinQty.Key}");
            }

            if (!ModelState.IsValid)
            {
                return View(productVM);
            }

            SyncLegacyPrices(productVM.Product, productVM.PriceTiers);

            string wwwRootPath = _webHostEnvironment.WebRootPath;

            if (file != null)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, "images", "product");

                Directory.CreateDirectory(productPath);

                if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                {
                    var oldImagePath = Path.Combine(
                        wwwRootPath,
                        productVM.Product.ImageUrl.TrimStart('\\', '/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create);
                file.CopyTo(fileStream);

                productVM.Product.ImageUrl = $@"\images\product\{fileName}";
            }

            if (productVM.Product.ProductId == 0)
            {
                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Save();

                foreach (var tier in productVM.PriceTiers)
                {
                    tier.Id = 0;
                    tier.ProductId = productVM.Product.ProductId;
                    _db.ProductPriceTiers.Add(tier);
                }
            }
            else
            {
                var existingProduct = _db.Products
                    .Include(p => p.PriceTiers)
                    .FirstOrDefault(p => p.ProductId == productVM.Product.ProductId);

                if (existingProduct == null)
                {
                    return NotFound();
                }

                existingProduct.Name = productVM.Product.Name;
                existingProduct.Description = productVM.Product.Description;
                existingProduct.CategoryName = productVM.Product.CategoryName;
                existingProduct.BasePrice = productVM.Product.BasePrice;
                existingProduct.Price4 = productVM.Product.Price4;
                existingProduct.Price10 = productVM.Product.Price10;
                existingProduct.AvailableNow = productVM.Product.AvailableNow;
                existingProduct.HasColorOption = productVM.Product.HasColorOption;
                existingProduct.HasTypeOption = productVM.Product.HasTypeOption;
                existingProduct.HasColorSchemeOption = productVM.Product.HasColorSchemeOption;

                if (!string.IsNullOrWhiteSpace(productVM.Product.ImageUrl))
                {
                    existingProduct.ImageUrl = productVM.Product.ImageUrl;
                }

                _db.ProductPriceTiers.RemoveRange(existingProduct.PriceTiers);

                foreach (var tier in productVM.PriceTiers)
                {
                    _db.ProductPriceTiers.Add(new ProductPriceTier
                    {
                        ProductId = existingProduct.ProductId,
                        MinQuantity = tier.MinQuantity,
                        Price = tier.Price
                    });
                }
            }

            _unitOfWork.Save();
            TempData["success"] = $"Product {(productVM.Product.ProductId == 0 ? "created" : "updated")} successfully.";
            return RedirectToAction(nameof(Index));
        }

        private static void SyncLegacyPrices(Product product, List<ProductPriceTier> tiers)
        {
            var ordered = tiers.OrderBy(x => x.MinQuantity).ToList();

            product.BasePrice = ordered.FirstOrDefault(x => x.MinQuantity <= 1)?.Price ?? ordered.First().Price;
            product.Price4 = ordered.LastOrDefault(x => x.MinQuantity <= 4)?.Price ?? product.BasePrice;
            product.Price10 = ordered.LastOrDefault(x => x.MinQuantity <= 10)?.Price ?? product.Price4;
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.ProductId == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            if (!string.IsNullOrWhiteSpace(productToBeDeleted.ImageUrl))
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\', '/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successful" });
        }

        #endregion
    }
}