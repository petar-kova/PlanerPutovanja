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
            var tripExists = await _db.Trips
                .AnyAsync(t => t.Id == tripId && t.UserId == CurrentUserId);

            if (!tripExists)
            {
                return NotFound(new
                {
                    error = "Putovanje nije pronađeno."
                });
            }

            var destinations = await _db.Set<TripDestination>()
                .Where(d => d.TripId == tripId)
                .OrderBy(d => d.Order)
                .ToListAsync();

            var stops = destinations
                .Select(d => d.City)
                .Where(city => !string.IsNullOrWhiteSpace(city))
                .Select(city => city.Trim())
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
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }
    }
}