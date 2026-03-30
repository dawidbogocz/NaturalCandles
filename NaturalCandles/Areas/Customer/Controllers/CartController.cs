using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaturalCandles.DataAccess.Repository.IRepository;
using NaturalCandles.Models;
using NaturalCandles.Models.Enums;
using NaturalCandles.Models.ViewModels;
using NaturalCandles.Utility;
using System.Security.Claims;

namespace NaturalCandles.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var userId = GetCurrentUserId();
            ShoppingCartVM = BuildShoppingCartVm(userId);
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var userId = GetCurrentUserId();
            ShoppingCartVM = BuildShoppingCartVm(userId);

            var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            if (applicationUser == null) return NotFound();

            ShoppingCartVM.OrderHeader.Name = !string.IsNullOrWhiteSpace(applicationUser.FullName)
                ? applicationUser.FullName
                : applicationUser.UserName;

            ShoppingCartVM.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = applicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = applicationUser.City;
            ShoppingCartVM.OrderHeader.State = applicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = applicationUser.PostalCode;

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var userId = GetCurrentUserId();

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Product,Product.PriceTiers");

            if (!ShoppingCartVM.ShoppingCartList.Any())
            {
                TempData["error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            if (applicationUser == null) return NotFound();

            decimal productsTotal = 0m;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                productsTotal += cart.Price * cart.Count;
            }

            if (ShoppingCartVM.OrderHeader.ShippingMethod == null)
                ModelState.AddModelError("OrderHeader.ShippingMethod", "Select a delivery method.");

            if (ShoppingCartVM.OrderHeader.PaymentMethod == null)
                ModelState.AddModelError("OrderHeader.PaymentMethod", "Select a payment method.");

            if (RequiresPickupPoint(ShoppingCartVM.OrderHeader.ShippingMethod) &&
                string.IsNullOrWhiteSpace(ShoppingCartVM.OrderHeader.DeliveryPointId))
            {
                ModelState.AddModelError("OrderHeader.DeliveryPointId", "Select or enter a pickup point.");
            }

            if (!ModelState.IsValid)
            {
                ShoppingCartVM.OrderHeader.DeliveryPrice = CalculateDeliveryPrice(ShoppingCartVM.OrderHeader.ShippingMethod);
                ShoppingCartVM.OrderHeader.OrderTotal = productsTotal + ShoppingCartVM.OrderHeader.DeliveryPrice;
                return View("Summary", ShoppingCartVM);
            }

            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.UtcNow;
            ShoppingCartVM.OrderHeader.DeliveryPrice = CalculateDeliveryPrice(ShoppingCartVM.OrderHeader.ShippingMethod);
            ShoppingCartVM.OrderHeader.OrderTotal = productsTotal + ShoppingCartVM.OrderHeader.DeliveryPrice;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.Currency = "PLN";

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                _unitOfWork.OrderDetail.Add(new OrderDetail
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                });
            }

            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ShoppingCartList);
            _unitOfWork.Save();

            HttpContext.Session.SetInt32(SD.SessionCart, 0);
            TempData["success"] = $"Order #{ShoppingCartVM.OrderHeader.Id} created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Plus(int cartId)
        {
            var userId = GetCurrentUserId();

            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.Id == cartId && u.ApplicationUserId == userId,
                tracked: true);

            if (cartFromDb == null) return NotFound();

            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();

            RefreshCartSession(userId);
            TempData["success"] = "Quantity updated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Minus(int cartId)
        {
            var userId = GetCurrentUserId();

            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.Id == cartId && u.ApplicationUserId == userId,
                tracked: true);

            if (cartFromDb == null) return NotFound();

            if (cartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }

            _unitOfWork.Save();
            RefreshCartSession(userId);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int cartId)
        {
            var userId = GetCurrentUserId();

            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.Id == cartId && u.ApplicationUserId == userId,
                tracked: true);

            if (cartFromDb == null) return NotFound();

            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();

            RefreshCartSession(userId);

            return RedirectToAction(nameof(Index));
        }

        private ShoppingCartVM BuildShoppingCartVm(string userId)
        {
            var vm = new ShoppingCartVM
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == userId,
                    includeProperties: "Product,Product.PriceTiers"),
                OrderHeader = new OrderHeader()
            };

            decimal productsTotal = 0m;

            foreach (var cart in vm.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                productsTotal += cart.Price * cart.Count;
            }

            vm.OrderHeader.OrderTotal = productsTotal;
            return vm;
        }

        private string GetCurrentUserId()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            return claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        }

        private void RefreshCartSession(string userId)
        {
            var cartCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count();
            HttpContext.Session.SetInt32(SD.SessionCart, cartCount);
        }

        private static bool RequiresPickupPoint(ShippingMethod? shippingMethod)
        {
            return shippingMethod == ShippingMethod.InPostLocker ||
                   shippingMethod == ShippingMethod.OrlenPaczka;
        }

        private static decimal CalculateDeliveryPrice(ShippingMethod? shippingMethod)
        {
            return shippingMethod switch
            {
                ShippingMethod.InPostLocker => 14.99m,
                ShippingMethod.InPostCourier => 16.99m,
                ShippingMethod.DpdCourier => 18.99m,
                ShippingMethod.OrlenPaczka => 12.99m,
                ShippingMethod.LocalPickup => 0m,
                _ => 0m
            };
        }

        private decimal GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            var tiers = shoppingCart.Product?.PriceTiers?
                .OrderByDescending(x => x.MinQuantity)
                .ToList();

            if (tiers != null && tiers.Any())
            {
                var matchedTier = tiers.FirstOrDefault(x => shoppingCart.Count >= x.MinQuantity);
                return matchedTier?.Price ?? tiers.OrderBy(x => x.MinQuantity).First().Price;
            }

            // Backward compatibility for older products not yet migrated to dynamic tiers
            if (shoppingCart.Count >= 10 && shoppingCart.Product.Price10 > 0)
                return shoppingCart.Product.Price10;

            if (shoppingCart.Count >= 4 && shoppingCart.Product.Price4 > 0)
                return shoppingCart.Product.Price4;

            return shoppingCart.Product.BasePrice;
        }
    }
}