using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaturalCandles.Models
{
    public enum CategoryType
    {
        AvailableNow,
        DecorativeCandles,
        FlowerBoxes,
        Bouquets,
        CandleDecorations
    }

    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public CategoryType CategoryType { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
