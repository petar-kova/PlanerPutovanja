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

        public async Task<IActionResult> Index(string filter = "all")
        {
            IQueryable<Trip> query = _context.Trips
                .Where(t => t.UserId == CurrentUserId);

            var today = DateTime.Today;

            if (filter == "upcoming")
            {
                query = query.Where(t => t.StartDate > today);
            }
            else if (filter == "past")
            {
                query = query.Where(t => t.EndDate < today);
            }
            else if (filter == "current")
            {
                query = query.Where(t => t.StartDate <= today && t.EndDate >= today);
            }

            var trips = await query
                .Include(t => t.Activities)
                .Include(t => t.Expenses)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            ViewBag.Filter = filter;
            return View(trips);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _context.Trips
                .Include(t => t.Activities)
                .Include(t => t.Expenses)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();
            return View(trip);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Location,StartDate,EndDate,Transport,Accommodation,Budget")] Trip trip)
        {
            trip.UserId = CurrentUserId;

            // IGNORIRAJ validaciju za Budget
            ModelState.Remove("Budget");

            if (ModelState.IsValid)
            {
                _context.Add(trip);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(trip);
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _context.Trips.FindAsync(id);
            if (trip == null || trip.UserId != CurrentUserId) return NotFound();

            return View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Trip trip)
        {
            if (id != trip.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTrip = await _context.Trips.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                    if (existingTrip == null || existingTrip.UserId != CurrentUserId)
                        return NotFound();

                    _context.Update(trip);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Trips.Any(e => e.Id == trip.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(trip);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _context.Trips
                .Include(t => t.Activities)
                .Include(t => t.Expenses)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();
            return View(trip);
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
    }
}
