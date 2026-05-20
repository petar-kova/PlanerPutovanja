using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanerPutovanja.Models;
using PlanerPutovanja.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PlanerPutovanja.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly WeatherService _weatherService;

        public TripsController(ApplicationDbContext context, WeatherService weatherService)
        {
            _context = context;
            _weatherService = weatherService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public async Task<IActionResult> Index(string filter = "all")
        {
            IQueryable<Trip> query = _context.Trips.Where(t => t.UserId == CurrentUserId);

            var today = DateTime.Today;

            query = filter switch
            {
                "upcoming" => query.Where(t => t.StartDate > today),
                "past" => query.Where(t => t.EndDate < today),
                "current" => query.Where(t => t.StartDate <= today && t.EndDate >= today),
                _ => query
            };

            var trips = await query
                .Include(t => t.Activities)
                .Include(t => t.Expenses)
                .Include(t => t.Destinations)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            ViewBag.CurrentFilter = filter;
            return View(trips);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _context.Trips
    .Include(t => t.Activities)
    .Include(t => t.Expenses)
    .Include(t => t.Destinations)
    .Include(t => t.Albums)
        .ThenInclude(a => a.Photos)
    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();

            var cities = trip.Destinations?
    .OrderBy(d => d.Order)
    .Select(d => d.City)
    .Where(c => !string.IsNullOrWhiteSpace(c))
    .Select(c => c!.Trim())
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToList() ?? new List<string>();

            var weatherByCity = new Dictionary<string, WeatherService.WeatherInfo?>(StringComparer.OrdinalIgnoreCase);

            foreach (var city in cities)
            {
                weatherByCity[city] = await _weatherService.GetCurrentWeatherAsync(city);
            }

            ViewBag.WeatherByCity = weatherByCity;

            var topCity = cities.FirstOrDefault() ?? trip.Destination;
            ViewBag.Weather = await _weatherService.GetCurrentWeatherAsync(topCity);

            return View(trip);
        }

        public IActionResult Create(string? destination = null)
        {
            var trip = new Trip
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };

            if (!string.IsNullOrWhiteSpace(destination))
            {
                trip.Destination = destination.Trim();
                trip.Name = $"Putovanje u {destination.Trim()}";
            }

            return View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Trip trip)
        {
            trip.UserId = CurrentUserId;

            ModelState.Remove(nameof(Trip.UserId));
            var budgetRaw = Request.Form[nameof(Trip.Budget)].FirstOrDefault()
                            ?? Request.Form["Budget"].FirstOrDefault()
                            ?? "";
            trip.Budget = ParseBudget(budgetRaw);
            ModelState.Remove(nameof(Trip.Budget));

            if (!ModelState.IsValid)
                return View(trip);

            _context.Add(trip);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();
            return View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Trip trip)
        {
            if (id != trip.Id) return NotFound();

            var existingTrip = await _context.Trips
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (existingTrip == null) return NotFound();

            trip.UserId = CurrentUserId;

            ModelState.Remove(nameof(Trip.UserId));

            var budgetRaw = Request.Form[nameof(Trip.Budget)].FirstOrDefault()
                            ?? Request.Form["Budget"].FirstOrDefault()
                            ?? "";
            trip.Budget = ParseBudget(budgetRaw);
            ModelState.Remove(nameof(Trip.Budget));

            if (!ModelState.IsValid)
                return View(trip);

            _context.Update(trip);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(int id)
        {
            var trip = await _context.Trips
                .Include(t => t.Destinations)
                .Include(t => t.Activities)
                .Include(t => t.Expenses)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();

            var destinations = trip.Destinations
                .OrderBy(d => d.Order)
                .ToList();

            var activities = trip.Activities.ToList();
            var expenses = trip.Expenses
                .OrderByDescending(e => e.Id)
                .ToList();

            var totalExpenses = expenses.Sum(e => e.Amount);
            var tripDays = (trip.EndDate.Date - trip.StartDate.Date).Days + 1;
            if (tripDays < 1) tripDays = 1;

            var costPerDay = tripDays > 0 ? totalExpenses / tripDays : totalExpenses;

            var budgetText = trip.Budget.HasValue
                ? $"{trip.Budget.Value:0.00} €"
                : "Nije uneseno";

            var budgetStatus = "Budžet nije unesen";

            if (trip.Budget.HasValue && trip.Budget.Value > 0)
            {
                var percent = (totalExpenses / trip.Budget.Value) * 100m;
                budgetStatus = percent <= 100
                    ? $"Iskorišteno {percent:0.##}% budžeta"
                    : $"Budžet premašen za {(percent - 100):0.##}%";
            }

            var transportText = trip.Transport switch
            {
                Trip.TransportMode.Car => "Auto",
                Trip.TransportMode.Plane => "Avion",
                Trip.TransportMode.Train => "Vlak",
                Trip.TransportMode.Bus => "Autobus",
                Trip.TransportMode.CruiseShip => "Brod",
                _ => "Nije odabrano"
            };

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(32);
                    page.PageColor(Colors.White);

                    page.DefaultTextStyle(x => x
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken3));

                    page.Header().Element(header =>
                    {
                        header.Column(col =>
                        {
                            col.Item()
                                .Background(Colors.Blue.Darken4)
                                .Padding(22)
                                .Column(inner =>
                                {
                                    inner.Spacing(6);

                                    inner.Item().Text("PLANER PUTOVANJA")
                                        .FontSize(10)
                                        .FontColor(Colors.Teal.Lighten3)
                                        .SemiBold()
                                        .LetterSpacing(1.4f);

                                    inner.Item().Text(trip.Name)
                                        .FontSize(26)
                                        .FontColor(Colors.White)
                                        .Bold();

                                    inner.Item().Text($"{trip.Destination}  |  {trip.StartDate:dd.MM.yyyy} - {trip.EndDate:dd.MM.yyyy}")
                                        .FontSize(11)
                                        .FontColor(Colors.Grey.Lighten2);
                                });
                        });
                    });

                    page.Content().PaddingVertical(18).Column(col =>
                    {
                        col.Spacing(16);

                        col.Item().Row(row =>
                        {
                            row.Spacing(10);

                            row.RelativeItem().Element(c => StatCard(c, "Trajanje", $"{tripDays}", tripDays == 1 ? "dan" : "dana"));
                            row.RelativeItem().Element(c => StatCard(c, "Prijevoz", transportText, "način putovanja"));
                            row.RelativeItem().Element(c => StatCard(c, "Budžet", budgetText, budgetStatus));
                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(10);

                            row.RelativeItem().Element(c => StatCard(c, "Ukupni troškovi", $"{totalExpenses:0.00} €", "uneseno u plan"));
                            row.RelativeItem().Element(c => StatCard(c, "Trošak po danu", $"{costPerDay:0.00} €", "prosječno"));
                            row.RelativeItem().Element(c => StatCard(c, "Destinacije", $"{destinations.Count}", "točke putovanja"));
                        });

                        col.Item().Element(SectionTitle).Text("Pregled putovanja");

                        col.Item()
                            .Background(Colors.Grey.Lighten4)
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(14)
                            .Column(info =>
                            {
                                info.Spacing(6);
                                info.Item().Text($"Naziv putovanja: {trip.Name}").SemiBold();
                                info.Item().Text($"Glavna destinacija: {trip.Destination}");
                                info.Item().Text($"Razdoblje: {trip.StartDate:dd.MM.yyyy} - {trip.EndDate:dd.MM.yyyy}");
                                info.Item().Text($"Način prijevoza: {transportText}");
                            });

                        col.Item().Element(SectionTitle).Text("Destinacije");

                        if (destinations.Any())
                        {
                            col.Item().Column(list =>
                            {
                                list.Spacing(6);

                                var counter = 1;
                                foreach (var destination in destinations)
                                {
                                    list.Item()
                                        .Background(Colors.Teal.Lighten5)
                                        .BorderLeft(4)
                                        .BorderColor(Colors.Teal.Medium)
                                        .Padding(10)
                                        .Text($"{counter}. {destination.City}")
                                        .SemiBold();

                                    counter++;
                                }
                            });
                        }
                        else
                        {
                            col.Item().Element(EmptyBox).Text("Nema dodanih destinacija.");
                        }

                        col.Item().Element(SectionTitle).Text("Aktivnosti");

                        if (activities.Any())
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(3);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(TableHeaderCell).Text("Naziv");
                                    header.Cell().Element(TableHeaderCell).Text("Napomena");
                                });

                                foreach (var activity in activities)
                                {
                                    table.Cell().Element(TableCell).Text(activity.Name);
                                    table.Cell().Element(TableCell).Text(string.IsNullOrWhiteSpace(activity.Notes) ? "-" : activity.Notes);
                                }
                            });
                        }
                        else
                        {
                            col.Item().Element(EmptyBox).Text("Nema dodanih aktivnosti.");
                        }

                        col.Item().Element(SectionTitle).Text("Troškovi");

                        if (expenses.Any())
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(TableHeaderCell).Text("Naziv");
                                    header.Cell().Element(TableHeaderCell).Text("Opis");
                                    header.Cell().Element(TableHeaderCell).AlignRight().Text("Iznos");
                                });

                                foreach (var expense in expenses)
                                {
                                    table.Cell().Element(TableCell).Text(expense.Name);
                                    table.Cell().Element(TableCell).Text(string.IsNullOrWhiteSpace(expense.Description) ? "-" : expense.Description);
                                    table.Cell().Element(TableCell).AlignRight().Text($"{expense.Amount:0.00} €");
                                }

                                table.Cell().ColumnSpan(2).Element(TotalCell).AlignRight().Text("Ukupno");
                                table.Cell().Element(TotalCell).AlignRight().Text($"{totalExpenses:0.00} €");
                            });
                        }
                        else
                        {
                            col.Item().Element(EmptyBox).Text("Nema dodanih troškova.");
                        }

                        col.Item()
                            .Background(Colors.Blue.Lighten5)
                            .Border(1)
                            .BorderColor(Colors.Blue.Lighten3)
                            .Padding(12)
                            .Column(note =>
                            {
                                note.Spacing(4);

                                note.Item().Text("Napomena")
                                    .FontSize(11)
                                    .FontColor(Colors.Blue.Darken3)
                                    .SemiBold();

                                note.Item().Text("Ovaj dokument je automatski generiran iz aplikacije Planer Putovanja i služi kao sažetak spremljenog plana.")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken2);
                            });
                    });

                    page.Footer()
                        .BorderTop(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .PaddingTop(10)
                        .Row(row =>
                        {
                            row.RelativeItem().Text($"Generirano: {DateTime.Now:dd.MM.yyyy HH:mm}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Medium);

                            row.RelativeItem().AlignRight().Text(text =>
                            {
                                text.Span("Stranica ").FontSize(9).FontColor(Colors.Grey.Medium);
                                text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Medium);
                                text.Span(" / ").FontSize(9).FontColor(Colors.Grey.Medium);
                                text.TotalPages().FontSize(9).FontColor(Colors.Grey.Medium);
                            });
                        });
                });
            }).GeneratePdf();

            var safeName = string.Join("-", trip.Name.Split(Path.GetInvalidFileNameChars()));
            if (string.IsNullOrWhiteSpace(safeName))
                safeName = $"putovanje-{trip.Id}";

            return File(pdf, "application/pdf", $"{safeName}-plan-putovanja.pdf");

            static void StatCard(IContainer container, string label, string value, string description)
            {
                container
                    .Background(Colors.Grey.Lighten4)
                    .Border(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Padding(12)
                    .Column(col =>
                    {
                        col.Spacing(4);

                        col.Item().Text(label)
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1)
                            .SemiBold();

                        col.Item().Text(value)
                            .FontSize(15)
                            .FontColor(Colors.Blue.Darken4)
                            .Bold();

                        col.Item().Text(description)
                            .FontSize(8)
                            .FontColor(Colors.Grey.Medium);
                    });
            }

            static IContainer SectionTitle(IContainer container)
            {
                return container
                    .PaddingTop(4)
                    .PaddingBottom(6)
                    .BorderBottom(1)
                    .BorderColor(Colors.Teal.Medium);
            }

            static IContainer EmptyBox(IContainer container)
            {
                return container
                    .Background(Colors.Grey.Lighten4)
                    .Border(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Padding(12)
                    .DefaultTextStyle(x => x.FontColor(Colors.Grey.Medium).Italic());
            }

            static IContainer TableHeaderCell(IContainer container)
            {
                return container
                    .Background(Colors.Blue.Darken4)
                    .PaddingVertical(8)
                    .PaddingHorizontal(8)
                    .DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold());
            }

            static IContainer TableCell(IContainer container)
            {
                return container
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten3)
                    .PaddingVertical(8)
                    .PaddingHorizontal(8);
            }

            static IContainer TotalCell(IContainer container)
            {
                return container
                    .Background(Colors.Teal.Lighten5)
                    .BorderTop(1)
                    .BorderColor(Colors.Teal.Medium)
                    .PaddingVertical(9)
                    .PaddingHorizontal(8)
                    .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.Blue.Darken4));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trip = await _context.Trips
                .Include(t => t.Activities)
                .Include(t => t.Expenses)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (trip != null)
            {
                _context.Activities.RemoveRange(trip.Activities);
                _context.Expenses.RemoveRange(trip.Expenses);
                _context.Trips.Remove(trip);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private decimal? ParseBudget(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            input = input.Trim();

            input = input.Replace(" ", "");

            if (input.Contains('.') && input.Contains(','))
            {
                input = input.Replace(".", "");
                input = input.Replace(",", ".");
            }
            else
            {
                if (input.Contains(',') && !input.Contains('.'))
                    input = input.Replace(",", ".");
            }

            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                return value;

            return null;
        }
    }
}
