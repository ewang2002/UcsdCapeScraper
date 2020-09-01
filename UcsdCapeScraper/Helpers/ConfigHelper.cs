using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UcsdCapeScraper.Definitions;

namespace UcsdCapeScraper.Helpers
{
	public static class ConfigHelper
	{
		/// <summary>
		/// Parses the Configuration file. This assumes that the directory exists and the file also exists.
		/// </summary>
		/// <param name="info">The FileInfo object that represents the config.txt file.</param>
		/// <returns>The parsed configuration object.</returns>
		public static async Task<ConfigFile> GetConfiguration(FileInfo info)
		{
			var configFile = new ConfigFile();

			var lines = await File.ReadAllLinesAsync(info.FullName);
			lines = lines
				.Where(x => !x.StartsWith('#'))
				.ToArray();

			foreach (var line in lines)
			{
				if (line.IndexOf('=') == -1)
					continue;

				var propVal = line.Split('=')
					.Select(x => x.Trim())
					.Where(x => x != string.Empty)
					.ToArray();
				if (propVal.Length < 2)
					continue;

				var prop = propVal[0].Trim();
				var val = string.Join('=', propVal.Skip(1)).Trim();

				switch (prop)
				{
					case "TRITONLINK_USERNAME":
						configFile.TritonLinkUsername = val;
						break;
					case "TRITONLINK_PASSWORD":
						configFile.TritonLinkPassword = val;
						break;
					case "PATH_TO_DRIVERS" when Directory.Exists(val):
						configFile.PathToDrivers = val;
						break;
					case "OUTPUT_JSON_LOCATION" when Directory.Exists(val):
						configFile.OutputJsonPath = val;
						break;
				}
			}

			return configFile;
		}
	}
}