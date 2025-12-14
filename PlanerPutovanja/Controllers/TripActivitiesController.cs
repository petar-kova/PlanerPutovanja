using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // GET: TripActivities/Create?tripId=1
        public IActionResult Create(int tripId)
        {
            ViewBag.TripId = tripId;
            return View();
        }

        // POST: TripActivities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TripActivity activity)
        {
            if (ModelState.IsValid)
            {
                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Trips", new { id = activity.TripId });
            }

            ViewBag.TripId = activity.TripId;
            return View(activity);
        }
    }
}
