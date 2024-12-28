using System.ComponentModel.DataAnnotations;

namespace Sklep.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Login jest wymagany.")]
        [StringLength(50)]
        public string Login { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Proszę potwierdzić hasło.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Hasła nie są takie same.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(200)]
        public string Address { get; set; }
    }
}
