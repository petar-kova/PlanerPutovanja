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
        [CompareDates("StartDate", ErrorMessage = "End date cannot be earlier than start date.")]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1_000_000_000, ErrorMessage = "Budget must be a positive number.")]
        public decimal? Budget { get; set; }

        public string Currency { get; set; } = "EUR";

        public ICollection<TripActivity> Activities { get; set; } = new List<TripActivity>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();

        [Required]
        public string UserId { get; set; } = string.Empty;
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

    // Custom validation for EndDate ≥ StartDate
    public class CompareDatesAttribute : ValidationAttribute
    {
        private readonly string _startDateProperty;
        public CompareDatesAttribute(string startDateProperty)
        {
            _startDateProperty = startDateProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var startDateProp = validationContext.ObjectType.GetProperty(_startDateProperty);
            if (startDateProp == null) return ValidationResult.Success;

            var startDate = (DateTime)startDateProp.GetValue(validationContext.ObjectInstance)!;
            var endDate = (DateTime)value!;

            if (endDate < startDate)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
