using System.Threading;
using System.Threading.Tasks;

namespace DevPulse.Application.Abstractions
{
    /// <summary>
    /// Simple weather abstraction (OpenWeather-backed in Infrastructure).
    /// Returns the current temperature in Celsius, or null if unavailable.
    /// </summary>
    public interface IWeatherClient
    {
        Task<double?> GetCurrentTempCAsync(
            string city,
            string countryCode = "US",
            CancellationToken ct = default);
    }
}