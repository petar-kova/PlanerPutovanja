using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // Optional total budget (null = no budget)
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000000000)]
        public decimal? Budget { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "EUR";

        public ICollection<TripActivity> Activities { get; set; } = new List<TripActivity>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();

        // Foreign key to User
        [Required]
        public string UserId { get; set; } = null!;

        // Navigation (not posted from MVC forms)
        public User? User { get; set; }
    }
}
