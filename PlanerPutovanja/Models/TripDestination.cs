using System.ComponentModel.DataAnnotations;

namespace PlanerPutovanja.Models
{
    public class TripDestination
    {
        public int Id { get; set; }

        [Required]
        public string City { get; set; } = null!;

        public int Order { get; set; }

        [Range(0, 30)]
        public int Nights { get; set; }

        public int TripId { get; set; }
        public Trip Trip { get; set; } = null!;
    }
}
