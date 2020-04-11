using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CoronaStatsBot.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CoronaStatsBot
{
	public static class CoronaStatsFunction
	{
		[FunctionName("CoronaStatsFunction")]
		public static async void Run([TimerTrigger("0 0 */4 * * *")]TimerInfo myTimer, ILogger log)
		{
			ChatId chatId = new ChatId(0); // your ID
			TelegramBotClient client = new TelegramBotClient(null); // bot API key

			var data = await DataProvider.GetMessage();
			await client.SendTextMessageAsync(chatId, Format(data));

			log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
		}

		private static string Format(IEnumerable<CountryStats> data)
		{
			var collection = new List<string> { Format(data.First()), string.Empty };
			collection.AddRange(data.Skip(1).Select(Format));

			return string.Join('\n', collection);

			static string Format(CountryStats x)
			{
				return
					$"{x.TotalCases:n0} :: +{x.NewCases:n0} - " +
					$"{x.Name} - " +
					$"{x.TotalDeaths:n0} :: +{x.NewDeaths:n0}";
			}
		}
	}
}
