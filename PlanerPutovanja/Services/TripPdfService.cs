using System.Globalization;
using System.Reflection;
using PlanerPutovanja.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PlanerPutovanja.Services
{
    public class TripPdfService
    {
        public byte[] GenerateTripPdf(Trip trip)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var tripName = GetString(trip, "Name", "Title") ?? "Putovanje";
            var description = GetString(trip, "Description", "Notes", "Summary") ?? "Nema dodatnog opisa putovanja.";
            var startDate = GetDate(trip, "StartDate", "DateFrom", "DepartureDate");
            var endDate = GetDate(trip, "EndDate", "DateTo", "ReturnDate");
            var plannedBudget = GetDecimal(trip, "Budget", "PlannedBudget", "TotalBudget");

            var destinations = (trip.Destinations ?? new List<TripDestination>()).Cast<object>().ToList();
            var activities = (trip.Activities ?? new List<TripActivity>()).Cast<object>().ToList();
            var expenses = (trip.Expenses ?? new List<Expense>()).Cast<object>().ToList();

            var totalExpenses = expenses.Sum(e => GetDecimal(e, "Amount", "Cost", "Price"));
            var remainingBudget = plannedBudget - totalExpenses;
            var totalDays = startDate.HasValue && endDate.HasValue
                ? Math.Max((endDate.Value.Date - startDate.Value.Date).Days + 1, 1)
                : 0;

            var itinerary = startDate.HasValue && endDate.HasValue
                ? $"{startDate.Value:dd.MM.yyyy} - {endDate.Value:dd.MM.yyyy}"
                : "Datumi nisu definirani";

            var expenseStatusText = remainingBudget >= 0 ? "Unutar budžeta" : "Prekoračen budžet";
            var expenseStatusColor = remainingBudget >= 0 ? "#0F766E" : "#B91C1C";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10.5f).FontColor("#0F172A"));

                    page.Header().PaddingBottom(8).Row(row =>
                    {
                        row.RelativeItem().Text(text =>
                        {
                            text.Span("PlanerPutovanja").SemiBold().FontColor("#0F766E");
                            text.Span(" • Premium Travel Brief").FontColor("#64748B");
                        });

                        row.ConstantItem(120).AlignRight().Text(text =>
                        {
                            text.Span("Generirano: ").FontColor("#64748B");
                            text.Span(DateTime.Now.ToString("dd.MM.yyyy")).SemiBold();
                        });
                    });

                    page.Content().Column(column =>
                    {
                        column.Spacing(16);

                        column.Item().Element(c => ComposeHero(c,
                            tripName,
                            itinerary,
                            description,
                            expenseStatusText,
                            expenseStatusColor));

                        column.Item().Row(row =>
                        {
                            row.Spacing(10);

                            row.RelativeItem().Element(c => ComposeStatCard(c, "Trajanje", totalDays > 0 ? $"{totalDays} dana" : "Nedefinirano", "#0F766E"));
                            row.RelativeItem().Element(c => ComposeStatCard(c, "Destinacije", destinations.Count.ToString(), "#0284C7"));
                            row.RelativeItem().Element(c => ComposeStatCard(c, "Aktivnosti", activities.Count.ToString(), "#7C3AED"));
                            row.RelativeItem().Element(c => ComposeStatCard(c, "Troškovi", expenses.Count.ToString(), "#EA580C"));
                        });

                        column.Item().Element(c => ComposeBudgetOverview(c,
                            plannedBudget,
                            totalExpenses,
                            remainingBudget));

                        column.Item().Element(c => ComposeTripSummary(c, tripName, itinerary, description));

                        column.Item().Element(c => ComposeDestinationsSection(c, destinations));
                        column.Item().Element(c => ComposeActivitiesSection(c, activities));
                        column.Item().Element(c => ComposeExpensesSection(c, expenses, totalExpenses, remainingBudget));
                    });

                    page.Footer().PaddingTop(8).BorderTop(1).BorderColor("#E2E8F0").Row(row =>
                    {
                        row.RelativeItem().Text(text =>
                        {
                            text.Span("PlanerPutovanja • ").FontColor("#64748B");
                            text.Span("Travel planner export").FontColor("#94A3B8");
                        });

                        row.ConstantItem(120).AlignRight().Text(text =>
                        {
                            text.Span("Stranica ").FontColor("#64748B");
                            text.CurrentPageNumber();
                            text.Span(" / ").FontColor("#64748B");
                            text.TotalPages();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHero(IContainer container, string title, string itinerary, string description, string statusText, string statusColor)
        {
            container
                .Background("#0F172A")
                .Border(1)
                .BorderColor("#1E293B")
                .CornerRadius(22)
                .Padding(24)
                .Column(column =>
                {
                    column.Spacing(10);

                    column.Item().Text(text =>
                    {
                        text.Span("PREMIUM ITINERARY").SemiBold().FontSize(10).FontColor("#67E8F9");
                    });

                    column.Item().Text(text =>
                    {
                        text.Span(title).Bold().FontSize(26).FontColor(Colors.White);
                    });

                    column.Item().Text(text =>
                    {
                        text.Span(itinerary).SemiBold().FontColor("#CBD5E1");
                    });

                    column.Item().Text(text =>
                    {
                        text.Span(description).FontColor("#E2E8F0");
                    });

                    column.Item().PaddingTop(6).AlignLeft().Element(c =>
                    {
                        c.Background(statusColor)
                         .CornerRadius(999)
                         .PaddingVertical(6)
                         .PaddingHorizontal(12)
                         .Text(statusText)
                         .FontColor(Colors.White)
                         .SemiBold()
                         .FontSize(9);
                    });
                });
        }

        private void ComposeStatCard(IContainer container, string label, string value, string accent)
        {
            container
                .Background("#F8FAFC")
                .Border(1)
                .BorderColor("#E2E8F0")
                .CornerRadius(16)
                .Padding(16)
                .Column(column =>
                {
                    column.Spacing(4);

                    column.Item().Text(label).FontColor("#64748B").SemiBold().FontSize(9);
                    column.Item().Text(value).Bold().FontSize(18).FontColor(accent);
                });
        }

        private void ComposeBudgetOverview(IContainer container, decimal plannedBudget, decimal totalExpenses, decimal remainingBudget)
        {
            container
                .Background(Colors.White)
                .Border(1)
                .BorderColor("#E2E8F0")
                .CornerRadius(18)
                .Padding(20)
                .Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Text("Budget Overview").Bold().FontSize(16).FontColor("#0F172A");

                    column.Item().Row(row =>
                    {
                        row.Spacing(10);

                        row.RelativeItem().Element(c => ComposeBudgetMiniCard(c, "Planirani budžet", FormatCurrency(plannedBudget), "#0F766E"));
                        row.RelativeItem().Element(c => ComposeBudgetMiniCard(c, "Ukupni troškovi", FormatCurrency(totalExpenses), "#EA580C"));
                        row.RelativeItem().Element(c => ComposeBudgetMiniCard(c, "Preostalo", FormatCurrency(remainingBudget), remainingBudget >= 0 ? "#0284C7" : "#B91C1C"));
                    });
                });
        }

        private void ComposeBudgetMiniCard(IContainer container, string label, string value, string color)
        {
            container
                .Background("#F8FAFC")
                .Border(1)
                .BorderColor("#E2E8F0")
                .CornerRadius(14)
                .Padding(14)
                .Column(column =>
                {
                    column.Spacing(4);
                    column.Item().Text(label).FontColor("#64748B").FontSize(9).SemiBold();
                    column.Item().Text(value).FontColor(color).FontSize(15).Bold();
                });
        }

        private void ComposeTripSummary(IContainer container, string tripName, string itinerary, string description)
        {
            ComposeSectionCard(container, "Sažetak putovanja", section =>
            {
                section.Column(column =>
                {
                    column.Spacing(8);
                    column.Item().Text($"Naziv: {tripName}").SemiBold();
                    column.Item().Text($"Termin: {itinerary}");
                    column.Item().Text($"Opis: {description}");
                });
            });
        }

        private void ComposeDestinationsSection(IContainer container, List<object> destinations)
        {
            ComposeSectionCard(container, "Destinacije", section =>
            {
                if (!destinations.Any())
                {
                    section.Text("Nema dodanih destinacija.").FontColor("#64748B");
                    return;
                }

                section.Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2.3f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.2f);
                    });

                    table.Header(header =>
                    {
                        HeaderCell(header.Cell(), "Naziv");
                        HeaderCell(header.Cell(), "Lokacija");
                        HeaderCell(header.Cell(), "Tip");
                    });

                    foreach (var destination in destinations)
                    {
                        BodyCell(table.Cell(), GetString(destination, "Name", "DestinationName", "City", "Title") ?? "Destinacija");
                        BodyCell(table.Cell(), GetString(destination, "Country", "Location", "Region", "Address") ?? "-");
                        BodyCell(table.Cell(), GetString(destination, "Type", "Category") ?? "-");
                    }
                });
            });
        }

        private void ComposeActivitiesSection(IContainer container, List<object> activities)
        {
            ComposeSectionCard(container, "Aktivnosti", section =>
            {
                if (!activities.Any())
                {
                    section.Text("Nema dodanih aktivnosti.").FontColor("#64748B");
                    return;
                }

                section.Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.8f);
                        columns.RelativeColumn(2.8f);
                        columns.RelativeColumn(1.2f);
                    });

                    table.Header(header =>
                    {
                        HeaderCell(header.Cell(), "Aktivnost");
                        HeaderCell(header.Cell(), "Opis");
                        HeaderCell(header.Cell(), "Datum");
                    });

                    foreach (var activity in activities)
                    {
                        var name = GetString(activity, "Name", "Title") ?? "Aktivnost";
                        var description = GetString(activity, "Description", "Notes") ?? "-";
                        var date = GetDate(activity, "ActivityDate", "Date", "StartDate");

                        BodyCell(table.Cell(), name);
                        BodyCell(table.Cell(), description);
                        BodyCell(table.Cell(), date.HasValue ? date.Value.ToString("dd.MM.yyyy") : "-");
                    }
                });
            });
        }

        private void ComposeExpensesSection(IContainer container, List<object> expenses, decimal totalExpenses, decimal remainingBudget)
        {
            ComposeSectionCard(container, "Troškovi", section =>
            {
                if (!expenses.Any())
                {
                    section.Text("Nema evidentiranih troškova.").FontColor("#64748B");
                    return;
                }

                section.Column(column =>
                {
                    column.Spacing(10);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.1f);
                        });

                        table.Header(header =>
                        {
                            HeaderCell(header.Cell(), "Trošak");
                            HeaderCell(header.Cell(), "Iznos");
                            HeaderCell(header.Cell(), "Datum");
                        });

                        foreach (var expense in expenses)
                        {
                            var name = GetString(expense, "Name", "Title", "Category", "Description") ?? "Trošak";
                            var amount = GetDecimal(expense, "Amount", "Cost", "Price");
                            var date = GetDate(expense, "ExpenseDate", "Date", "CreatedAt");

                            BodyCell(table.Cell(), name);
                            BodyCell(table.Cell(), FormatCurrency(amount));
                            BodyCell(table.Cell(), date.HasValue ? date.Value.ToString("dd.MM.yyyy") : "-");
                        }
                    });

                    column.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem();
                        row.ConstantItem(220).Background("#F8FAFC").Border(1).BorderColor("#E2E8F0").CornerRadius(14).Padding(12).Column(col =>
                        {
                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Ukupno troškovi").SemiBold();
                                r.ConstantItem(90).AlignRight().Text(FormatCurrency(totalExpenses)).Bold();
                            });

                            col.Item().PaddingTop(6).Row(r =>
                            {
                                r.RelativeItem().Text("Preostali budžet").SemiBold();
                                r.ConstantItem(90).AlignRight().Text(FormatCurrency(remainingBudget)).Bold().FontColor(remainingBudget >= 0 ? "#0F766E" : "#B91C1C");
                            });
                        });
                    });
                });
            });
        }

        private void ComposeSectionCard(IContainer container, string title, Action<IContainer> content)
        {
            container
                .Background(Colors.White)
                .Border(1)
                .BorderColor("#E2E8F0")
                .CornerRadius(18)
                .Padding(20)
                .Column(column =>
                {
                    column.Spacing(12);
                    column.Item().Text(title).Bold().FontSize(16).FontColor("#0F172A");
                    column.Item().Element(content);
                });
        }

        private void HeaderCell(IContainer container, string text)
        {
            container
                .Background("#EFF6FF")
                .BorderBottom(1)
                .BorderColor("#BFDBFE")
                .PaddingVertical(8)
                .PaddingHorizontal(10)
                .Text(text)
                .SemiBold()
                .FontSize(9)
                .FontColor("#0F172A");
        }

        private void BodyCell(IContainer container, string text)
        {
            container
                .BorderBottom(1)
                .BorderColor("#E2E8F0")
                .PaddingVertical(8)
                .PaddingHorizontal(10)
                .Text(text ?? "-")
                .FontSize(9.5f)
                .FontColor("#334155");
        }

        private string? GetString(object? source, params string[] propertyNames)
        {
            if (source == null) return null;

            foreach (var propertyName in propertyNames)
            {
                var property = source.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property == null) continue;

                var value = property.GetValue(source);
                if (value == null) continue;

                var stringValue = value.ToString();
                if (!string.IsNullOrWhiteSpace(stringValue))
                    return stringValue;
            }

            return null;
        }

        private DateTime? GetDate(object? source, params string[] propertyNames)
        {
            if (source == null) return null;

            foreach (var propertyName in propertyNames)
            {
                var property = source.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property == null) continue;

                var value = property.GetValue(source);
                if (value == null) continue;

                if (value is DateTime dateTime)
                    return dateTime;

                if (DateTime.TryParse(value.ToString(), out var parsed))
                    return parsed;
            }

            return null;
        }

        private decimal GetDecimal(object? source, params string[] propertyNames)
        {
            if (source == null) return 0m;

            foreach (var propertyName in propertyNames)
            {
                var property = source.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property == null) continue;

                var value = property.GetValue(source);
                if (value == null) continue;

                if (value is decimal d) return d;
                if (value is double db) return Convert.ToDecimal(db);
                if (value is float f) return Convert.ToDecimal(f);
                if (value is int i) return i;
                if (value is long l) return l;

                if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedInvariant))
                    return parsedInvariant;

                if (decimal.TryParse(value.ToString(), NumberStyles.Any, new CultureInfo("hr-HR"), out var parsedHr))
                    return parsedHr;
            }

            return 0m;
        }

        private string FormatCurrency(decimal value)
        {
            return string.Format(new CultureInfo("hr-HR"), "{0:N2} €", value);
        }
    }
}