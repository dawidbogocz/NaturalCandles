using NaturalCandles.DataAccess.Repository.IRepository;
using NaturalCandles.DataAccess.Services.IServices;
using NaturalCandles.Models;
using NaturalCandles.Models.Enums;

namespace NaturalCandles.DataAccess.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckoutService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<ShippingMethodSetting> GetEnabledShippingMethods()
        {
            return _unitOfWork.ShippingMethodSetting
                .GetAll(x => x.IsEnabled)
                .OrderBy(x => x.SortOrder)
                .ToList();
        }

        public decimal GetShippingPrice(ShippingMethod? shippingMethod)
        {
            if (shippingMethod == null) return 0m;

            return _unitOfWork.ShippingMethodSetting
                       .Get(x => x.ShippingMethod == shippingMethod.Value && x.IsEnabled)
                       ?.Price ?? 0m;
        }

        public bool RequiresPickupPoint(ShippingMethod? shippingMethod)
        {
            if (shippingMethod == null) return false;

            return _unitOfWork.ShippingMethodSetting
                       .Get(x => x.ShippingMethod == shippingMethod.Value && x.IsEnabled)
                       ?.RequiresPickupPoint ?? false;
        }

        public bool SupportsCashOnDelivery(ShippingMethod? shippingMethod)
        {
            if (shippingMethod == null) return false;

            return _unitOfWork.ShippingMethodSetting
                       .Get(x => x.ShippingMethod == shippingMethod.Value && x.IsEnabled)
                       ?.SupportsCashOnDelivery ?? false;
        }

        public void ValidateDeliveryAndPayment(OrderHeader orderHeader, ModelStateDictionaryWrapper modelState)
        {
            if (orderHeader.ShippingMethod == null)
                modelState.AddModelError("OrderHeader.ShippingMethod", "Select a delivery method.");

            if (orderHeader.PaymentMethod == null)
                modelState.AddModelError("OrderHeader.PaymentMethod", "Select a payment method.");

            if (RequiresPickupPoint(orderHeader.ShippingMethod) &&
                string.IsNullOrWhiteSpace(orderHeader.DeliveryPointId))
            {
                modelState.AddModelError("OrderHeader.DeliveryPointId", "Select a pickup point.");
            }

            if (!RequiresPickupPoint(orderHeader.ShippingMethod))
            {
                orderHeader.DeliveryPointId = null;
                orderHeader.DeliveryPointName = null;
            }

            if (orderHeader.PaymentMethod == PaymentMethod.CashOnDelivery &&
                !SupportsCashOnDelivery(orderHeader.ShippingMethod))
            {
                modelState.AddModelError("OrderHeader.PaymentMethod",
                    "Cash on delivery is not available for the selected shipping method.");
            }

            if (orderHeader.ShippingMethod == ShippingMethod.LocalPickup)
            {
                orderHeader.DeliveryPointId = null;
                orderHeader.DeliveryPointName = null;
            }
        }
    }
}