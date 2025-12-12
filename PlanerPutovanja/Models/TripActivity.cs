using System.ComponentModel.DataAnnotations;

namespace PlanerPutovanja.Models
{
    public class TripActivity
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string? Notes { get; set; }

        [Required]
        public string Category { get; set; } = null!;

        // Foreign key
        public int TripId { get; set; }
        public Trip Trip { get; set; } = null!;
    }
}
