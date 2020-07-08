using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using MyWeather;
using NWS;

namespace Server
{
    public class WeatherService : MyWeather.WeatherService.WeatherServiceBase
    {
        private readonly ILogger<WeatherService> _logger;
        private readonly IWeather _weather;

        public WeatherService(ILogger<WeatherService> logger, IWeather weather)
        {
            _logger = logger;
            _weather = weather;
        }

        public override async Task RequestStreamData(WeatherRequest request, IServerStreamWriter<WeatherDataReply> responseStream, ServerCallContext context)
        {
            while (true)
            {
                var weatherData = await GetWeatherData(request);
                await responseStream.WriteAsync(weatherData);
                _logger.LogInformation("Wait for the delay to pass");
                await Task.Delay(5 * 1000);// Delay 5 seconds for testing. Change to (3600 * 1000) for hourly queries
                
            }
        }

        private async Task<WeatherDataReply> GetWeatherData(WeatherRequest request)
        {
            var split = request.Location.Split(",");
            var latitude = split[0];
            var longitude = split[1];

            var temp = await _weather.GetWeather(latitude, longitude);

            var record = new WeatherDataReply
            {
                Temperature = temp,
                Location = request.Location,
                Windspeed = 0,
                Winddirection = 0

            };
            return record;
        }
    }
}