using System.Linq;
using System.Threading.Tasks;
using CoronaStatsBot.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoronaStatsBot.Tests
{
	[TestClass]
	public class DataProviderTests
	{
		[TestMethod]
		public async Task GetMessage_Provides()
		{
			var data = await DataProvider.GetMessage();
			Assert.IsTrue(data.Any());
		}
	}
}
