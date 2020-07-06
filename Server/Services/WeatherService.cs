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
                await Task.Delay(5 * 1000);
                
            }
        }

        private async Task<WeatherDataReply> GetWeatherData(WeatherRequest request)
        {
            var split = request.Location.Split(",");
            var lat = split[0];
            var longi = split[1];

            var temp = await _weather.GetWeather(lat, longi);

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