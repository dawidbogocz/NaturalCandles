using Microsoft.AspNetCore.Mvc;
using NaturalCandles.DataAccess.Repository.IRepository;
using NaturalCandles.DataAccess.Services.IServices;
using NaturalCandles.Utility;
using NaturalCandles.Utility.Services;

namespace NaturalCandles.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CheckoutController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentGatewayService _paymentGatewayService;

        public CheckoutController(
            IUnitOfWork unitOfWork,
            IPaymentGatewayService paymentGatewayService)
        {
            _unitOfWork = unitOfWork;
            _paymentGatewayService = paymentGatewayService;
        }

        public async Task<IActionResult> Return(int orderId)
        {
            var order = _unitOfWork.OrderHeader.Get(u => u.Id == orderId);
            if (order == null) return NotFound();

            var verified = await _paymentGatewayService.VerifyPaymentAsync(order);

            if (verified)
            {
                order.PaymentStatus = SD.PaymentStatusApproved;
                order.OrderStatus = SD.StatusApproved;
                _unitOfWork.OrderHeader.Update(order);

                var carts = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == order.ApplicationUserId);
                _unitOfWork.ShoppingCart.RemoveRange(carts);

                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, 0);
            }

            return RedirectToAction("Confirmation", "Cart", new { area = "Customer", orderId = order.Id });
        }

        [HttpPost]
        public IActionResult Notify()
        {
            return Ok();
        }
    }
}