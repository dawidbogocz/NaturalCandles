using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NaturalCandles.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; } = new();

        [ValidateNever]
        public IEnumerable<SelectListItem> CategoryList { get; set; } = Enumerable.Empty<SelectListItem>();

        public List<ProductPriceTier> PriceTiers { get; set; } = new();
    }
}