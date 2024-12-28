using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sklep.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        // Nawigacyjna właściwość do użytkownika
        public int UserId { get; set; }
        public User User { get; set; }

        // Nawigacyjna właściwość do OrderProduct
        public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    }
}
