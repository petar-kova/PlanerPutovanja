using System.ComponentModel.DataAnnotations;

namespace PlanerPutovanja.Models
{
    public class TripActivity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Activity name is required.")]
        [StringLength(100, ErrorMessage = "Activity name must be at most 100 characters long.")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Notes must be at most 500 characters long.")]
        public string? Notes { get; set; }

        // Foreign key to Trip
        [Required]
        public int TripId { get; set; }

        public Trip Trip { get; set; } = null!;
    }
}
