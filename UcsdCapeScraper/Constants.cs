namespace UcsdCapeScraper
{
	public static class Constants
	{
		/// <summary>
		/// The URL to the CAPE evaluations.
		/// </summary>
		public const string CapeUrl = "https://cape.ucsd.edu/responses/Results.aspx";

		/// <summary>
		/// All the letters and numbers.
		/// </summary>
		public const string LettersNumber = "ABCDEFGHIJLKMNOPQRSTUVWXYZ1234567890";

		/// <summary>
		/// Subjects that aren't under any departments that CAPE has, because CAPE is outdated.
		/// </summary>
		public static readonly string[] SubjectsToCheck =
		{
			"AIP",
			"AAS",
			"AWP",
			"CLAS",
			"CCS",
			"DSC",
			"GLBH",
			"FMPH",
			"SEV",
			"SYN"
		};
	}
}