using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanerPutovanja.Models
{
    public class Expense
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Foreign key to Trip
        public int TripId { get; set; }
        public Trip Trip { get; set; } = null!;
    }
}
