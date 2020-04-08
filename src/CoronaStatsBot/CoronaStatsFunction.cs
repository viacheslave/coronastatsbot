using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CoronaStatsBot
{
	public static class CoronaStatsFunction
	{
		[FunctionName("CoronaStatsFunction")]
		public static async void Run([TimerTrigger("0 0 0/6 ? * * *")]TimerInfo myTimer, ILogger log)
		{
			ChatId chatId = new ChatId(0); // your ID
			TelegramBotClient client = new TelegramBotClient(""); // your key

			var data = await GetMessage();
			await client.SendTextMessageAsync(chatId, data);

			log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
		}

		private async static Task<string> GetMessage()
		{
			var httpClient = new HttpClient();
			var message = await httpClient.GetAsync("https://www.worldometers.info/coronavirus/");
			var content = await message.Content.ReadAsStringAsync();

			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(content);

			var tableNode = doc.GetElementbyId("main_table_countries_today");
			var tbodyNode = tableNode.SelectSingleNode("tbody");
			var trNodes = tbodyNode.ChildNodes.Where(x => x.Name == "tr");

			var statsByCountry = GetStatsByCountry(trNodes.Skip(1).Select(x => x.InnerText))
				.OrderByDescending(x => x.NewDeaths)
				.Take(20);

			return string.Join('\n', statsByCountry.Select(x => $"{x.NewDeaths} ({x.Name} | {x.TotalCases})"));
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

		public class CountryStats
		{
			public string Name { get; set; }

			public int TotalCases { get; set; }
			public int NewCases { get; set; }

			public int TotalDeaths { get; set; }
			public int NewDeaths { get; set; }
		}
	}
}
