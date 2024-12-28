using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sklep.Models
{
    public class OrderCreateViewModel
    {
        [Required(ErrorMessage = "Użytkownik jest wymagany.")]
        [Display(Name = "Użytkownik")]
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Musisz wybrać co najmniej jeden produkt.")]
        [Display(Name = "Produkty")]
        public List<int> SelectedProductIds { get; set; } = new List<int>();

        public List<SelectListItem> UsersSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProductsSelectList { get; set; } = new List<SelectListItem>();
    }
}
