using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Weatherio.Models;

namespace Weatherio.Core {
    public static class GeoWeatherHelper {
        private const string LocationUrl = "http://ip-api.com/json";
        private const string WeatherUrlStart = "https://api.weather.yandex.ru/v1/forecast?lat=";
        private const string WeatherUrlEnd = "&limit=1&lon=";

        private const string YandexApiKey = "Your Key";
        private const string YandexApiKeyHeaderName = "X-Yandex-API-Key";

        private const string CurrentWeatherResponseKey = "fact";
        private const string TemperatureResponseKey = "temp";
        private const string ConditionResponseKey = "condition";
        private const string FeelResponseKey = "feels_like";
        private const string PressureResponseKey = "pressure_mm";
        private const string WaterResponseKey = "temp_water";
        private const string WindResponseKey = "wind_speed";

        private static LocationResponse _location;

        public static async Task<WeatherInfo> GetWeather() {
            if (_location == null)
                await GetCurrentLocation();
            if (_location == null)
                return null;

            string resultJson;
            var request =
                (HttpWebRequest) WebRequest.Create(WeatherUrlStart + _location.Lat + WeatherUrlEnd + _location.Lon);
            request.Headers.Add(YandexApiKeyHeaderName, YandexApiKey);
            try {
                using (var response = (HttpWebResponse) await request.GetResponseAsync())
                using (var stream = response.GetResponseStream()) {
                    if (stream == null)
                        return null;
                    using (var reader = new StreamReader(stream)) {
                        resultJson = await reader.ReadToEndAsync();
                    }
                }
            }
            catch (Exception) {
                return null;
            }

            return MakeWeatherInfoFromJson(resultJson);
        }

        private static async Task GetCurrentLocation() {
            string resultJson;
            var request = (HttpWebRequest) WebRequest.Create(LocationUrl);
            try {
                using (var response = (HttpWebResponse) await request.GetResponseAsync())
                using (var stream = response.GetResponseStream()) {
                    if (stream == null)
                        return;
                    using (var reader = new StreamReader(stream)) {
                        resultJson = await reader.ReadToEndAsync();
                    }
                }
            }
            catch (Exception) {
                return;
            }

            _location = JsonConvert.DeserializeObject<LocationResponse>(resultJson);
        }

        private static WeatherInfo MakeWeatherInfoFromJson(string json) {
            try {
                var factInfo = JObject.Parse(json)[CurrentWeatherResponseKey];
                return new WeatherInfo {
                    Condition = factInfo[ConditionResponseKey].ToString(),
                    Temp = factInfo[TemperatureResponseKey].ToString(),
                    FeelsLike = factInfo[FeelResponseKey].ToString(),
                    TempWater = factInfo[WaterResponseKey].ToString(),
                    WindSpeed = factInfo[WindResponseKey].ToString(),
                    Pressure = factInfo[PressureResponseKey].ToString()
                };
            }
            catch (Exception) {
                return null;
            }
        }
    }
}