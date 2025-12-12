using Microsoft.AspNetCore.Identity;

namespace PlanerPutovanja.Models
{
    public class User : IdentityUser
    {
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
