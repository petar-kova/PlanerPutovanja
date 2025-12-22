using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanerPutovanja.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Expense name is required.")]
        [StringLength(100, ErrorMessage = "Expense name must be at most 100 characters long.")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description must be at most 500 characters long.")]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 1_000_000_000, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        // Foreign key to Trip
        [Required]
        public int TripId { get; set; }

        public Trip Trip { get; set; } = null!;
    }
}
