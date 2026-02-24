using System.Text.Json;

namespace PlanerPutovanja.Services;

public sealed class WeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public WeatherService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<WeatherInfo?> GetCurrentWeatherAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city)) return null;

        var apiKey = (_configuration["OpenWeather:ApiKey"] ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(apiKey)) return null;

        var client = _httpClientFactory.CreateClient("weather");
        var encodedCity = Uri.EscapeDataString(city.Trim());
        var response = await client.GetAsync($"weather?q={encodedCity}&units=metric&lang=hr&appid={apiKey}");

        if (!response.IsSuccessStatusCode) return null;

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var root = document.RootElement;
        var temp = root.GetProperty("main").GetProperty("temp").GetDecimal();
        var feelsLike = root.GetProperty("main").GetProperty("feels_like").GetDecimal();
        var weatherDescription = root.GetProperty("weather")[0].GetProperty("description").GetString() ?? "n/a";

        return new WeatherInfo(city, temp, feelsLike, weatherDescription);
    }

    public sealed record WeatherInfo(string City, decimal TemperatureC, decimal FeelsLikeC, string Description);
}
