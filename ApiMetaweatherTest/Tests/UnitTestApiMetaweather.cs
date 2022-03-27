using ApiMetaweatherTest.HttpClientUtilites;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiMetaweatherTest
{
	public class UnitTestApiMetaweather
	{
		private const string _SearchCity = "Minsk";

		private ControllerClientMetaweather ClientMetaweather;

		[SetUp]
		public void Setup()
		{
			ClientMetaweather = new ControllerClientMetaweather();
		}

		[Test, Order(1)]
		public void TestSearchByQuery()
		{
			var locations = ClientMetaweather.GetLocationsByQuery("min");
			Assert.IsTrue(locations.Any(local => local.Title == _SearchCity), "Ñity is not found");
		}

		[Test, Order(2)]
		public void TestGeoCode()
		{
			const float widthMinsk = 53.9f;
			const float longitudeMinsk = 27.5667f;

			ClientMetaweather.GetWidthAndLongitude(_SearchCity,
				out float width,
				out float longitude
			);

			Assert.IsTrue(Math.Abs(widthMinsk - width) < 0.005, "Incorrect width");
			Assert.IsTrue(Math.Abs(longitudeMinsk - longitude) < 0.005, "Incorrect longitude");
		}

		[Test, Order(3)]
		public void TestGetToDayWeather()
		{
			Forecast forecast = ClientMetaweather.GetToDayForecastByQuery(_SearchCity);
			bool isCurrentDate = Math.Abs((forecast.ApplicableDate - DateTime.Now).TotalDays) < 1;
			Assert.IsTrue(isCurrentDate, "Could not get current weather forecast");
		}

		[Test, Order(4)]
		public void TestNearestForecastsOnTemperature()
		{
			const double springOrAutumnTempInterval = 10;
			List<Forecast> forecasts = ClientMetaweather.GetNearestForecasts(_SearchCity);

			int month = DateTime.Now.Month;
			bool isNowWinter = (month == 12 || month == 1 || month == 2);
			bool isNowSummer = (month == 6 || month == 7 || month == 8);
			bool isNowSpringOrAutumn = !(isNowWinter || isNowSummer);
			if (isNowWinter)
				Assert.IsTrue(forecasts.All(forecast => forecast.TheTemp < 0), "Not all forecasts are less than zero");
			else if (isNowSummer)
				Assert.IsTrue(forecasts.All(forecast => forecast.TheTemp > 0), "Not all forecasts are more than zero");
			else if(isNowSpringOrAutumn)
				Assert.IsTrue(forecasts.All(
					forecast => forecast.TheTemp < springOrAutumnTempInterval && forecast.TheTemp > -springOrAutumnTempInterval
				), "Not all forecasts are included in the interval");
		}

		[Test, Order(5)]
		public void TestComparisonForecastStatusWithPast()
		{
			Forecast toDayForecast = ClientMetaweather.GetToDayForecastByQuery(_SearchCity);
			DateTime pathDate = DateTime.Now.AddYears(-5);
			List<Forecast> patsForecasts = ClientMetaweather.GetCollectionForecastsOnDateByQuery(_SearchCity, pathDate);

			Assert.IsTrue(patsForecasts.Any(
				pathForecast => pathForecast.WeatherStateName == toDayForecast.WeatherStateName
			), "Not found similar forecast status in past");

		}
	}
}