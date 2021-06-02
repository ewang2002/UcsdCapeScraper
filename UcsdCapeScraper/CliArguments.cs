using CommandLine;

namespace UcsdCapeScraper
{
	public class CliArguments
	{
		private const string DirHelp = "Contains your Selenium webdriver. This will also be the place where the " +
		                               "resulting JSON will be saved to.";

		private const string UsernameHelp = "Your Tritonlink Username. Make sure you have access to Duo 2FA.";

		[Option('d', "dir", Required = true, HelpText = DirHelp)]
		public string Directory { get; set; }

#nullable enable

		[Option('u', "username", Required = false, HelpText = UsernameHelp)]
		public string? Username { get; set; }

#nullable disable
	}
}