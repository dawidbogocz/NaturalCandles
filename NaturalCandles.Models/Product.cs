using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NaturalCandles.Models.Enums;

namespace NaturalCandles.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public string CategoryName { get; set; }
        
        [ForeignKey("CategoryName")]
        [ValidateNever]
        public Category Category { get; set; }

        public string Description { get; set; }

        [Required]
		[Display(Name = "Base price (1-4)")]
		public decimal BasePrice { get; set; }
		[Required]
        [Display (Name = "Price for 4+")]
		public decimal Price4 { get; set; }
		[Required]
		[Display(Name = "Price for 10+")]
		public decimal Price10 { get; set; }

		public bool AvailableNow { get; set; } // For fast shipping products

        public bool HasColorOption { get; set; } // true/false
        public bool HasTypeOption { get; set; } // true/false
        public bool HasColorSchemeOption { get; set; } // true/false
		[ValidateNever]
		public string ImageUrl { get;set; }

        [ValidateNever]
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
