using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sklep.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa produktu jest wymagana.")]
        [StringLength(200, ErrorMessage = "Nazwa produktu nie może przekraczać 200 znaków.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Opis produktu nie może przekraczać 500 znaków.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Kategoria jest wymagana.")]
public int? CategoryId { get; set; }


        // Nawigacyjna właściwość do kategorii
        public Category Category { get; set; }

        [Display(Name = "Adres URL Zdjęcia")]
        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; }

        // Nawigacyjna właściwość do OrderProduct
        public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    }
}
