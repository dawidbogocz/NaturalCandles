using Microsoft.Extensions.Options;
using NaturalCandles.DataAccess.Services;
using NaturalCandles.DataAccess.Services.IServices;
using NaturalCandles.Models;
using NaturalCandles.Models.Enums;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NaturalCandles.Utility.Services
{
    public class Przelewy24PaymentGatewayService : IPaymentGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly Przelewy24Options _options;

        public Przelewy24PaymentGatewayService(
            IHttpClientFactory httpClientFactory,
            IOptions<Przelewy24Options> options)
        {
            _httpClient = httpClientFactory.CreateClient();
            _options = options.Value;
        }

        public async Task<PaymentStartResult> StartPaymentAsync(OrderHeader orderHeader, IEnumerable<OrderDetail> details)
        {
            if (orderHeader.PaymentMethod != PaymentMethod.Przelewy24 &&
                orderHeader.PaymentMethod != PaymentMethod.Blik &&
                orderHeader.PaymentMethod != PaymentMethod.Card)
            {
                return new PaymentStartResult { Success = true };
            }

            var sessionId = $"NC-{orderHeader.Id}-{Guid.NewGuid():N}";
            var amountInGrosz = (int)Math.Round(orderHeader.OrderTotal * 100m, 0);

            var signPayload = new
            {
                sessionId,
                merchantId = int.Parse(_options.MerchantId),
                amount = amountInGrosz,
                currency = "PLN",
                crc = _options.Crc
            };

            var sign = ComputeSha384(JsonSerializer.Serialize(signPayload));

            var body = new
            {
                merchantId = int.Parse(_options.MerchantId),
                posId = int.Parse(_options.PosId),
                sessionId = sessionId,
                amount = amountInGrosz,
                currency = "PLN",
                description = $"Order {orderHeader.Id}",
                email = orderHeader.EmailAddress,
                country = "PL",
                language = "pl",
                urlReturn = $"{_options.ReturnUrl}?orderId={orderHeader.Id}",
                urlStatus = _options.StatusUrl,
                sign = sign
            };

            var auth = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_options.PosId}:{_options.ApiKey}"));

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_options.BaseUrl}/api/v1/transaction/register");

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new PaymentStartResult
                {
                    Success = false,
                    ErrorMessage = $"P24 register failed: {responseBody}"
                };
            }

            using var doc = JsonDocument.Parse(responseBody);

            var token = doc.RootElement
                .GetProperty("data")
                .GetProperty("token")
                .GetString();

            return new PaymentStartResult
            {
                Success = true,
                ExternalSessionId = sessionId,
                RedirectUrl = $"{_options.BaseUrl}/trnRequest/{token}"
            };
        }

        public async Task<bool> VerifyPaymentAsync(OrderHeader orderHeader)
        {
            if (string.IsNullOrWhiteSpace(orderHeader.SessionId))
                return false;

            if (string.IsNullOrWhiteSpace(orderHeader.PaymentIntentId))
                return false;

            var amountInGrosz = (int)Math.Round(orderHeader.OrderTotal * 100m, 0);

            var signPayload = new
            {
                sessionId = orderHeader.SessionId,
                orderId = int.Parse(orderHeader.PaymentIntentId),
                amount = amountInGrosz,
                currency = "PLN",
                crc = _options.Crc
            };

            var sign = ComputeSha384(JsonSerializer.Serialize(signPayload));

            var body = new
            {
                merchantId = int.Parse(_options.MerchantId),
                posId = int.Parse(_options.PosId),
                sessionId = orderHeader.SessionId,
                amount = amountInGrosz,
                currency = "PLN",
                orderId = int.Parse(orderHeader.PaymentIntentId),
                sign = sign
            };

            var auth = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_options.PosId}:{_options.ApiKey}"));

            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"{_options.BaseUrl}/api/v1/transaction/verify");

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        private static string ComputeSha384(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = SHA384.HashData(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}