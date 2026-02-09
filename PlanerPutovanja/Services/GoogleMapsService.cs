using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace PlanerPutovanja.Services
{
    public class GoogleMapsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly string _apiKey;

        public GoogleMapsService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;

            _apiKey = configuration["GoogleMaps:ApiKey"]
                ?? throw new InvalidOperationException("GoogleMaps:ApiKey missing (appsettings.json -> GoogleMaps:ApiKey).");
        }

        public async Task<RouteSummary> CalculateRouteAsync(List<string> locations)
        {
            if (locations == null || locations.Count < 2)
                return new RouteSummary { TotalDistanceKm = 0, TotalDurationMinutes = 0 };

            // Cache key (ordered)
            string cacheKey = "route_" + string.Join("->", locations).ToLowerInvariant();
            if (_cache.TryGetValue(cacheKey, out RouteSummary? cached) && cached != null)
                return cached;

            // Encode locations for URL
            var origins = string.Join("|", locations.Take(locations.Count - 1).Select(Uri.EscapeDataString));
            var destinations = string.Join("|", locations.Skip(1).Select(Uri.EscapeDataString));

            var url =
                "https://maps.googleapis.com/maps/api/distancematrix/json" +
                $"?origins={origins}&destinations={destinations}&mode=driving&units=metric&key={_apiKey}";

            var client = _httpClientFactory.CreateClient("gm");


            // IMPORTANT: if Google returns REQUEST_DENIED, this call will still deserialize,
            // but status won't be OK - we'll surface that as an exception below.
            var response = await client.GetFromJsonAsync<DistanceMatrixResponse>(url);

            if (response == null)
                throw new InvalidOperationException("Google Distance Matrix: null response.");

            if (!string.Equals(response.status, "OK", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Google Distance Matrix error: {response.status}");

            if (response.rows == null || response.rows.Count == 0)
                return new RouteSummary { TotalDistanceKm = 0, TotalDurationMinutes = 0 };

            double totalKm = 0;
            int totalMinutes = 0;

            // We expect: row i -> element i (A->B, B->C, C->D...)
            for (int i = 0; i < response.rows.Count; i++)
            {
                var row = response.rows[i];
                if (row?.elements == null || row.elements.Count == 0)
                    continue;

                // Uzimamo element [i] ako postoji (idealno),
                // a ako ne postoji, uzmi [0] kao fallback.
                var idx = Math.Min(i, row.elements.Count - 1);
                var element = row.elements[idx];

                if (element == null || !string.Equals(element.status, "OK", StringComparison.OrdinalIgnoreCase))
                    continue;

                var meters = element.distance?.value ?? 0;
                var seconds = element.duration?.value ?? 0;

                totalKm += meters / 1000.0;
                totalMinutes += (int)Math.Round(seconds / 60.0);
            }


            var result = new RouteSummary
            {
                TotalDistanceKm = Math.Round(totalKm, 2),
                TotalDurationMinutes = totalMinutes
            };

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
            return result;
        }

        public class RouteSummary
        {
            public double TotalDistanceKm { get; set; }
            public int TotalDurationMinutes { get; set; }
        }

        private class DistanceMatrixResponse
        {
            public string? status { get; set; }
            public List<Row> rows { get; set; } = new();

            public class Row
            {
                public List<Element> elements { get; set; } = new();
            }

            public class Element
            {
                public string? status { get; set; }
                public ValueText? distance { get; set; }
                public ValueText? duration { get; set; }
            }

            public class ValueText
            {
                public int value { get; set; }
            }
        }
    }
}
