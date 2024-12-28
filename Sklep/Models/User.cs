using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sklep.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(50, ErrorMessage = "Imię nie może przekraczać 50 znaków.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [StringLength(50, ErrorMessage = "Nazwisko nie może przekraczać 50 znaków.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format email.")]
        [StringLength(100, ErrorMessage = "Email nie może przekraczać 100 znaków.")]
        public string Email { get; set; }

        [StringLength(200, ErrorMessage = "Adres nie może przekraczać 200 znaków.")]
        public string Address { get; set; }

        // Nawigacja do zamówień
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
