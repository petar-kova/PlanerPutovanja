using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanerPutovanja.Models
{
    public class Trip
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Trip name is required.")]
        [StringLength(100, ErrorMessage = "Trip name must be at most 100 characters long.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Destination is required.")]
        [StringLength(100, ErrorMessage = "Destination must be at most 100 characters long.")]
        public string Destination { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1_000_000_000, ErrorMessage = "Budget must be a positive number.")]
        public decimal? Budget { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(3, ErrorMessage = "Currency code must be 3 characters.")]
        public string Currency { get; set; } = "EUR";

        public ICollection<TripActivity> Activities { get; set; } = new List<TripActivity>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();

 
        [Required]
        public string UserId { get; set; } = null!;


        public User? User { get; set; }
        public List<TripDestination> Destinations { get; set; } = new();

        public enum TransportMode
        {
            NotSelected = 0,
            Car = 1,
            Plane = 2,
            Train = 3,
            Bus = 4,
            CruiseShip = 5,
            Other = 6
        }

        public TransportMode Transport { get; set; }
        public int? DrivingDistanceKm { get; set; }

        public bool IsCruise => Transport == TransportMode.CruiseShip;
    }
}

