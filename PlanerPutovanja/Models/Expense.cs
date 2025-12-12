using System.ComponentModel.DataAnnotations;

namespace PlanerPutovanja.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Category { get; set; } = null!;

        // Foreign key
        public int TripId { get; set; }
        public Trip Trip { get; set; } = null!;
    }
}
