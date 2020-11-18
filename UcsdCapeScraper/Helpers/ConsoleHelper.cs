using System;

namespace UcsdCapeScraper.Helpers
{
	public static class ConsoleHelper
	{
		/// <summary>
		/// Calls Console.WriteLine with a foreground color.
		/// </summary>
		/// <typeparam name="T">The input type.</typeparam>
		/// <param name="input">The input.</param>
		/// <param name="foreground">The color that the output should be.</param>
		public static void WriteLineWithColor<T>(T input, ConsoleColor foreground)
		{
			Console.ForegroundColor = foreground;
			Console.WriteLine(input);
			Console.ResetColor();
		}

		/// <summary>
		/// Calls Console.WriteLine with a foreground color and log type.
		/// </summary>
		/// <typeparam name="T">The input type.</typeparam>
		/// <param name="input">The input to be logged.</param>
		/// <param name="foreground">The color that the output should be.</param>
		/// <param name="type">The logging type.</param>
		public static void LogWithColor<T>(T input, ConsoleColor foreground, LogType type)
		{
			var time = DateTime.Now;

			var logMsg = time.ToString("[HH:mm:ss] ") + type switch
			{
				LogType.Error => "[Error] ",
				LogType.Info => "[Info] ",
				LogType.Warning => "[Warn] ",
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			} + input;

			Console.ForegroundColor = foreground;
			Console.WriteLine(logMsg);
			Console.ResetColor();
		}
	}

	public enum LogType
	{
		Error,
		Info,
		Warning
	}
}