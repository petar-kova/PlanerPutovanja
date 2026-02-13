using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanerPutovanja.Models;
using PlanerPutovanja.Services;

namespace PlanerPutovanja.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/routes")]
    public class RoutesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly GoogleMapsService _googleMaps;

        public RoutesApiController(ApplicationDbContext db, GoogleMapsService googleMaps)
        {
            _db = db;
            _googleMaps = googleMaps;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        
        [HttpGet("trip/{tripId:int}/summary")]
        public async Task<IActionResult> GetTripRouteSummary(int tripId)
        {
            
            var tripOk = await _db.Trips.AnyAsync(t => t.Id == tripId && t.UserId == CurrentUserId);
            if (!tripOk) return NotFound();

            var q = _db.Set<TripDestination>()
                       .Where(d => d.TripId == tripId);

            List<TripDestination> destinations;

            
            try
            {
                destinations = await q.OrderBy(d => EF.Property<int>(d, "Order")).ToListAsync();
            }
            catch
            {
                destinations = await q.OrderBy(d => d.Id).ToListAsync();
            }

            if (destinations.Count < 2)
            {
                return Ok(new GoogleMapsService.RouteSummary
                {
                    TotalDistanceKm = 0,
                    TotalDurationMinutes = 0
                });
            }

            var stops = destinations
                .Select(GetLocationText)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (stops.Count < 2)
            {
                return Ok(new GoogleMapsService.RouteSummary
                {
                    TotalDistanceKm = 0,
                    TotalDurationMinutes = 0
                });
            }

            try
            {
                var result = await _googleMaps.CalculateRouteAsync(stops);
                return Ok(result);
            }
            catch (Exception ex)
            {
                
                return BadRequest(new { error = ex.Message });
            }
        }

        private static string GetLocationText(TripDestination d)
        {
            var candidates = new[]
            {
                "Name", "Destination", "DestinationName", "City", "Location", "Naziv", "Mjesto"
            };

            var type = d.GetType();

            foreach (var propName in candidates)
            {
                var prop = type.GetProperty(propName);
                if (prop != null && prop.PropertyType == typeof(string))
                {
                    var value = prop.GetValue(d) as string;
                    if (!string.IsNullOrWhiteSpace(value))
                        return value!;
                }
            }

            return d.ToString() ?? "";
        }
    }
}
