using System.ComponentModel.DataAnnotations;

namespace PlanerPutovanja.Models
{
    public class TripActivity
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public string? Notes { get; set; }

        // Foreign key to Trip
        public int TripId { get; set; }
        public Trip Trip { get; set; } = null!;
    }
}
