using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sklep.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa kategorii jest wymagana.")]
        [StringLength(100, ErrorMessage = "Nazwa kategorii nie może przekraczać 100 znaków.")]
        public string Name { get; set; }

        // Nawigacyjna właściwość do produktów
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
