using System.ComponentModel.DataAnnotations;

namespace Sklep.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Login jest wymagany.")]
        [StringLength(50)]
        public string Login { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
