using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;

namespace ApiMetaweatherTest.HttpClientUtilites
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum WeatherStates
	{
		[EnumMember(Value = "sn")]
		Snow,

		[EnumMember(Value = "sl")]
		Sleet,

		[EnumMember(Value = "h")]
		Hail,

		[EnumMember(Value = "t")]
		Thunderstorm,

		[EnumMember(Value = "hr")]
		HeavyRain,

		[EnumMember(Value = "lr")]
		LightRain,

		[EnumMember(Value = "s")]
		Showers,

		[EnumMember(Value = "hc")]
		HeavyCloud,

		[EnumMember(Value = "lc")]
		LightCloud,

		[EnumMember(Value = "c")]
		Clear,
	}

	class ControllerClientMetaweather
	{
		private const string _UrlApiForSearchLocationByQuery		= "https://www.metaweather.com/api/location/search/?query={0}";
		private const string _UrlApiForSearchLocationByWoeid		= "https://www.metaweather.com/api/location/{0}/";
		private const string _UrlApiForSearchLocationByWoeidAndDate = "https://www.metaweather.com/api/location/{0}/{1}/{2}/{3}/";

		public List<Location> GetLocationsByQuery(string query)
		{
			List<Location> locations = new List<Location>();

			string url = String.Format(_UrlApiForSearchLocationByQuery, query);

			string responseString = GetGETRequest(url);
			locations = JsonConvert.DeserializeObject<List<Location>>(responseString);

			return locations;
		}
		public Location GetLocationByQuery(string query)
			=> GetLocationsByQuery(query)
				.First(local => local.Title == query);

		public void GetWidthAndLongitude(string query, out float width, out float longitude)
		{
			var widthAndLongitude = GetLocationByQuery(query)
				.LattLong
				.Split(',');

			width		= float.Parse(widthAndLongitude[0], CultureInfo.InvariantCulture.NumberFormat);
			longitude	= float.Parse(widthAndLongitude[1], CultureInfo.InvariantCulture.NumberFormat);
		}

		public Forecast GetToDayForecastByQuery(string query)
			=> GetNearestForecasts(query).First();

		public List<Forecast> GetNearestForecasts(string query)
		{
			List<Forecast> forecasts = new List<Forecast>();

			uint woeid = GetWoeidByQuery(query);
			string url = String.Format(_UrlApiForSearchLocationByWoeid, woeid);

			string responseString = GetGETRequest(url);
			dynamic location = JsonConvert.DeserializeObject(responseString);
			forecasts = JsonConvert.DeserializeObject<List<Forecast>>(location.consolidated_weather.ToString());

			return forecasts;
		}

		public List<Forecast> GetCollectionForecastsOnDateByQuery(string query, DateTime searchDate)
		{
			List<Forecast> forecasts = new List<Forecast>();

			uint woeid = GetWoeidByQuery(query);
			string url = String.Format(_UrlApiForSearchLocationByWoeidAndDate,
				woeid,
				searchDate.Year,
				searchDate.Month,
				searchDate.Day
			);

			string responseString = GetGETRequest(url);
			forecasts = JsonConvert.DeserializeObject<List<Forecast>>(responseString);

			return forecasts;
		}

		private string GetGETRequest(string url)
		{
			string responseString = String.Empty;

			using (var client = new HttpClient())
			{
				var response = client.GetAsync(url).Result;
				if (response.IsSuccessStatusCode)
				{
					responseString = response.Content.ReadAsStringAsync().Result;
				}
			}

			return responseString;
		}

		private uint GetWoeidByQuery(string query)
			=> GetLocationByQuery(query).Woeid;
	}

	public struct Location
	{
		[JsonProperty("title")]
		public string Title;

		[JsonProperty("location_type")]
		public string LocationType;

		[JsonProperty("woeid")]
		public uint Woeid;

		[JsonProperty("latt_long")]
		public string LattLong;
	}

	public struct Forecast
	{
		[JsonProperty("id")]
		public string Id;

		[JsonProperty("weather_state_abbr")]
		public WeatherStates WeatherStateName;

		[JsonProperty("wind_direction_compass")]
		public string WindDirectionCompass;

		[JsonProperty("created")]
		public DateTime Created;

		[JsonProperty("applicable_date")]
		public DateTime ApplicableDate;

		[JsonProperty("min_temp")]
		public double MinTemp;

		[JsonProperty("max_temp")]
		public double MaxTemp;

		[JsonProperty("the_temp")]
		public double TheTemp;

		[JsonProperty("wind_speed")]
		public string WindSpeed;

		[JsonProperty("wind_direction")]
		public string WindDirection;

		[JsonProperty("air_pressure")]
		public string AirPressure;

		[JsonProperty("humidity")]
		public string Humidity;

		[JsonProperty("visibility")]
		public string Visibility;

		[JsonProperty("predictability")]
		public string Predictability;

	}
}
