using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanerPutovanja.Models
{
    public class Trip
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Trip name is required.")]
        [StringLength(100, ErrorMessage = "Trip name must be at most 100 characters long.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Destination is required.")]
        [StringLength(100, ErrorMessage = "Destination must be at most 100 characters long.")]
        public string Destination { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        // Optional total budget (null = no budget)
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1_000_000_000, ErrorMessage = "Budget must be a positive number.")]
        public decimal? Budget { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(3, ErrorMessage = "Currency code must be 3 characters.")]
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
