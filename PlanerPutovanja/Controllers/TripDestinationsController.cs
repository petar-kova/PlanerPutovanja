using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanerPutovanja.Models;

namespace PlanerPutovanja.Controllers
{
    [Authorize]
    public class TripDestinationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TripDestinationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int tripId, string destination)
        {
            if (string.IsNullOrWhiteSpace(destination))
                return RedirectToAction("Details", "Trips", new { id = tripId });

            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();

            var parts = destination
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var currentMax = await _context.Set<TripDestination>()
                .Where(d => d.TripId == tripId)
                .Select(d => (int?)d.Order)
                .MaxAsync() ?? 0;

            var nextOrder = currentMax + 1;

            foreach (var part in parts)
            {
                var city = part.Trim();
                if (string.IsNullOrWhiteSpace(city)) continue;

                var dest = new TripDestination
                {
                    TripId = tripId,
                    Order = nextOrder++,
                    Nights = 1,
                    City = city
                };

                _context.Add(dest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int tripId)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();

            var dest = await _context.Set<TripDestination>()
                .FirstOrDefaultAsync(d => d.Id == id && d.TripId == tripId);

            if (dest != null)
            {
                _context.Remove(dest);
                await _context.SaveChangesAsync();
            }

            // opcionalno: prepakiraj Order (1..N) da bude uredno
            await NormalizeOrder(tripId);

            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        // ===== REORDER ACTIONI (↑ ↓) =====

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveUp(int id, int tripId)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();

            var current = await _context.Set<TripDestination>()
                .FirstOrDefaultAsync(d => d.Id == id && d.TripId == tripId);

            if (current == null)
                return RedirectToAction("Details", "Trips", new { id = tripId });

            var prev = await _context.Set<TripDestination>()
                .Where(d => d.TripId == tripId && d.Order < current.Order)
                .OrderByDescending(d => d.Order)
                .FirstOrDefaultAsync();

            if (prev == null)
                return RedirectToAction("Details", "Trips", new { id = tripId });

            (current.Order, prev.Order) = (prev.Order, current.Order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Trips", new { id = tripId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NightsPlus(int id, int tripId)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();

            var dest = await _context.Set<TripDestination>()
                .FirstOrDefaultAsync(d => d.Id == id && d.TripId == tripId);

            if (dest != null && dest.Nights < 30)
            {
                dest.Nights += 1;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NightsMinus(int id, int tripId)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();

            var dest = await _context.Set<TripDestination>()
                .FirstOrDefaultAsync(d => d.Id == id && d.TripId == tripId);

            if (dest != null && dest.Nights > 1)
            {
                dest.Nights -= 1;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveDown(int id, int tripId)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();

            var current = await _context.Set<TripDestination>()
                .FirstOrDefaultAsync(d => d.Id == id && d.TripId == tripId);

            if (current == null)
                return RedirectToAction("Details", "Trips", new { id = tripId });

            var next = await _context.Set<TripDestination>()
                .Where(d => d.TripId == tripId && d.Order > current.Order)
                .OrderBy(d => d.Order)
                .FirstOrDefaultAsync();

            if (next == null)
                return RedirectToAction("Details", "Trips", new { id = tripId });

            (current.Order, next.Order) = (next.Order, current.Order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        private async Task NormalizeOrder(int tripId)
        {
            var items = await _context.Set<TripDestination>()
                .Where(d => d.TripId == tripId)
                .OrderBy(d => d.Order)
                .ThenBy(d => d.Id)
                .ToListAsync();

            var order = 1;
            foreach (var item in items)
                item.Order = order++;

            await _context.SaveChangesAsync();
        }
    }
}
