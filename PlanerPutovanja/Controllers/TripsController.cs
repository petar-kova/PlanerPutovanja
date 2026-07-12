using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanerPutovanja.Models;
using PlanerPutovanja.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PlanerPutovanja.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly WeatherService _weatherService;

        public TripsController(ApplicationDbContext context, WeatherService weatherService)
        {
            _context = context;
            _weatherService = weatherService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public async Task<IActionResult> Index(string filter = "all")
        {
            IQueryable<Trip> query = _context.Trips.Where(t => t.UserId == CurrentUserId);

            var today = DateTime.Today;

            query = filter switch
            {
                "upcoming" => query.Where(t => t.StartDate > today),
                "past" => query.Where(t => t.EndDate < today),
                "current" => query.Where(t => t.StartDate <= today && t.EndDate >= today),
                _ => query
            };

            var trips = await query
                .Include(t => t.Activities)
                .Include(t => t.Expenses)
                .Include(t => t.Destinations)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            ViewBag.CurrentFilter = filter;
            return View(trips);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _context.Trips
    .Include(t => t.Activities)
    .Include(t => t.Expenses)
    .Include(t => t.Destinations)
    .Include(t => t.Albums)
        .ThenInclude(a => a.Photos)
    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();

            var cities = trip.Destinations?
    .OrderBy(d => d.Order)
    .Select(d => d.City)
    .Where(c => !string.IsNullOrWhiteSpace(c))
    .Select(c => c!.Trim())
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToList() ?? new List<string>();

            var weatherByCity = new Dictionary<string, WeatherService.WeatherInfo?>(StringComparer.OrdinalIgnoreCase);

            foreach (var city in cities)
            {
                weatherByCity[city] = await _weatherService.GetCurrentWeatherAsync(city);
            }

            ViewBag.WeatherByCity = weatherByCity;

            var topCity = cities.FirstOrDefault() ?? trip.Destination;
            ViewBag.Weather = await _weatherService.GetCurrentWeatherAsync(topCity);

            return View(trip);
        }

        public IActionResult Create(string? destination = null)
        {
            var trip = new Trip
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };

            if (!string.IsNullOrWhiteSpace(destination))
            {
                trip.Destination = destination.Trim();
                trip.Name = $"Putovanje u {destination.Trim()}";
            }

            return View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Trip trip)
        {
            trip.UserId = CurrentUserId;

            ModelState.Remove(nameof(Trip.UserId));
            trip.Budget = ParseBudgetFromForm();
            ModelState.Remove(nameof(Trip.Budget));

            if (!ModelState.IsValid)
                return View(trip);

            _context.Add(trip);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();
            return View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Trip trip)
        {
            if (id != trip.Id) return NotFound();

            var existingTrip = await _context.Trips
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (existingTrip == null) return NotFound();

            trip.UserId = CurrentUserId;

            ModelState.Remove(nameof(Trip.UserId));
            trip.Budget = ParseBudgetFromForm();
            ModelState.Remove(nameof(Trip.Budget));

            if (!ModelState.IsValid)
                return View(trip);

            _context.Update(trip);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(int id)
        {
            var trip = await _context.Trips
                .Include(t => t.Activities)
                .Include(t => t.Expenses)
                .Include(t => t.Destinations)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip == null)
                return NotFound();

            var pdfService = new TripPdfService();
            var pdfBytes = pdfService.GenerateTripPdf(trip);

            var safeFileName = SanitizeFileName(trip.Name ?? "putovanje");

            return File(pdfBytes, "application/pdf", $"{safeFileName}-premium.pdf");
        }
        private string SanitizeFileName(string fileName)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '-');
            }

            return fileName.Replace(" ", "-").ToLowerInvariant();
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trip = await _context.Trips
                .Include(t => t.Activities)
                .Include(t => t.Expenses)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip != null)
            {
                _context.Activities.RemoveRange(trip.Activities);
                _context.Expenses.RemoveRange(trip.Expenses);
                _context.Trips.Remove(trip);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private decimal? ParseBudgetFromForm()
        {
            var raw = Request.Form[nameof(Trip.Budget)].FirstOrDefault()
                      ?? Request.Form["Budget"].FirstOrDefault()
                      ?? "";
            return ParseBudget(raw);
        }

        private decimal? ParseBudget(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            input = input.Trim();

            input = input.Replace(" ", "");

            if (input.Contains('.') && input.Contains(','))
            {
                input = input.Replace(".", "");
                input = input.Replace(",", ".");
            }
            else
            {
                if (input.Contains(',') && !input.Contains('.'))
                    input = input.Replace(",", ".");
            }

            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                return value;

            return null;
        }
    }
}
