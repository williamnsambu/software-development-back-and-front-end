using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using DevPulse.Application.Abstractions;
using DevPulse.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DevPulse.Infrastructure.Providers.OpenWeather;

public sealed class OpenWeatherClient : IWeatherClient
{
    private readonly HttpClient _http;
    private readonly ILogger<OpenWeatherClient> _log;
    private readonly OpenWeatherOptions _opt;

    public OpenWeatherClient(IHttpClientFactory f, IOptions<OpenWeatherOptions> opt, ILogger<OpenWeatherClient> log)
    {
        _http = f.CreateClient("openweather");
        _opt = opt.Value;
        _log = log;
    }

    // Very small call: current temperature in °C
    public async Task<double?> GetCurrentTempCAsync(string city, string countryCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_opt.ApiKey))
            return null;

        var url = $"weather?q={Uri.EscapeDataString(city)},{Uri.EscapeDataString(countryCode)}&appid={_opt.ApiKey}&units=metric";
        try
        {
            var json = await _http.GetFromJsonAsync<OpenWeatherResponse>(url, ct);
            return json?.Main?.Temp;
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "OpenWeather call failed");
            return null;
        }
    }

    private sealed class OpenWeatherResponse
    {
        public MainData? Main { get; set; }
        public sealed class MainData { public double Temp { get; set; } }
    }
}