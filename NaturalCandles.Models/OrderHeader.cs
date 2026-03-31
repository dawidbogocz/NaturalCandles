using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NaturalCandles.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NaturalCandles.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime? ShippingDate { get; set; }

        [Range(0, 999999)]
        public decimal DeliveryPrice { get; set; }

        [Range(0, 999999)]
        public decimal OrderTotal { get; set; }

        public string Currency { get; set; } = "PLN";

        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }

        public DateTime? PaymentDate { get; set; }
        public DateOnly? PaymentDueDate { get; set; }

        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }
        public ShippingMethod? ShippingMethod { get; set; }

        [StringLength(100)]
        public string? DeliveryPointId { get; set; }

        [StringLength(200)]
        public string? DeliveryPointName { get; set; }

        [Required]
        [StringLength(120)]
        public string Name { get; set; }

        [Required]
        [Phone]
        [StringLength(30)]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [StringLength(200)]
        public string StreetAddress { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [Required]
        [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Use Polish postal code format: 00-000")]
        public string PostalCode { get; set; }
    }
}