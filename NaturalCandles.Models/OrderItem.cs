using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaturalCandles.Models
{
    public enum CandleSize { Small, Medium, Large }
    public enum CandleColor { White, Red, Green, Blue }
    public enum CandleFragrance { Vanilla, Lavender, Citrus, Rose }

    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public CandleSize Size { get; set; }
        public CandleColor Color { get; set; }
        public CandleFragrance Fragrance { get; set; }
        public string ContainerType { get; set; }
        public string LabelText { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
