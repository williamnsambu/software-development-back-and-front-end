using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DevPulse.Application.Abstractions;
using DevPulse.Application.Options;
using Microsoft.Extensions.Options;

namespace DevPulse.Infrastructure.Services.OpenWeather;

public sealed class OpenWeatherClient : IWeatherClient
{
    private readonly HttpClient _http;
    private readonly OpenWeatherOptions _options;

    public OpenWeatherClient(HttpClient http, IOptions<OpenWeatherOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<double?> GetCurrentTempCAsync(string city, string countryCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            return null; // no key configured

        var url = $"https://api.openweathermap.org/data/2.5/weather?q={city},{countryCode}&appid={_options.ApiKey}&units=metric";

        using var resp = await _http.GetAsync(url, ct);
        if (!resp.IsSuccessStatusCode) return null;

        using var stream = await resp.Content.ReadAsStreamAsync(ct);

        try
        {
            var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var temp = doc.RootElement.GetProperty("main").GetProperty("temp").GetDouble();
            return temp;
        }
        catch
        {
            return null;
        }
    }
}