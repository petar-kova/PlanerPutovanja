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

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: Trips
        public async Task<IActionResult> Index()
        {
            var trips = await _context.Trips
                .Where(t => t.UserId == CurrentUserId)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            return View(trips);
        }

        // GET: Trips/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var trip = await _context.Trips
                .Include(t => t.Activities)
                .Include(t => t.Expenses)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();

            return View(trip);
        }

        // GET: Trips/Create
        public IActionResult Create() => View();

        // POST: Trips/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Trip trip)
        {
            trip.UserId = CurrentUserId;
            trip.User = null;

            ModelState.Remove(nameof(Trip.UserId));
            ModelState.Remove(nameof(Trip.User));

            // date validation
            if (trip.EndDate < trip.StartDate)
            {
                ModelState.AddModelError(nameof(Trip.EndDate), "End date must be on or after start date.");
            }

            if (!ModelState.IsValid) return View(trip);

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Trips/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();

            return View(trip);
        }

        // POST: Trips/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Trip trip)
        {
            if (id != trip.Id) return BadRequest();

            ModelState.Remove(nameof(Trip.UserId));
            ModelState.Remove(nameof(Trip.User));

            // date validation
            if (trip.EndDate < trip.StartDate)
            {
                ModelState.AddModelError(nameof(Trip.EndDate), "End date must be on or after start date.");
            }

            if (!ModelState.IsValid) return View(trip);

            var tripFromDb = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (tripFromDb == null) return NotFound();

            tripFromDb.Name = trip.Name;
            tripFromDb.Destination = trip.Destination;
            tripFromDb.StartDate = trip.StartDate;
            tripFromDb.EndDate = trip.EndDate;
            tripFromDb.Budget = trip.Budget;
            tripFromDb.Currency = trip.Currency;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Trips/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();

            return View(trip);
        }

        // POST: Trips/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip != null)
            {
                _context.Trips.Remove(trip);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
