using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using static UcsdCapeScraper.Constants;
using UcsdCapeScraper.Definitions;
using static UcsdCapeScraper.Helpers;

namespace UcsdCapeScraper
{
	public class Program
	{
		public static async Task Main()
		{
			// get inputs
			Console.WriteLine("[INPUT] Please type your TritonLink username (Or Student PID).");
			var username = Console.ReadLine()?.Trim() ?? "";
			Console.WriteLine("[INPUT] Please type your password.");
			var password = Console.ReadLine()?.Trim() ?? "";
			Console.Clear();
			using var driver = new ChromeDriver(@"C:\Users\ewang\Desktop\Selenium Drivers");
			driver.Navigate().GoToUrl(CapeUrl);

			Console.Clear();
			driver.FindElementByName("urn:mace:ucsd.edu:sso:username").SendKeys(username);
			driver.FindElementByName("urn:mace:ucsd.edu:sso:password").SendKeys(password + Keys.Enter);

			if (driver.FindElementsById("_login_error_message").Count != 0)
			{
				Console.WriteLine("[ERROR] Your username or password was incorrect. Please restart the program.");
				driver.Close();
				return;
			}

			try
			{
				Console.WriteLine("[INFO] Please authenticate with Duo. You have 1 minute.");
				var wait = new WebDriverWait(driver, TimeSpan.FromMinutes(1));
				wait.Until(x => x.Url.Contains(CapeUrl));
			}
			catch (Exception)
			{
				Console.WriteLine("[ERROR] You did not authenticate with Duo in time. Please restart the program.");
				driver.Close();
				return;
			}

			Console.WriteLine("[INFO] Logged in successfully!");

			// get all departments
			var departmentDropDown =
				new SelectElement(driver.FindElement(By.Name("ctl00$ContentPlaceHolder1$ddlDepartments")));
			var allOptions = departmentDropDown.Options
				.Skip(1) // remove "select a department" 
				.ToArray();
			
			var returnDict = new Dictionary<string, IList<CapeEvalResultsRow>>();
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			// go through each department 
			foreach (var option in allOptions)
			{
				Console.WriteLine();
				option.Click();
				driver.FindElementByName("ctl00$ContentPlaceHolder1$btnSubmit").Click();
				// because the website takes that long to change between
				// the loading and not loading state 
				await Task.Delay(TimeSpan.FromSeconds(1));
				try
				{
					var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
					wait.Until(x =>
						x.FindElement(By.Id("ctl00_ContentPlaceHolder1_UpdateProgress1")).GetAttribute("style")
							.Equals("display: none;"));
				}
				catch (Exception)
				{
					Console.WriteLine($"[ERROR] Error getting CAPE data for {option.Text}. Skipping.");
					continue;
				}

				Console.WriteLine($"[INFO] CAPE website has been loaded for {option.Text}.");
				var departmentId = option.Text.Split('-')[0].Trim();
				returnDict.Add(departmentId, new List<CapeEvalResultsRow>());

				// begin parsing 
				var doc = new HtmlDocument();
				doc.LoadHtml(driver.PageSource);

				// get table 
				var outerTable = doc.GetElementbyId("ctl00_ContentPlaceHolder1_gvCAPEs");
				if (outerTable == null)
				{
					Console.WriteLine("[ERROR] A table could not be found. Skipping.");
					continue;
				}

				// get all the rows except for the top row 
				var tBodyNodes = outerTable.SelectNodes("tbody");
				if (tBodyNodes == null || tBodyNodes.Count == 0)
				{
					Console.WriteLine("[ERROR] A table was found, but there are no entries available. Skipping.");
					continue;
				}

				var rowNodes = tBodyNodes.First().SelectNodes("tr");

				// go through each row
				foreach (var row in rowNodes)
				{
					var cells = row.SelectNodes("td");
					var instructor = cells[0].InnerText.Trim();
					var courseIdAndName = cells[1].ChildNodes[1].InnerText.Split("-")
						.Select(x => x.Trim())
						.ToArray();

					// basic course info 
					var courseId = CleanStringInput(courseIdAndName[0]);
					var courseName = CleanStringInput(RemoveParenLetter(courseIdAndName[1]));
					var term = CleanStringInput(cells[2].InnerText);
					var enrolled = CleanIntString(cells[3].InnerText);
					var evalsMade = CleanIntString(cells[4].ChildNodes[1].InnerText);
					// recommended class
					var remdClass = CleanRecommendationString(cells[5].ChildNodes[1].InnerText);
					// recommended instructor 
					var remdInstructor = CleanRecommendationString(cells[6].ChildNodes[1].InnerText);
					// study hours per week
					var studyHrWk = CleanDecimalString(cells[7].ChildNodes[1].InnerText);
					// avg gpa that was expected
					var avgGradeExpected = CleanGpaString(cells[8].ChildNodes[1].InnerText);
					// avg gpa that was received
					var avgGradeReceived = CleanGpaString(cells[9].ChildNodes[1].InnerText);

					returnDict[departmentId].Add(new CapeEvalResultsRow
					{
						Instructor = instructor,
						AverageGradeExpected = avgGradeExpected,
						AverageGradeReceived = avgGradeReceived,
						CourseNumber = courseId,
						CourseTitle = courseName,
						Enrolled = enrolled,
						EvalsMade = evalsMade,
						RecommendClass = remdClass,
						RecommendInstructor = remdInstructor,
						StudyHrsWk = studyHrWk,
						Term = term
					});
				}

				Console.WriteLine($"[INFO] Successfully scraped {returnDict[departmentId].Count} rows.");
			}
			stopwatch.Stop();
			Console.WriteLine();

			// save as json
			var json = JsonConvert.SerializeObject(returnDict);

			// we're done parsing
			// time to figure out a place
			while (true)
			{
				Console.WriteLine("[INFO] Please input the directory where you want to save the data.");
				var dir = Console.ReadLine()?.Trim() ?? "";
				if (!Directory.Exists(dir)) 
					continue;
				try
				{
					await File.WriteAllTextAsync(Path.Join(dir, "out.json"), json);
					break;
				}
				catch (Exception)
				{
					Console.WriteLine("[ERROR] Couldn't write to directory. Try again.");
				}
			}

			Console.WriteLine();
			Console.WriteLine($"[INFO] Scraping took {stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds}.");
			Console.ReadKey();
		}
	}
}