using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITestBooking.ChromeDriverUtilities
{
	enum BookingLanguage
	{
		EnglishUK,
		EnglishUS,
		Russion,
	}
	enum BookingCurrency
	{
		USD,
		BYN,
		RUB,
		EUR,
	}

	class ChromeDriverBooking : BaseControllerChromeDriver
	{
		private readonly Dictionary<BookingLanguage, string> DataLanguagePairs = new Dictionary<BookingLanguage, string>() {
			{ BookingLanguage.EnglishUK,    "en-gb" },
			{ BookingLanguage.EnglishUS,    "en-us" },
			{ BookingLanguage.Russion,      "ru"    },
		};
		private IWebElement CalendarElement;
		private IWebElement CalendarGroupElement;
		private IWebElement MenuGroupsElement;

		private bool IsDisplayCalendarElement => CalendarElement.GetCssValue("display") != "none";

		public ChromeDriverBooking(string userName)
			: base(userName) { }

		public ChromeDriverBooking()
			: base() { }

		public override void OpenHomePage()
		{
			_ChromeDriver.Navigate().GoToUrl("http://Booking.com/");

			CalendarElement = _ChromeDriver.FindElement(By.XPath(@"//div[@data-component='search/dates/single-calendar']/div[1]"));
			CalendarGroupElement = _ChromeDriver.FindElement(By.XPath("//div[@class='xp__fieldset js--sb-fieldset accommodation ']/div[2]"));
			MenuGroupsElement = _ChromeDriver.FindElement(By.XPath("//div[@class='xp__fieldset js--sb-fieldset accommodation ']/div[3]"));
		}

		public void ChangeLanguage(BookingLanguage language)
		{
			_ChromeDriver.FindElement(By.CssSelector("button[data-modal-id='language-selection']")).Click();
			string languageCode = DataLanguagePairs[language];
			_ChromeDriver.FindElement(By.CssSelector($"a[data-lang='{languageCode}']")).Click();
		}

		public void ChangeCurrency(BookingCurrency currency)
		{
			_ChromeDriver.FindElement(By.CssSelector("button[data-modal-header-async-type='currencyDesktop']")).Click();
			string currencyCode = Enum.GetName(typeof(BookingCurrency), currency);
			_ChromeDriver.FindElement(By.XPath($"//a[contains(@data-modal-header-async-url-param,'selected_currency={currencyCode}')]")).Click();
		}

		public void SingInAccount()
		{
			bool isNotSingInUser = _ChromeDriver.FindElements(By.XPath("//div[@class='bui-group__item']/*[@class='bk-icon -streamline-account_create']")).Any();
			if (isNotSingInUser)
			{
				throw new Exception("you don't sign in");
			}

			_ChromeDriver.FindElement(By.XPath("//a[contains(@aria-describedby,'profile-menu-trigger--content')]")).Click();
			_ChromeDriver.FindElement(By.XPath("//div[@role='menu']/ul/li[1]/a")).Click();
		}

		public void SetCheckinPlace(string address)
		{
			IWebElement searchPlace = _ChromeDriver.FindElement(By.Id("ss"));
			searchPlace.Clear();
			searchPlace.SendKeys(address);
			_ChromeDriver.FindElement(By.XPath("//ul[@data-list]/li[1]")).Click();
		}

		public void SetCheckPeriodInCalendar(DateTime checkinDate, DateTime checkoutDate)
		{
			double bookingDays = (checkoutDate - checkinDate).TotalDays;
			if (bookingDays > 45)
				throw new Exception("You can not booking more than 45 day");
			if (bookingDays < 1)
				throw new Exception("You can not booking less than one day");
			if (checkinDate < DateTime.Now)
				throw new Exception("You can not booking in the past");

			ResetCalendar(checkinDate);

			SetDateInCalendar(checkinDate);
			SetDateInCalendar(checkoutDate);
		}

		public void SetAdults(int countAdults)
		{
			if (countAdults < 1)
				throw new Exception("Minimum 1 adult");

			SetGroupValueInThirdFilter(countAdults, "group_adults");
			MenuGroupsElement.Click();
		}

		public void SetChildren(int countChildren, int age)
		{
			if (countChildren < 0)
				throw new Exception("Minimum 0 child");

			SetGroupValueInThirdFilter(countChildren, "group_children");

			_ChromeDriver.FindElements(
				By.XPath($"//select[@name='age']/option[@value='{age}']")
			).ToList().ForEach(option => option.Click());

			MenuGroupsElement.Click();
		}

		public void SetRooms(int countRooms)
		{
			if (countRooms < 1)
				throw new Exception("Minimum 1 room");

			SetGroupValueInThirdFilter(countRooms, "no_rooms");
			MenuGroupsElement.Click();
		}

		public void ClickSearchButton()
		{
			_ChromeDriver.FindElement(
				By.XPath($"//button[@data-sb-id='main'][@type='submit']")
			).Click();
		}

		#region private

		private void ResetCalendar(DateTime checkinDate)
		{
			int i = 1;
			do
			{
				SetDateInCalendar(checkinDate.AddDays(i));
				i++;
			}
			while (IsDisplayCalendarElement);
		}

		private void SetDateInCalendar(DateTime date)
		{
			if (IsDisplayCalendarElement == false)
				CalendarGroupElement.Click();

			SweepCalendarToLeft();

			string dateCode = date.ToString("yyyy-MM-dd");
			IWebElement dateElement;
			do
			{
				dateElement = _ChromeDriver.FindElements(
					By.XPath($"//div[@class='bui-calendar__content']/div/table/tbody/tr/td[@data-date='{dateCode}']")
				).FirstOrDefault();
				if (dateElement == null)
				{
					IWebElement nextCalendarDiv = _ChromeDriver.FindElements(
						By.XPath("//div[@class='bui-calendar__control bui-calendar__control--next'][@data-bui-ref='calendar-next']")
					).FirstOrDefault();
					if (nextCalendarDiv == null)
					{
						throw new Exception("Too big date");
					}
					nextCalendarDiv.Click();
				}
			}
			while (dateElement == null);
			dateElement.Click();
		}

		private void SweepCalendarToLeft()
		{
			IWebElement prevCalendarDiv;
			do
			{
				prevCalendarDiv = _ChromeDriver.FindElements(
					By.XPath("//div[@class='bui-calendar__control bui-calendar__control--prev'][@data-bui-ref='calendar-prev']")
				).FirstOrDefault();
				prevCalendarDiv?.Click();
			}
			while (prevCalendarDiv != null);
		}

		private void SetGroupValueInThirdFilter(int number, string nameGroup)
		{
			MenuGroupsElement.Click();
			int firstValue = Convert.ToInt32(_ChromeDriver.FindElement(
				By.XPath($"//input[@name='{nameGroup}']")
			).GetAttribute("value"));

			int numberClick = Math.Abs(number - firstValue);
			string classButton = number > firstValue ? "add-button" : "subtract-button";
			IWebElement stepButton = _ChromeDriver.FindElement(
				By.XPath($"//button[contains(@aria-describedby,'{nameGroup}')][contains(@class,'{classButton}')]")
			);
			for (int i = 0; i < numberClick; i++)
			{
				stepButton.Click();
			}
		}

		#endregion
	}
}
