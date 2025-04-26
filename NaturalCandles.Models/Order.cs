using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NaturalCandles.Models.Enums;

namespace NaturalCandles.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public bool IsConfirmedByManager { get; set; } = false;

        public ShippingMethod ShippingMethod { get; set; }

        public decimal ShippingCost { get; set; }

        public ICollection<OrderItem> Items { get; set; }

        public Payment Payment { get; set; }
    }
}
