using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Sklep.Models
{
    public class OrderEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Data zamówienia jest wymagana.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data Zamówienia")]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Użytkownik jest wymagany.")]
        [Display(Name = "Użytkownik")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Musisz wybrać co najmniej jeden produkt.")]
        [Display(Name = "Produkty")]
        public List<int> SelectedProductIds { get; set; } = new List<int>();

        [BindNever]
        public List<SelectListItem> UsersSelectList { get; set; } = new List<SelectListItem>();

        [BindNever]
        public List<SelectListItem> ProductsSelectList { get; set; } = new List<SelectListItem>();
    }
}
