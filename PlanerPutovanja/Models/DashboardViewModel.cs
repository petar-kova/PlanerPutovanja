namespace PlanerPutovanja.Models;

public class DashboardViewModel
{
    public int TotalTrips { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal BudgetUsagePercent { get; set; }
    public List<TripSummaryItem> UpcomingTrips { get; set; } = new();
    public List<MonthlyExpensePoint> MonthlyExpenses { get; set; } = new();
    public List<TopDestinationItem> TopDestinations { get; set; } = new();
    public List<OverBudgetTripItem> OverBudgetTrips { get; set; } = new();
}

public class TripSummaryItem
{
    public int TripId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class MonthlyExpensePoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class TopDestinationItem
{
    public string City { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class OverBudgetTripItem
{
    public int TripId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public decimal Expenses { get; set; }
    public decimal UsagePercent { get; set; }
}
