using OpenQA.Selenium.Chrome;
using System;
using System.IO;

namespace UITestBooking.ChromeDriverUtilities
{
	abstract class BaseControllerChromeDriver
	{
		protected ChromeDriver _ChromeDriver;

		public string Url => _ChromeDriver.Url;

		protected BaseControllerChromeDriver(string userName)
		{
			var options = new ChromeOptions();
			options.AddArgument(@$"user-data-dir=C:\Users\{userName}\AppData\Local\Google\Chrome\User Data");
			_ChromeDriver = new ChromeDriver(Directory.GetCurrentDirectory() + @"\chromedriver", options);
		}
		protected BaseControllerChromeDriver()
		{
			_ChromeDriver = new ChromeDriver(Directory.GetCurrentDirectory() + @"\chromedriver");
		}

		public void SetWindowMaximize()
		{
			_ChromeDriver.Manage().Window.Maximize();
		}

		public void Quit()
		{
			_ChromeDriver.Quit();
		}

		public void SetImplicitWait(int milliseconds)
		{
			_ChromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(milliseconds);
		}

		abstract public void OpenHomePage();
	}
}
