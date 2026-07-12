using System.ComponentModel.DataAnnotations;

namespace PlanerPutovanja.Models
{
    public class TripAlbum
    {
        public int Id { get; set; }

        [Required]
        public int TripId { get; set; }

        public Trip Trip { get; set; } = null!;

        [Required]
        [StringLength(80)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1500)]
        public string? Review { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [StringLength(255)]
        public string? CoverImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TripPhoto> Photos { get; set; } = new List<TripPhoto>();
    }
}