using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlanerPutovanja.Models
{
    public class Trip
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Destination { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<TripActivity> Activities { get; set; } = new List<TripActivity>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();

        // Foreign key to User (ovo mora biti popunjeno kod spremanja)
        [Required]
        public string UserId { get; set; } = null!;

        // Navigacija NE treba biti required u MVC formi (ne dolazi iz forme)
        public User? User { get; set; }
    }
}
