﻿using System;
using System.IO;
using System.Linq;
using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenQA.Selenium.Chrome;
using UcsdCapeScraper;
using UcsdCapeScraper.Helpers;
using LogType = UcsdCapeScraper.Helpers.LogType;

// Note: For anyone that doesn't like my use of "goto" and labels, know that I felt like this would be
// the most appropriate way to approach this particular problem. I could use a method to handle exiting
// the program but felt that was overkill.

// Apparently, using a callback results in no exceptions being thrown.
var res = Parser.Default.ParseArguments<CliArguments>(args);
var o = (res as Parsed<CliArguments>)?.Value;
if (o is null) 
	return;

// Make sure the directory exists.
if (!Directory.Exists(o.Directory))
{
	ConsoleHelper.WriteLine(LogType.Error, "Directory is invalid. Please try again.");
	goto exit;
}

// Make sure chromedriver.exe exists.
var dirInfo = new DirectoryInfo(o.Directory);
var files = dirInfo.GetFiles();
if (files.All(x => x.Name != "chromedriver.exe"))
{
	ConsoleHelper.WriteLine(LogType.Error, "chromedriver.exe does not exist in specified directory.");
	goto exit;
}

// Make sure out.json doesn't exist.
if (files.Any(x => x.Name == "out.json"))
{
	ConsoleHelper.WriteLine(LogType.Warning, "You have a duplicate out.json file. Overwrite? y/[n]");
	var ans = Console.ReadLine() ?? "n";
	if (ans.ToLower().Trim() != "y")
	{
		ConsoleHelper.WriteLine(LogType.Error, "Answered yes to question. Please remove out.json and try again.");
		goto exit;
	}
}

var username = o.Username;
if (username is null)
{
	ConsoleHelper.WriteLine(LogType.Info, "What is your TritonLink username?");
	username = Console.ReadLine() ?? string.Empty;
}

ConsoleHelper.WriteLine(LogType.Info, $"TritonLink password for corresponding username {username}?");
var password = MiscHelpers.ReadLineMasked();
Console.Clear();

var chromeOptions = new ChromeOptions();
chromeOptions.AddArgument("--headless");
chromeOptions.AddArgument("log-level=3");

var driver = new ChromeDriver(o.Directory, chromeOptions);
await LoginHelper.Login(driver, username, password);

var result = await ProgramRunner.GetAllCapes(driver);

Console.WriteLine();

// save as json
var json = JsonConvert.SerializeObject(result, new JsonSerializerSettings
{
	ContractResolver = new CamelCasePropertyNamesContractResolver()
});

// we're done parsing
await File.WriteAllTextAsync(Path.Join(o.Directory, "out.json"), json);

driver.Close();
driver.Dispose();

exit:
Console.ReadKey();