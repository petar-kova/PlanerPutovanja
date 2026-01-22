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

        public async Task<IActionResult> Create(int tripId)
        {
            var ownsTrip = await _context.Trips.AnyAsync(t => t.Id == tripId && t.UserId == CurrentUserId);
            if (!ownsTrip) return NotFound();

            return View(new TripActivity { TripId = tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int tripId, TripActivity activity)
        {
            var ownsTrip = await _context.Trips.AnyAsync(t => t.Id == tripId && t.UserId == CurrentUserId);
            if (!ownsTrip) return NotFound();

            activity.TripId = tripId;

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

        public async Task<IActionResult> Edit(int id)
        {
            var activity = await _context.Activities
                .Include(a => a.Trip)
                .FirstOrDefaultAsync(a => a.Id == id && a.Trip.UserId == CurrentUserId);

            if (activity == null) return NotFound();

            return View(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TripActivity activity)
        {
            if (id != activity.Id) return BadRequest();

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
