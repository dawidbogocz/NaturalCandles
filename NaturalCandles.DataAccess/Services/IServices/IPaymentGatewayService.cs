using NaturalCandles.Models;
using NaturalCandles.Models.ViewModels;

namespace NaturalCandles.DataAccess.Services.IServices
{
    public interface IPaymentGatewayService
    {
        Task<PaymentStartResult> StartPaymentAsync(OrderHeader orderHeader, IEnumerable<OrderDetail> details);
        Task<bool> VerifyPaymentAsync(OrderHeader orderHeader);
    }

    public class PaymentStartResult
    {
        public bool Success { get; set; }
        public string? RedirectUrl { get; set; }
        public string? ExternalSessionId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}