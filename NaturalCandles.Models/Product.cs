using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NaturalCandles.Models.Enums;

namespace NaturalCandles.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        [Required]
        public string Name { get; set; }

        public ProductCategory Category { get; set; }

        [Required]
        public decimal BasePrice { get; set; }

        public bool AvailableNow { get; set; } // For fast shipping products

        public bool HasColorOption { get; set; } // true/false
        public bool HasTypeOption { get; set; } // true/false
        public bool HasColorSchemeOption { get; set; } // true/false

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
