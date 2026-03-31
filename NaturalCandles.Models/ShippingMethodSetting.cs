using NaturalCandles.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace NaturalCandles.Models
{
    public class ShippingMethodSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public ShippingMethod ShippingMethod { get; set; }

        [Required]
        [MaxLength(120)]
        public string DisplayName { get; set; } = string.Empty;

        [Range(0, 999999)]
        public decimal Price { get; set; }

        public bool IsEnabled { get; set; } = true;

        public bool RequiresPickupPoint { get; set; }

        public bool SupportsCashOnDelivery { get; set; }

        public int SortOrder { get; set; }
    }
}