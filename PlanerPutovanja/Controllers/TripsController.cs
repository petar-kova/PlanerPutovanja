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
        public async Task<IActionResult> Index(string filter = "all")
        {
            IQueryable<Trip> query = _context.Trips
                .Where(t => t.UserId == CurrentUserId);

            var today = DateTime.Today;

            switch (filter?.ToLower())
            {
                case "upcoming":
                    // tripovi koji još nisu počeli
                    query = query.Where(t => t.StartDate > today);
                    break;

                case "past":
                    // tripovi koji su završili
                    query = query.Where(t => t.EndDate < today);
                    break;

                case "inprogress":
                    // tripovi koji trenutno traju
                    query = query.Where(t => t.StartDate <= today && t.EndDate >= today);
                    break;

                default:
                    // "all" ili nepoznata vrijednost -> bez dodatnog filtra
                    break;
            }

            var trips = await query
                .Include(t => t.Activities)
                .Include(t => t.Expenses)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            ViewBag.CurrentFilter = filter;
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
