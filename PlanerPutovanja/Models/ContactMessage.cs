using System.ComponentModel.DataAnnotations;

namespace PlanerPutovanja.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Nevažeći format email adrese.")]
        [StringLength(150)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Tema je obavezna.")]
        [StringLength(100)]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "Poruka je obavezna.")]
        [StringLength(2000)]
        public string Message { get; set; } = null!;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}
