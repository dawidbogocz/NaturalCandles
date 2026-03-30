using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace NaturalCandles.Models
{
    public class ProductPriceTier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        [ValidateNever]
        public Product Product { get; set; }

        [Required]
        [Range(1, 100000, ErrorMessage = "Minimum quantity must be at least 1.")]
        public int MinQuantity { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Display(Name = "Unit price")]
        public decimal Price { get; set; }
    }
}