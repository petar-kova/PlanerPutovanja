using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanerPutovanja.Models;

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

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            var userTrips = _context.Trips.Where(t => t.UserId == CurrentUserId);

            var totalTrips = await userTrips.CountAsync();

            var totalExpenses = await _context.Expenses
                .Where(e => e.Trip.UserId == CurrentUserId)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;

            var totalBudget = await userTrips
                .SumAsync(t => (decimal?)t.Budget) ?? 0m;

            var budgetUsagePercent = totalBudget <= 0m
                ? 0m
                : (totalExpenses / totalBudget) * 100m;

            var today = DateTime.Today;

            var upcomingTrips = await userTrips
                .Where(t => t.StartDate >= today)
                .OrderBy(t => t.StartDate)
                .Select(t => new TripSummaryItem
                {
                    TripId = t.Id,
                    Name = t.Name,
                    Destination = t.Destination,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate
                })
                .Take(5)
                .ToListAsync();

            var expensesByTripRaw = await userTrips
                .Select(t => new
                {
                    t.Name,
                    Total = t.Expenses.Sum(e => (decimal?)e.Amount) ?? 0m
                })
                .OrderByDescending(x => x.Total)
                .Take(12)
                .ToListAsync();

            var monthlyExpenses = expensesByTripRaw
                .Select(x => new MonthlyExpensePoint
                {
                    Label = x.Name,
                    Total = x.Total
                })
                .ToList();

            var topDestinations = await _context.TripDestinations
                .Where(d => d.Trip.UserId == CurrentUserId)
                .GroupBy(d => d.City)
                .Select(g => new TopDestinationItem
                {
                    City = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(7)
                .ToListAsync();

            var overBudgetRaw = await userTrips
                .Where(t => t.Budget.HasValue && t.Budget.Value > 0m)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    Budget = t.Budget!.Value,
                    Expenses = t.Expenses.Sum(e => (decimal?)e.Amount) ?? 0m
                })
                .ToListAsync();

            var overBudgetTrips = overBudgetRaw
                .Select(t => new OverBudgetTripItem
                {
                    TripId = t.Id,
                    Name = t.Name,
                    Budget = t.Budget,
                    Expenses = t.Expenses,
                    UsagePercent = (t.Expenses / t.Budget) * 100m
                })
                .Where(x => x.UsagePercent >= 90m)
                .OrderByDescending(x => x.UsagePercent)
                .Take(10)
                .ToList();

            var vm = new DashboardViewModel
            {
                TotalTrips = totalTrips,
                TotalExpenses = totalExpenses,
                TotalBudget = totalBudget,
                BudgetUsagePercent = budgetUsagePercent,
                UpcomingTrips = upcomingTrips,
                MonthlyExpenses = monthlyExpenses,
                TopDestinations = topDestinations,
                OverBudgetTrips = overBudgetTrips
            };

            return View(vm);
        }
    }
}