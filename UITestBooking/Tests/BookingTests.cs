using NUnit.Framework;
using System;
using UITestBooking.ChromeDriverUtilities;

namespace UITestBooking
{
	public class BookingTests
	{
		private const string _UserName = "shmig";
		private ChromeDriverBooking ChromeDriver;

		[SetUp]
		public void Setup()
		{
			ChromeDriver = new ChromeDriverBooking(_UserName);
			ChromeDriver.SetWindowMaximize();
			ChromeDriver.OpenHomePage();
			ChromeDriver.SetImplicitWait(1000);
		}

		[Test, Order(1)]
		public void TestChangeLanguage()
		{
			ChromeDriver.ChangeLanguage(BookingLanguage.EnglishUS);
			Assert.IsTrue(ChromeDriver.Url.Contains("lang=en-us"), "Language is not changed on EnglishUS");

			ChromeDriver.ChangeLanguage(BookingLanguage.Russion);
			Assert.IsTrue(ChromeDriver.Url.Contains("lang=ru"), "Language is not changed on Russion");

			ChromeDriver.Quit();
		}

		[Test, Order(2)]
		public void TestChangeCurrency()
		{
			ChromeDriver.ChangeCurrency(BookingCurrency.USD);
			Assert.IsTrue(ChromeDriver.Url.Contains("selected_currency=USD"), "Currency is not changed on USD");

			ChromeDriver.ChangeCurrency(BookingCurrency.BYN);
			Assert.IsTrue(ChromeDriver.Url.Contains("selected_currency=BYN"), "Currency is not changed on BYN");

			ChromeDriver.Quit();
		}

		[Test, Order(3)]
		public void TestSingInAccount()
		{
			ChromeDriver.SingInAccount();
			Assert.IsTrue(ChromeDriver.Url.Contains("aid="), "Not sing in mysettings");

			ChromeDriver.Quit();
		}

		[Test, Order(4)]
		public void TestTravelFilter()
		{
			const int adults = 2;
			const int children = 1;
			const int age = 12;
			const int rooms = 1;
			const string ss = "Beijing";

			ChromeDriver.SetAdults(adults);
			ChromeDriver.SetChildren(children, age);
			ChromeDriver.SetRooms(rooms);

			var checkinDate = DateTime.Now.AddDays(7);
			var checkoutDate = checkinDate.AddDays(2);
			ChromeDriver.SetCheckPeriodInCalendar(checkinDate, checkoutDate);

			ChromeDriver.SetCheckinPlace(ss);

			ChromeDriver.ClickSearchButton();

			string url = ChromeDriver.Url;
			Assert.IsTrue(url.Contains($"ss={ss}"), "Incorrect place");
			Assert.IsTrue(url.Contains($"checkin_year={checkinDate.Year}&checkin_month={checkinDate.Month}&checkin_monthday={checkinDate.Day}"), "Incorrect check in date");
			Assert.IsTrue(url.Contains($"checkout_year={checkoutDate.Year}&checkout_month={checkoutDate.Month}&checkout_monthday={checkoutDate.Day}"), "Incorrect check Out date");
			Assert.IsTrue(url.Contains($"group_adults={adults}"), "Adults is wrong");
			Assert.IsTrue(url.Contains($"group_children={children}"), "Incorrect children");
			Assert.IsTrue(url.Contains($"age={age}"), "Incorrect age");
			Assert.IsTrue(url.Contains($"no_rooms={rooms}"), "Incorrect rooms");

			ChromeDriver.Quit();
		}
	}
}