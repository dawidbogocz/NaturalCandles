using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaturalCandles.DataAccess.Repository.IRepository;
using NaturalCandles.DataAccess.Services;
using NaturalCandles.DataAccess.Services.IServices;
using NaturalCandles.Models;
using NaturalCandles.Models.Enums;
using NaturalCandles.Models.ViewModels;
using NaturalCandles.Utility;
using NaturalCandles.Utility.Services;
using System.Security.Claims;

namespace NaturalCandles.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICheckoutService _checkoutService;
        private readonly IPaymentGatewayService _paymentGatewayService;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; } = new();

        public CartController(
            IUnitOfWork unitOfWork,
            ICheckoutService checkoutService,
            IPaymentGatewayService paymentGatewayService)
        {
            _unitOfWork = unitOfWork;
            _checkoutService = checkoutService;
            _paymentGatewayService = paymentGatewayService;
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
            ShoppingCartVM.OrderHeader.EmailAddress = applicationUser.Email;

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPOST(ShoppingCartVM input)
        {
            var userId = GetCurrentUserId();

            ShoppingCartVM = BuildShoppingCartVm(userId);
            ShoppingCartVM.OrderHeader = input.OrderHeader ?? new OrderHeader();

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Product,Product.PriceTiers");

            if (!ShoppingCartVM.ShoppingCartList.Any())
            {
                TempData["error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            decimal productsTotal = 0m;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                productsTotal += cart.Price * cart.Count;
            }

            var wrapper = new MvcModelStateWrapper(ModelState);
            _checkoutService.ValidateDeliveryAndPayment(ShoppingCartVM.OrderHeader, wrapper);

            ShoppingCartVM.OrderHeader.DeliveryPrice =
                _checkoutService.GetShippingPrice(ShoppingCartVM.OrderHeader.ShippingMethod);

            ShoppingCartVM.OrderHeader.OrderTotal =
                productsTotal + ShoppingCartVM.OrderHeader.DeliveryPrice;

            if (!ModelState.IsValid)
            {
                return View("Summary", ShoppingCartVM);
            }

            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.UtcNow;
            ShoppingCartVM.OrderHeader.Currency = "PLN";
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            var details = new List<OrderDetail>();
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                var detail = new OrderDetail
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };

                details.Add(detail);
                _unitOfWork.OrderDetail.Add(detail);
            }

            _unitOfWork.Save();

            if (ShoppingCartVM.OrderHeader.PaymentMethod == PaymentMethod.Przelewy24 ||
                ShoppingCartVM.OrderHeader.PaymentMethod == PaymentMethod.Blik ||
                ShoppingCartVM.OrderHeader.PaymentMethod == PaymentMethod.Card)
            {
                var paymentResult = await _paymentGatewayService.StartPaymentAsync(
                    ShoppingCartVM.OrderHeader,
                    details);

                if (!paymentResult.Success)
                {
                    TempData["error"] = paymentResult.ErrorMessage ?? "Unable to start payment.";
                    return View("Summary", ShoppingCartVM);
                }

                ShoppingCartVM.OrderHeader.SessionId = paymentResult.ExternalSessionId;
                _unitOfWork.OrderHeader.Update(ShoppingCartVM.OrderHeader);
                _unitOfWork.Save();

                return Redirect(paymentResult.RedirectUrl!);
            }

            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ShoppingCartList);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.SessionCart, 0);

            return RedirectToAction(nameof(Confirmation), new { orderId = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult Confirmation(int orderId)
        {
            var order = _unitOfWork.OrderHeader.Get(u => u.Id == orderId);
            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Plus(int cartId)
        {
            var userId = GetCurrentUserId();

            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.Id == cartId && u.ApplicationUserId == userId,
                tracked: true);

            if (cartFromDb == null)
            {
                TempData["error"] = "Cart item not found.";
                return RedirectToAction(nameof(Index));
            }

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

            if (cartFromDb == null)
            {
                TempData["error"] = "Cart item not found.";
                return RedirectToAction(nameof(Index));
            }

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

            if (cartFromDb == null)
            {
                TempData["error"] = "Cart item not found.";
                return RedirectToAction(nameof(Index));
            }

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
                OrderHeader = new OrderHeader(),
                ShippingMethodSettings = _checkoutService.GetEnabledShippingMethods()
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
            var cartCount = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == userId)
                .Count();

            HttpContext.Session.SetInt32(SD.SessionCart, cartCount);
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

            if (shoppingCart.Product == null)
                return 0m;

            if (shoppingCart.Count >= 10 && shoppingCart.Product.Price10 > 0)
                return shoppingCart.Product.Price10;

            if (shoppingCart.Count >= 4 && shoppingCart.Product.Price4 > 0)
                return shoppingCart.Product.Price4;

            return shoppingCart.Product.BasePrice;
        }
    }
}