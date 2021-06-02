namespace UcsdCapeScraper.Definitions
{
	// To be used in a dictionary.
	public struct ProfessorResultsEntry
	{
		public int TotalEvaluations { get; init; }
		
		public double AverageRecommendInstructor { get; init; }
		public double MedianRecommendInstructor { get; init; }
		private double StDeviationRecommendInstructor { get; init; }
		
		public double AverageGradeExpected { get; init; }
		public double StDeviationAverageGradeExpected { get; init; }
		
		public double AverageGradeReceived { get; init; }
		public double StDeviationAverageGradeReceived { get; init; }
	}
}