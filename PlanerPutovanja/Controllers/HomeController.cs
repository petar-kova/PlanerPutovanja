using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PlanerPutovanja.Models;

namespace PlanerPutovanja.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Destinacije()
        {
            return View();
        }

        public IActionResult Planer()
        {
            return View();
        }

        public IActionResult Budzet()
        {
            return View();
        }

        public IActionResult Galerija()
        {
            return View();
        }

        public IActionResult Kontakt()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kontakt(ContactMessage contactMessage)
        {
            if (!ModelState.IsValid)
                return View(contactMessage);

            contactMessage.SentAt = DateTime.UtcNow;
            contactMessage.IsRead = false;

            _context.ContactMessages.Add(contactMessage);
            await _context.SaveChangesAsync();

            TempData["ContactSuccess"] = true;
            return RedirectToAction(nameof(Kontakt));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
