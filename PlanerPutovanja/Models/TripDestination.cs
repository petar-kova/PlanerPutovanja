using System.ComponentModel.DataAnnotations;

namespace PlanerPutovanja.Models
{
    public class TripDestination
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City name must be at most 100 characters.")]
        public string City { get; set; } = null!;

        [Range(1, 30, ErrorMessage = "Nights must be at least 1 and at most 30.")]
        public int Nights { get; set; } = 1;

        // Redoslijed destinacije u putovanju (1,2,3...)
        public int Order { get; set; }

        [Required]
        public int TripId { get; set; }

        public Trip Trip { get; set; } = null!;

        public override string ToString() => City;
    }
}
