using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
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
        /// <param name="writer">The file to write to.</param>
        public static async Task GetAllCapes(ChromeDriver driver, StreamWriter writer)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var processedEntries = new HashSet<string>();

            // get all departments
            var departmentDropDown =
                new SelectElement(driver.FindElement(By.Name("ctl00$ContentPlaceHolder1$ddlDepartments")));
            var totalScraped = 0;
            // Go through each department 
            // Note that we're skipping the "select a department" op 
            foreach (var option in departmentDropDown.Options.Skip(1))
            {
                Console.WriteLine();
                option.Click();
                driver.FindElement(By.Name("ctl00$ContentPlaceHolder1$btnSubmit")).Click();
                // because the website takes that long to change between
                // the loading and not loading state 
                await Task.Delay(TimeSpan.FromSeconds(1));
                if (!WaitForTableToLoad(driver, option.Text))
                    continue;
                ConsoleHelper.WriteLine(LogType.Info,
                    $"CAPE website has been loaded with the department: {option.Text}");

                var processed = ProcessPage(driver, option.Text, processedEntries);
                if (!processed.Any())
                    continue;

                foreach (var row in processed)
                    await writer.WriteLineAsync(row);

                ConsoleHelper.WriteLine(LogType.Info,
                    $"Successfully scraped {processed.Count} rows for this department.");
                totalScraped += processed.Count;
            }

            // Now that we're gone through all department, we can just reset the search so we can go through 
            // individual subjects that weren't in the department list.
            driver.Navigate().GoToUrl(Constants.CapeUrl);
            var searchCourseBox = driver.FindElement(By.Name("ctl00$ContentPlaceHolder1$txtCourse"));
            foreach (var subject in Constants.SubjectsToCheck)
            {                
                Console.WriteLine();
                searchCourseBox.Clear();
                searchCourseBox.SendKeys(subject);
                driver.FindElement(By.Name("ctl00$ContentPlaceHolder1$btnSubmit")).Click();
                await Task.Delay(TimeSpan.FromSeconds(1));
                if (!WaitForTableToLoad(driver, subject))
                    continue;
                ConsoleHelper.WriteLine(LogType.Info,
                    $"CAPE website has been loaded with the subject: {subject}");

                var processed = ProcessPage(driver, subject, processedEntries);
                if (!processed.Any())
                    continue;
                
                foreach (var row in processed)
                    await writer.WriteLineAsync(row);

                ConsoleHelper.WriteLine(LogType.Info,
                    $"Successfully scraped {processed.Count} rows for this subject.");
                totalScraped += processed.Count;
            }

            await writer.FlushAsync();
            Console.WriteLine();
            stopwatch.Stop();
            var timeTaken = $"{stopwatch.Elapsed.Minutes} Minutes, {stopwatch.Elapsed.Seconds} Seconds";
            ConsoleHelper.WriteLine(LogType.Info, $"Scraped {totalScraped} rows. Time taken: {timeTaken}.");
        }

        /// <summary>
        /// Waits for the table to load.
        /// </summary>
        /// <param name="driver">The driver.</param>
        /// <param name="dataFor">The department or subject that we're getting data for.</param>
        /// <returns>True if the table loaded successfully, False otherwise.</returns>
        private static bool WaitForTableToLoad(IWebDriver driver, string dataFor)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(x =>
                    x.FindElement(By.Id("ContentPlaceHolder1_UpdateProgress1"))
                        .GetAttribute("style")
                        .Equals("display: none;"));
                return true;
            }
            catch (Exception)
            {
                ConsoleHelper.WriteLine(LogType.Warning, $"Error getting CAPE data for {dataFor}. Skipping.");
                return false;
            }
        }

        /// <summary>
        /// Processes a page from the CAPE website.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <param name="optionText">The department or subject string.</param>
        /// <param name="processed">The entries that are already processed.</param>
        /// <returns>The list of entries to be written to the file.</returns>
        private static IList<string> ProcessPage(IWebDriver driver, string optionText,
            ISet<string> processed)
        {
            var entries = new List<string>();
            var doc = new HtmlDocument();
            doc.LoadHtml(driver.PageSource);

            // get table 
            var outerTable = doc.GetElementbyId("ContentPlaceHolder1_gvCAPEs");
            if (outerTable is null)
            {
                ConsoleHelper.WriteLine(LogType.Warning, "A table could not be found for this department/subject. " +
                                                         "There is probably no data available. Skipping.");
                return entries;
            }

            // get all the rows except for the top row 
            var tBodyNodes = outerTable.SelectNodes("tbody");
            if (tBodyNodes == null || tBodyNodes.Count == 0)
            {
                ConsoleHelper.WriteLine(LogType.Warning, $"A table was found for {optionText}, but there is " +
                                                         "no data available. Skipping.");
                return entries;
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
                var subCourse = CleanStringInput(courseIdAndName[0]);
                var courseName = CleanStringInput(RemoveParenLetter(courseIdAndName[1]));
                var term = CleanStringInput(cells[2].InnerText);
                var enrolled = CleanIntString(cells[3].InnerText);
                var evalsMade = CleanIntString(cells[4].ChildNodes[1].InnerText);
                // recommended class
                var recmdClass = CleanRecommendationString(cells[5].ChildNodes[1].InnerText);
                // recommended instructor 
                var recmdInstructor = CleanRecommendationString(cells[6].ChildNodes[1].InnerText);
                // study hours per week
                var studyHrWk = CleanDecimalString(cells[7].ChildNodes[1].InnerText);
                // avg gpa that was expected
                var avgGradeExpected = CleanGpaString(cells[8].ChildNodes[1].InnerText);
                // avg gpa that was received
                var avgGradeReceived = CleanGpaString(cells[9].ChildNodes[1].InnerText);

                // ReSharper disable once UseStringInterpolation
                var rowText = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}",
                    instructor,
                    subCourse,
                    courseName,
                    term,
                    enrolled,
                    evalsMade,
                    recmdClass,
                    recmdInstructor,
                    studyHrWk,
                    avgGradeExpected,
                    avgGradeReceived);
                if (processed.Contains(rowText))
                    continue;

                entries.Add(rowText);
                processed.Add(rowText);
            }

            return entries;
        }
    }
}