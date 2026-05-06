using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PlanerPutovanja.Models;

namespace PlanerPutovanja.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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