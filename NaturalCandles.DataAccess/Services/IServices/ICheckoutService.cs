using NaturalCandles.Models;
using NaturalCandles.Models.Enums;

namespace NaturalCandles.DataAccess.Services.IServices
{
    public interface ICheckoutService
    {
        List<ShippingMethodSetting> GetEnabledShippingMethods();
        decimal GetShippingPrice(ShippingMethod? shippingMethod);
        bool RequiresPickupPoint(ShippingMethod? shippingMethod);
        bool SupportsCashOnDelivery(ShippingMethod? shippingMethod);
        void ValidateDeliveryAndPayment(OrderHeader orderHeader, ModelStateDictionaryWrapper modelState);
    }

    public interface ModelStateDictionaryWrapper
    {
        void AddModelError(string key, string errorMessage);
    }
}