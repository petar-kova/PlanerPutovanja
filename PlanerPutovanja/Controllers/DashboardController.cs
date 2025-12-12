using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PlanerPutovanja.Models;
using Microsoft.EntityFrameworkCore;

namespace PlanerPutovanja.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var trips = await _context.Trips
                .Include(t => t.Activities)
                .Include(t => t.Expenses)
                .ToListAsync();

            return View(trips);
        }
    }
}
