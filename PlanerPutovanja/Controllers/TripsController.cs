using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanerPutovanja.Models;

namespace PlanerPutovanja.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TripsController(ApplicationDbContext context)
        {
            _context = context;
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
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();
            return View(trip);
        }

        public IActionResult Create()
        {
            return View(new Trip
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Trip trip)
        {
            trip.UserId = CurrentUserId;

            ModelState.Remove(nameof(Trip.UserId));            var budgetRaw = Request.Form[nameof(Trip.Budget)].FirstOrDefault()
                            ?? Request.Form["Budget"].FirstOrDefault()
                            ?? "";
            trip.Budget = ParseBudget(budgetRaw);
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

            var budgetRaw = Request.Form[nameof(Trip.Budget)].FirstOrDefault()
                            ?? Request.Form["Budget"].FirstOrDefault()
                            ?? "";
            trip.Budget = ParseBudget(budgetRaw);
            ModelState.Remove(nameof(Trip.Budget));

            if (!ModelState.IsValid)
                return View(trip);

            _context.Update(trip);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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

        private decimal? ParseBudget(string? budgetInput)
        {
            if (string.IsNullOrWhiteSpace(budgetInput)) return null;

            budgetInput = budgetInput.Trim().Replace(" ", "").Replace(",", ".");

            return decimal.TryParse(budgetInput, NumberStyles.Any, CultureInfo.InvariantCulture, out var budget)
                ? budget
                : null;
        }
    }
}
