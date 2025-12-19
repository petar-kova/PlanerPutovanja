using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanerPutovanja.Models;

namespace PlanerPutovanja.Controllers
{
    [Authorize]
    public class TripActivitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TripActivitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: TripActivities/Create?tripId=1
        public async Task<IActionResult> Create(int tripId)
        {
            var ownsTrip = await _context.Trips.AnyAsync(t => t.Id == tripId && t.UserId == CurrentUserId);
            if (!ownsTrip) return NotFound();

            return View(new TripActivity { TripId = tripId });
        }

        // POST: TripActivities/Create?tripId=1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int tripId, TripActivity activity)
        {
            var ownsTrip = await _context.Trips.AnyAsync(t => t.Id == tripId && t.UserId == CurrentUserId);
            if (!ownsTrip) return NotFound();

            // Force FK from route/query, not from form
            activity.TripId = tripId;

            // Trip navigation isn't posted -> remove from validation
            ModelState.Remove(nameof(TripActivity.Trip));
            ModelState.Remove(nameof(TripActivity.TripId));

            if (!ModelState.IsValid)
            {
                activity.TripId = tripId;
                return View(activity);
            }

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        // GET: TripActivities/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var activity = await _context.Activities
                .Include(a => a.Trip)
                .FirstOrDefaultAsync(a => a.Id == id && a.Trip.UserId == CurrentUserId);

            if (activity == null) return NotFound();

            return View(activity);
        }

        // POST: TripActivities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TripActivity activity)
        {
            if (id != activity.Id) return BadRequest();

            // Don't trust posted FK/navigation
            ModelState.Remove(nameof(TripActivity.TripId));
            ModelState.Remove(nameof(TripActivity.Trip));

            if (!ModelState.IsValid) return View(activity);

            var activityFromDb = await _context.Activities
                .Include(a => a.Trip)
                .FirstOrDefaultAsync(a => a.Id == id && a.Trip.UserId == CurrentUserId);

            if (activityFromDb == null) return NotFound();

            activityFromDb.Name = activity.Name;
            activityFromDb.Notes = activity.Notes;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Trips", new { id = activityFromDb.TripId });
        }

        // POST: TripActivities/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var activity = await _context.Activities
                .Include(a => a.Trip)
                .FirstOrDefaultAsync(a => a.Id == id && a.Trip.UserId == CurrentUserId);

            if (activity == null) return NotFound();

            var tripId = activity.TripId;

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Trips", new { id = tripId });
        }
    }
}
