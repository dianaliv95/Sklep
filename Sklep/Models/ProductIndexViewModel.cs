using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sklep.Models
{
    public class ProductIndexViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public SelectList Categories { get; set; }
        public int? SelectedCategoryId { get; set; }
    }
}
