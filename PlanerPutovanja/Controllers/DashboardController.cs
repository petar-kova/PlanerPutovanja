using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanerPutovanja.Models;

namespace PlanerPutovanja.Controllers;

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

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var now = DateTime.UtcNow;
        var today = now.Date;
        var firstMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-11);
        var nextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);

        var totalTripsTask = _context.Trips
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .CountAsync();

        var totalExpensesTask = _context.Expenses
            .AsNoTracking()
            .Where(e => e.Trip.UserId == userId)
            .SumAsync(e => (decimal?)e.Amount);

        var totalBudgetTask = _context.Trips
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Budget.HasValue)
            .SumAsync(t => (decimal?)t.Budget);

        var upcomingTripsTask = _context.Trips
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.StartDate >= today)
            .OrderBy(t => t.StartDate)
            .Select(t => new TripSummaryItem
            {
                TripId = t.Id,
                Name = t.Name,
                Destination = t.Destinations
                    .OrderBy(d => d.Order)
                    .Select(d => d.City)
                    .FirstOrDefault() ?? t.Destination,
                StartDate = t.StartDate,
                EndDate = t.EndDate
            })
            .Take(5)
            .ToListAsync();

        var monthlyRawTask = _context.Expenses
            .AsNoTracking()
            .Where(e => e.Trip.UserId == userId && e.Trip.StartDate >= firstMonth && e.Trip.StartDate < nextMonth)
            .GroupBy(e => new { e.Trip.StartDate.Year, e.Trip.StartDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Total = g.Sum(x => x.Amount)
            })
            .ToListAsync();

        var topDestinationsTask = _context.TripDestinations
            .AsNoTracking()
            .Where(td => td.Trip.UserId == userId)
            .GroupBy(td => td.City)
            .Select(g => new TopDestinationItem
            {
                City = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.City)
            .Take(7)
            .ToListAsync();

        var overBudgetTask = _context.Trips
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Budget.HasValue && t.Budget.Value > 0)
            .Select(t => new OverBudgetTripItem
            {
                TripId = t.Id,
                Name = t.Name,
                Budget = t.Budget!.Value,
                Expenses = t.Expenses.Sum(e => (decimal?)e.Amount) ?? 0m,
                UsagePercent = ((t.Expenses.Sum(e => (decimal?)e.Amount) ?? 0m) / t.Budget!.Value) * 100m
            })
            .Where(x => x.UsagePercent > 90m)
            .OrderByDescending(x => x.UsagePercent)
            .ThenBy(x => x.Name)
            .ToListAsync();

        await Task.WhenAll(
            totalTripsTask,
            totalExpensesTask,
            totalBudgetTask,
            upcomingTripsTask,
            monthlyRawTask,
            topDestinationsTask,
            overBudgetTask);

        var monthlyLookup = monthlyRawTask.Result
            .ToDictionary(x => (x.Year, x.Month), x => x.Total);

        var monthlyExpenses = Enumerable.Range(0, 12)
            .Select(offset => firstMonth.AddMonths(offset))
            .Select(month => new MonthlyExpensePoint
            {
                Label = month.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                Total = monthlyLookup.TryGetValue((month.Year, month.Month), out var total) ? total : 0m
            })
            .ToList();

        var totalExpenses = totalExpensesTask.Result ?? 0m;
        var totalBudget = totalBudgetTask.Result ?? 0m;
        var budgetUsagePercent = totalBudget <= 0m
            ? 0m
            : Math.Round((totalExpenses / totalBudget) * 100m, 2);

        var model = new DashboardViewModel
        {
            TotalTrips = totalTripsTask.Result,
            TotalExpenses = totalExpenses,
            TotalBudget = totalBudget,
            BudgetUsagePercent = budgetUsagePercent,
            UpcomingTrips = upcomingTripsTask.Result,
            MonthlyExpenses = monthlyExpenses,
            TopDestinations = topDestinationsTask.Result,
            OverBudgetTrips = overBudgetTask.Result
        };

        return View(model);
    }
}
