using System.ComponentModel.DataAnnotations;

namespace PlanerPutovanja.Models
{
    public class TripPhoto
    {
        public int Id { get; set; }

        [Required]
        public int TripAlbumId { get; set; }

        public TripAlbum TripAlbum { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string ImagePath { get; set; } = string.Empty;

        [StringLength(120)]
        public string? Caption { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}