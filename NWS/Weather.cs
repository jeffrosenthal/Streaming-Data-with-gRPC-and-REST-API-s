using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NWS
{
    public interface IWeather
    {
        Task<int> GetWeather();
        Task<int> GetWeather(string lat, string longitude);
        Task<string> GetGridUrl();
        string Latitude { get; set; }
        string Longitude { get; set; }
    }

    public class Weather : IWeather
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _client;

        public Weather(IHttpClientFactory factory) 
        {
            
            _httpClientFactory = factory;
            _client = _httpClientFactory.CreateClient();
        }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public string Url => string.Format("https://api.weather.gov/points/{0},{1}", Latitude, Longitude);

        public string GridUrl { get; set; }

        public async Task<int> GetWeather(string latitude, string longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            GridUrl = await GetGridUrl();
            return await GetWeather();
        }
        public async Task<int> GetWeather()
        {
            //Added for reference
            //https://api.weather.gov/gridpoints/tbw/70,97/forecast/hourly?units=us

            int temp = 0;
            try
            {
                //Now we have the url specific for our grid, lets tell it what we want and get it
                var forecastUrl = $"{GridUrl}/forecast/hourly";


                //The NWS requires User-Agent to be specified. If not, the operation is forbidden
                _client.DefaultRequestHeaders.Add("User-Agent", "MyApplication");

                var forecast = await _client.GetStringAsync(forecastUrl);

                //We have the data, now lets pull it apart so we can get the temperature
                var fx = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(forecast);
                var properties = fx["properties"].ToString();

                //Continuing to drill into the properties
                var data = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(properties);
                string periods = data["periods"].ToString();
                periods = periods.ToString().Replace("[", "").Replace("]", "");
                var subperiod1 = periods.Substring(periods.IndexOf(",", StringComparison.Ordinal) + 2);
                var subperiod2 = subperiod1.Substring(0, subperiod1.IndexOf("},", StringComparison.Ordinal) - 2);

                //Split up the current data into a dictionary that we can access
                var splits = subperiod2.Split(',');


                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                foreach (string s in splits)
                {
                    try
                    {
                        //first clean up the data
                        //var d = s.Trim().Trim('\"');
                        var splitagain = s.Split(':');
                        
                        //make sure the data is in a format convertible to kvp
                        if(splitagain.Length != 2)
                            continue;

                        //now put it into the kvp (after we trim it up some and make it pretty
                        keyValuePairs.Add(splitagain[0].Trim().Trim('\"'), splitagain[1].Trim().Trim('\"'));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        
                    }

                }
                temp = int.Parse(keyValuePairs["temperature"]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return temp;
            
        }

        public async Task<string> GetGridUrl()
        {
            //Query to get the grid that NWS uses to track weather
            _client.DefaultRequestHeaders.Add("User-Agent", "MyApplication");
            string getString;
            try
            {
                getString = await _client.GetStringAsync(Url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            //Now extract the information that we need to query the forecast
            var deserialize = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(getString);
            var properties = deserialize["properties"].ToString();
            var data = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(properties);
            var gridUrl = data["forecastGridData"].ToString();
            return gridUrl;
        }
    }
}
