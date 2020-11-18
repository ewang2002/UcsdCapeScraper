namespace UcsdCapeScraper.Definitions
{
	public class CapeEvalResultsRow
	{
		/// <summary>
		/// The name of the instructor.
		/// </summary>
		public string Instructor { get; init; }

		/// <summary>
		/// The course number (ex. CSE 8B)
		/// </summary>
		public string CourseNumber { get; init; }

		/// <summary>
		/// The course title (ex. Intro/Computer Sci. Java (II))
		/// </summary>
		public string CourseTitle { get; init; }

		/// <summary>
		/// The term that this CAPE evaluation result was taken at (ex. SP20)
		/// </summary>
		public string Term { get; init; }

		/// <summary>
		/// The number of students that enrolled in this class.
		/// </summary>
		public int Enrolled { get; init; }

		/// <summary>
		/// The number of evaluations that were made for this class.
		/// </summary>
		public int EvalsMade { get; init; }

		/// <summary>
		/// The percent of people that would recommend the class. 
		/// </summary>
		public double RecommendClass { get; init; }

		/// <summary>
		/// The percent of people that would recommend the instructor. 
		/// </summary>
		public double RecommendInstructor { get; init; }

		/// <summary>
		/// The amount of studying the average student had to do (in hours) per week.
		/// </summary>
		public double StudyHrsWk { get; init; }

		/// <summary>
		/// The average grade that students expected.
		/// </summary>
		public double AverageGradeExpected { get; init; }

		/// <summary>
		/// The average grade that students received.
		/// </summary>
		public double AverageGradeReceived { get; init; }
	}
}