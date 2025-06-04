using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NaturalCandles.Models.Enums;

namespace NaturalCandles.Models
{

    public class Category
    {
        [Key]
        [Required]
        [MaxLength(50)]
        [DisplayName("Category Name")]
        public string CategoryName { get; set; }

        [ValidateNever]
        public ICollection<Product> Products { get; set; }
    }
}
