using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NaturalCandles.Models.Enums;

namespace NaturalCandles.Models
{

    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        [MaxLength(50)]
        [DisplayName("Category Name")]
        public string Name { get; set; }
		[DisplayName("Category Type")]
		public ProductCategory CategoryType { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
