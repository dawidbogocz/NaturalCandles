using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaturalCandles.DataAccess.Repository.IRepository;
using NaturalCandles.Models;
using NaturalCandles.Utility;
using System.Diagnostics;
using System.Security.Claims;

namespace NaturalCandles.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                HttpContext.Session.SetInt32(
                    SD.SessionCart,
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).Count());
            }

            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,PriceTiers");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            var cart = new ShoppingCart
            {
                Product = _unitOfWork.Product.Get(u => u.ProductId == productId, includeProperties: "Category,PriceTiers"),
                Count = 1,
                ProductId = productId
            };

            if (cart.Product == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            return AddOrUpdateCart(shoppingCart);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult QuickAdd(ShoppingCart shoppingCart)
        {
            return AddOrUpdateCart(shoppingCart);
        }

        private IActionResult AddOrUpdateCart(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count < 1)
            {
                shoppingCart.Count = 1;
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            shoppingCart.ApplicationUserId = userId;

            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            else
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }

            _unitOfWork.Save();

            HttpContext.Session.SetInt32(
                SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());

            TempData["success"] = "Cart updated successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}