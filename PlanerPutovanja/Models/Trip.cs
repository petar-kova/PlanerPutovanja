using System.ComponentModel.DataAnnotations;

namespace PlanerPutovanja.Models
{
    public class Trip
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Destination { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        // Navigation
        public ICollection<TripActivity> Activities { get; set; } = new List<TripActivity>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
