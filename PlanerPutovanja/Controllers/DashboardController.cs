using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanerPutovanja.Models;

namespace PlanerPutovanja.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            var trips = _context.Trips
                .Where(t => t.UserId == userId)
                .ToList();

            var totalTrips = trips.Count;
            var upcomingTrips = trips.Count(t => t.StartDate > DateTime.Today);

            var totalExpenses = _context.Expenses
                .Where(e => e.Trip.UserId == userId)
                .Sum(e => (decimal?)e.Amount) ?? 0m;

            var model = new DashboardViewModel
            {
                TotalTrips = totalTrips,
                UpcomingTrips = upcomingTrips,
                TotalExpenses = totalExpenses
            };

            return View(model);
        }
    }

    public class DashboardViewModel
    {
        public int TotalTrips { get; set; }
        public int UpcomingTrips { get; set; }
        public decimal TotalExpenses { get; set; }
    }
}
