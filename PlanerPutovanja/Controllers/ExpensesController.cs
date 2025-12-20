using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanerPutovanja.Models;

namespace PlanerPutovanja.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: Expenses/Create?tripId=1
        public async Task<IActionResult> Create(int tripId)
        {
            var ownsTrip = await _context.Trips.AnyAsync(t => t.Id == tripId && t.UserId == CurrentUserId);
            if (!ownsTrip) return NotFound();

            return View(new Expense { TripId = tripId });
        }

        // POST: Expenses/Create?tripId=1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int tripId, Expense expense)
        {
            var ownsTrip = await _context.Trips.AnyAsync(t => t.Id == tripId && t.UserId == CurrentUserId);
            if (!ownsTrip) return NotFound();

            // FK comes from route/query, not from form
            expense.TripId = tripId;

            // Navigation not posted
            ModelState.Remove(nameof(Expense.Trip));

            if (!ModelState.IsValid)
                return View(expense);

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _context.Expenses
                .Include(e => e.Trip)
                .FirstOrDefaultAsync(e => e.Id == id && e.Trip.UserId == CurrentUserId);

            if (expense == null) return NotFound();

            return View(expense);
        }

        // POST: Expenses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expense expense)
        {
            if (id != expense.Id) return BadRequest();

            // Don't trust posted FK/navigation
            ModelState.Remove(nameof(Expense.TripId));
            ModelState.Remove(nameof(Expense.Trip));

            if (!ModelState.IsValid) return View(expense);

            var expenseFromDb = await _context.Expenses
                .Include(e => e.Trip)
                .FirstOrDefaultAsync(e => e.Id == id && e.Trip.UserId == CurrentUserId);

            if (expenseFromDb == null) return NotFound();

            expenseFromDb.Name = expense.Name;
            expenseFromDb.Description = expense.Description;
            expenseFromDb.Amount = expense.Amount;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Trips", new { id = expenseFromDb.TripId });
        }

        // POST: Expenses/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _context.Expenses
                .Include(e => e.Trip)
                .FirstOrDefaultAsync(e => e.Id == id && e.Trip.UserId == CurrentUserId);

            if (expense == null) return NotFound();

            var tripId = expense.TripId;

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Trips", new { id = tripId });
        }
    }
}
