namespace NaturalCandles.DataAccess.Services
{
    public class Przelewy24Options
    {
        public string MerchantId { get; set; } = string.Empty;
        public string PosId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Crc { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://przelewy24.pl";
        public string ReturnUrl { get; set; } = string.Empty;
        public string StatusUrl { get; set; } = string.Empty;
    }
}