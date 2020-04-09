namespace CoronaStatsBot.Data
{
	public class CountryStats
	{
		public string Name { get; set; }

		public int TotalCases { get; set; }
		public int NewCases { get; set; }

		public int TotalDeaths { get; set; }
		public int NewDeaths { get; set; }
	}
}
