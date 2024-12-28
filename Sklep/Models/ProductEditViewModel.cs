using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Sklep.Models
{
    public class ProductEditViewModel
    {
        public int Id { get; set; } 

        [Required(ErrorMessage = "Nazwa produktu jest wymagana.")]
        [StringLength(200, ErrorMessage = "Nazwa produktu nie może przekraczać 200 znaków.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Opis produktu nie może przekraczać 500 znaków.")]
        public string Description { get; set; }

        [Display(Name = "Kategoria")]
        [Required(ErrorMessage = "Kategoria jest wymagana.")]
        public int CategoryId { get; set; } 

        [Display(Name = "Adres URL Zdjęcia")]
        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; }

        [BindNever]
        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; }

        // Pole do wprowadzenia nowej kategorii
        [Display(Name = "Nowa Kategoria")]
        public string NewCategoryName { get; set; }
    }
}
