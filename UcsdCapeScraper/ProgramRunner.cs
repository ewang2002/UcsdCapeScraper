using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using UcsdCapeScraper.Definitions;
using UcsdCapeScraper.Helpers;
using static UcsdCapeScraper.Helpers.ParseHelpers;
using LogType = UcsdCapeScraper.Helpers.LogType;

namespace UcsdCapeScraper
{
	public static class ProgramRunner
	{
		/// <summary>
		/// Gets all CAPE data.
		/// </summary>
		/// <param name="driver">The driver to use.</param>
		/// <returns>A dictionary of CAPE data. Key is department; V is list of rows.</returns>
		public static async Task<Dictionary<string, IList<CapeEvalResultsRow>>> GetAllCapes(ChromeDriver driver)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			// get all departments
			var departmentDropDown =
				new SelectElement(driver.FindElement(By.Name("ctl00$ContentPlaceHolder1$ddlDepartments")));
			var allOptions = departmentDropDown.Options
				.Skip(1) // remove "select a department" 
				.ToArray();

			var returnDict = new Dictionary<string, IList<CapeEvalResultsRow>>();
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
						x.FindElement(By.Id("ContentPlaceHolder1_UpdateProgress1")).GetAttribute("style")
							.Equals("display: none;"));
				}
				catch (Exception)
				{
					ConsoleHelper.WriteLine(LogType.Warning, $"Error getting CAPE data for {option.Text}. Skipping.");
					continue;
				}

				ConsoleHelper.WriteLine(LogType.Info, $"CAPE website has been loaded with the department: {option.Text}");
				var departmentId = option.Text.Split('-')[0].Trim();
				returnDict.Add(departmentId, new List<CapeEvalResultsRow>());

				// begin parsing 
				var doc = new HtmlDocument();
				doc.LoadHtml(driver.PageSource);

				// get table 
				var outerTable = doc.GetElementbyId("ContentPlaceHolder1_gvCAPEs");
				if (outerTable is null)
				{
					ConsoleHelper.WriteLine(LogType.Warning, "A table could not be found for this department. " +
					                                         "There is probably no data available. Skipping.");
					continue;
				}

				// get all the rows except for the top row 
				var tBodyNodes = outerTable.SelectNodes("tbody");
				if (tBodyNodes == null || tBodyNodes.Count == 0)
				{
					ConsoleHelper.WriteLine(LogType.Warning, $"A table was found {option.Text}, but there is " +
					                                         "no data available. Skipping.");
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

				ConsoleHelper.WriteLine(LogType.Info, $"Successfully scraped {returnDict[departmentId].Count} rows " +
				                                      "for this department.");
			}

			var rowsScraped = returnDict.Select(x => x.Value.Count).Sum();
			Console.WriteLine();
			stopwatch.Stop();
			var timeTaken = $"{stopwatch.Elapsed.Minutes} Minutes, " +
			                $"{stopwatch.Elapsed.Seconds} Seconds, " +
			                $"{stopwatch.Elapsed.Milliseconds} Milliseconds.";
			ConsoleHelper.WriteLine(LogType.Info, $"Successfully scraped {rowsScraped} rows. Time taken: {timeTaken}.");
			return returnDict;
		}
	}
}