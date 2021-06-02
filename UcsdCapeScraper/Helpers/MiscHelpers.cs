using System;
using System.Text;

namespace UcsdCapeScraper.Helpers
{
	public static class MiscHelpers
	{

		/// <summary>
		/// Basically just like <c>Console.WriteLine</c> but masks your input.
		/// See: https://stackoverflow.com/questions/3404421/password-masking-console-application
		/// </summary>
		/// <returns>The input.</returns>
		public static string ReadLineMasked()
		{
			var input = new StringBuilder();
			ConsoleKey key;
			do
			{
				var keyInfo = Console.ReadKey(true);
				key = keyInfo.Key;
				if (key == ConsoleKey.Backspace && input.Length > 0)
				{
					Console.Write("\b \b");
					input.Remove(input.Length - 1, 1);
				}
				else if (!char.IsControl(keyInfo.KeyChar))
				{
					Console.Write('*');
					input.Append(keyInfo.KeyChar);
				}
			} while (key != ConsoleKey.Enter);

			return input.ToString();
		}
	}
}