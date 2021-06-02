using System.Linq;
using System.Net;

namespace UcsdCapeScraper.Helpers
{
	/// <summary>
	/// A series of helper methods that are intended to clean the given input.
	/// </summary>
	public static class ParseHelpers
	{
		/// <summary>
		/// Parses the GPA input. The GPA input is usually in the form "Letter (GPA)."
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns>The GPA, if available; otherwise, -1.</returns>
		public static double CleanGpaString(string input)
		{
			if (IsNotAvailable(input))
				return -1;
			
			var splitLeftParen = input.Split('(');
			
			// invalid input
			if (splitLeftParen.Length != 2)
				return -1;
			var gpaStr = splitLeftParen[1]
				.Split(')')[0]
				.Trim();

			return double.TryParse(gpaStr, out var result)
				? result
				: -1;
		}

		/// <summary>
		/// Parses the recommended professor & class values.
		/// </summary>
		/// <param name="input">The input string.</param>
		/// <returns>The parsed percent value, as a double. If this is not available, -1 is returned.</returns>
		public static double CleanRecommendationString(string input)
		{
			if (IsNotAvailable(input))
				return -1;

			input = input
				.Replace("%", string.Empty)
				.Trim();

			return double.TryParse(input, out var result)
				? result
				: -1;
		}

		/// <summary>
		/// Simply ensures that a decimal string doesn't contain any non-numerical values.
		/// </summary>
		/// <param name="input">The input string.</param>
		/// <returns>The parsed value, if possible. Otherwise, -1.</returns>
		public static double CleanDecimalString(string input)
		{
			if (IsNotAvailable(input))
				return -1;

			input = input.Trim();
			return double.TryParse(input, out var result)
				? result
				: -1;
		}

		/// <summary>
		/// Simply ensures that an integer string doesn't contain any non-numerical values.
		/// </summary>
		/// <param name="input">The input string.</param>
		/// <returns>The parsed value, if possible. Otherwise, -1.</returns>
		public static int CleanIntString(string input)
		{
			if (IsNotAvailable(input))
				return -1;

			input = input.Trim();
			return int.TryParse(input, out var result)
				? result
				: -1;
		}

		/// <summary>
		/// Cleans the string input. 
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns>The cleaned output.</returns>
		public static string CleanStringInput(string input)
		{
			if (input.Length == 0)
				return string.Empty;

			// make sure no random html stuff
			input = WebUtility.HtmlDecode(input)
				.Trim();

			// no extra spaces
			input = string.Join(" ", input
				.Split(" ")
				.Select(x => x.Trim())
				.Where(y => y != string.Empty));

			return input;
		}

		/// <summary>
		/// Removes the parenthesis and letter. For example, "Mathematical Reasoning (A)" would become "Mathematical Reasoning."
		/// </summary>
		/// <param name="input">The string input.</param>
		/// <returns>The input with no parenthesis and letter.</returns>
		public static string RemoveParenLetter(string input)
		{
			foreach (var character in Constants.LettersNumber)
			{
				if (!input.Contains($"({character})"))
					continue;

				input = input.Replace($"({character})", string.Empty);
				break;
			}

			return input.Trim();
		}

		/// <summary>
		/// Simply ensures that the string input doesn't contain "N/A," which is quite common. 
		/// </summary>
		/// <param name="input">The input string to check.</param>
		/// <returns>Whether the input is not available.</returns>
		private static bool IsNotAvailable(string input)
			=> input.ToLower().Trim().Contains("n/a");
	}
}