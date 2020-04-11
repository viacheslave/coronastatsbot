using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CoronaStatsBot.Data
{
	public static class DataProvider
	{
		public async static Task<IEnumerable<CountryStats>> GetMessage()
		{
			var httpClient = new HttpClient();
			var message = await httpClient.GetAsync("https://www.worldometers.info/coronavirus/");
			var content = await message.Content.ReadAsStringAsync();

			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(content);

			var tableNode = doc.GetElementbyId("main_table_countries_today");
			var tbodyNode = tableNode.SelectSingleNode("tbody");
			var trNodes = tbodyNode.ChildNodes.Where(x => x.Name == "tr");

			var statsByCountry = GetStatsByCountry(trNodes.Select(x => x.InnerText))
				.Skip(7)
				.OrderByDescending(x => x.TotalCases)
				.Take(21);

			return statsByCountry;
		}

		private static IReadOnlyList<CountryStats> GetStatsByCountry(IEnumerable<string> lines)
		{
			var culture = CultureInfo.GetCultureInfo("en-US");
			const NumberStyles numberStyles = NumberStyles.AllowThousands
				| NumberStyles.AllowLeadingSign
				| NumberStyles.AllowTrailingWhite;

			var stats = new List<CountryStats>();

			foreach (var line in lines)
			{
				var parts = line.Split('\n');

				int.TryParse(parts[2], numberStyles, culture, out var tc);
				int.TryParse(parts[3], numberStyles, culture, out var nc);
				int.TryParse(parts[4], numberStyles, culture, out var td);
				int.TryParse(parts[5], numberStyles, culture, out var nd);

				var statsItem = new CountryStats
				{
					Name = parts[1],
					TotalCases = tc,
					NewCases = nc,
					TotalDeaths = td,
					NewDeaths = nd
				};

				stats.Add(statsItem);
			}

			return stats;
		}
	}
}
