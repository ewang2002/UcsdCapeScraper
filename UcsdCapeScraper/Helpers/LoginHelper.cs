using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using static UcsdCapeScraper.Constants;

namespace UcsdCapeScraper.Helpers
{
	public static class LoginHelper
	{
		public static async Task<ChromeDriver> Login(string pathToDrivers, string username, string password)
		{
			var chromeOptions = new ChromeOptions();
			chromeOptions.AddArgument("--headless");
			chromeOptions.AddArgument("log-level=3");

			using var driver = new ChromeDriver(pathToDrivers, chromeOptions);
			driver.Navigate().GoToUrl(CapeUrl);

			Console.Clear();
			driver.FindElementByName("urn:mace:ucsd.edu:sso:username")
				.SendKeys(username);
			driver.FindElementByName("urn:mace:ucsd.edu:sso:password")
				.SendKeys(password + Keys.Enter);

			if (driver.FindElementsById("_login_error_message").Count != 0)
			{
				ConsoleHelper.WriteLine(LogType.Error, "Your username or password was incorrect.");
				driver.Close();
				return null;
			}

			try
			{
				ConsoleHelper.WriteLine(LogType.Info, "Please authenticate this session with Duo 2FA. You have one " +
				                                      "minute.");
				var wait = new WebDriverWait(driver, TimeSpan.FromMinutes(1));
				wait.Until(x => x.Url.Contains("responses"));
			}
			catch (Exception)
			{
				ConsoleHelper.WriteLine(LogType.Error, "You did not authenticate with Duo 2FA in time. Please restart " +
				                                       "this program.");
				driver.Close();
				return null;
			}

			ConsoleHelper.WriteLine(LogType.Info, "Logged in successfully.");

			await Task.Delay(TimeSpan.FromSeconds(3));
			return driver;
		}
	}
}