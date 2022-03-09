using System;

namespace UcsdCapeScraper.Helpers
{
	public static class ConsoleHelper
	{
		/// <summary>
		/// Calls Console.WriteLine with a foreground color and log type.
		/// </summary>
		/// <typeparam name="T">The input type.</typeparam>
		/// <param name="input">The input to be logged.</param>
		/// <param name="type">The logging type.</param>
		public static void WriteLine<T>(LogType type, T input)
		{
			var time = DateTime.Now;
			var color = type switch
			{
				LogType.Error => ConsoleColor.Red,
				LogType.Warning => ConsoleColor.Yellow,
				LogType.Info => ConsoleColor.White,
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};

			var logMsg = time.ToString("[HH:mm:ss] ") + type switch
			{
				LogType.Error => "[Error] ",
				LogType.Info => "[Info] ",
				LogType.Warning => "[Warn] ",
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			} + input;

			Console.ForegroundColor = color;
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