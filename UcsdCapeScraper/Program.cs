using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using static UcsdCapeScraper.Constants;
using UcsdCapeScraper.Definitions;
using static UcsdCapeScraper.Helpers.ConfigHelper;
using static UcsdCapeScraper.Helpers.ConsoleHelper;
using static UcsdCapeScraper.Helpers.ParseHelpers;
using LogType = UcsdCapeScraper.Helpers.LogType;

namespace UcsdCapeScraper
{
	public class Program
	{
		public static async Task Main()
		{
			// determine initial path
			var execAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
			var pathOfExecution = Path.GetDirectoryName(execAssembly.Location);
			var dirInfo = new DirectoryInfo(pathOfExecution!);
			var possibleConfigFiles = dirInfo
				.GetFiles()
				.Where(x => x.Name.ToLower() == "config.txt")
				.ToArray();
			FileInfo configFile;

			if (possibleConfigFiles.Any())
				configFile = possibleConfigFiles.First();
			else
			{
				LogWithColor(
					"A config.txt file could not be found. This program cannot continue unless you have this file. Please type the path to this file now.",
					ConsoleColor.DarkYellow, LogType.Warning);
				var userSpecifiedPath = Console.ReadLine() ?? string.Empty;
				if (userSpecifiedPath == string.Empty || !File.Exists(userSpecifiedPath))
				{
					LogWithColor("The path you specified was invalid. This program is unable to continue.",
						ConsoleColor.Red, LogType.Error);
					return;
				}

				configFile = new FileInfo(userSpecifiedPath);
			}

			var parsedConfig = await GetConfiguration(configFile);
			if (!Directory.Exists(parsedConfig.PathToDrivers))
			{
				LogWithColor(
					"Your PATH_TO_DRIVERS variable is invalid. This variable must point to a folder that has the Chrome Selenium drivers.",
					ConsoleColor.Red, LogType.Error);
				return;
			}

			if (!Directory.Exists(parsedConfig.OutputJsonPath))
			{
				LogWithColor(
					"Your OUTPUT_JSON_LOCATION variable is invalid. This variable must point to a folder where the generated JSON file should be saved.",
					ConsoleColor.Red, LogType.Error);
				return;
			}

			if (parsedConfig.TritonLinkPassword == string.Empty || parsedConfig.TritonLinkUsername == string.Empty)
			{
				LogWithColor(
					"Your TRITONLINK_USERNAME and/or TRITONLINK_PASSWORD variable is invalid. Please make sure both variables have been filled out and then try again.",
					ConsoleColor.Red, LogType.Error);
				return;
			}

			using var driver = new ChromeDriver(parsedConfig.PathToDrivers);
			driver.Navigate().GoToUrl(CapeUrl);

			Console.Clear();
			driver.FindElementByName("urn:mace:ucsd.edu:sso:username").SendKeys(parsedConfig.TritonLinkUsername);
			driver.FindElementByName("urn:mace:ucsd.edu:sso:password")
				.SendKeys(parsedConfig.TritonLinkPassword + Keys.Enter);

			if (driver.FindElementsById("_login_error_message").Count != 0)
			{
				LogWithColor(
					"Your username or password was incorrect. Please check your configuration file and restart the program.",
					ConsoleColor.Red, LogType.Error);
				driver.Close();
				return;
			}

			try
			{
				LogWithColor("Please authenticate this session with Duo 2FA. You have one minute.", ConsoleColor.Cyan,
					LogType.Info);
				var wait = new WebDriverWait(driver, TimeSpan.FromMinutes(1));
				wait.Until(x => x.Url.Contains(CapeUrl));
			}
			catch (Exception)
			{
				LogWithColor("You did not authenticate with Duo 2FA in time. Please restart this program.",
					ConsoleColor.Red, LogType.Error);
				driver.Close();
				return;
			}

			LogWithColor("Logged in successfully.", ConsoleColor.Green, LogType.Info);

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
					LogWithColor($"Error getting CAPE data for {option.Text}. Skipping.", ConsoleColor.DarkYellow,
						LogType.Warning);
					continue;
				}

				LogWithColor($"CAPE website has been loaded with the department: {option.Text}", ConsoleColor.Cyan,
					LogType.Info);
				var departmentId = option.Text.Split('-')[0].Trim();
				returnDict.Add(departmentId, new List<CapeEvalResultsRow>());

				// begin parsing 
				var doc = new HtmlDocument();
				doc.LoadHtml(driver.PageSource);

				// get table 
				var outerTable = doc.GetElementbyId("ctl00_ContentPlaceHolder1_gvCAPEs");
				if (outerTable == null)
				{
					LogWithColor(
						$"A table could not be found for this department. There is probably no data available. Skipping.",
						ConsoleColor.DarkYellow, LogType.Warning);
					continue;
				}

				// get all the rows except for the top row 
				var tBodyNodes = outerTable.SelectNodes("tbody");
				if (tBodyNodes == null || tBodyNodes.Count == 0)
				{
					LogWithColor($"A table was found {option.Text}, but there is no data available. Skipping.",
						ConsoleColor.DarkYellow, LogType.Warning);
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

				WriteLineWithColor($"\tSuccessfully scraped {returnDict[departmentId].Count} rows for this department.",
					ConsoleColor.Green);
			}

			stopwatch.Stop();
			Console.WriteLine();

			// save as json
			var json = JsonConvert.SerializeObject(returnDict);

			// we're done parsing
			await File.WriteAllTextAsync(Path.Join(parsedConfig.OutputJsonPath, "out.json"), json);

			Console.WriteLine();
			LogWithColor(
				$"Scraped {returnDict.Select(x => x.Value.Count).Sum()} rows in {stopwatch.Elapsed.Minutes} Minutes and {stopwatch.Elapsed.Seconds} Seconds.\n\nPress any key to exit.",
				ConsoleColor.Cyan, LogType.Info);
			Console.ReadKey();
		}
	}
}