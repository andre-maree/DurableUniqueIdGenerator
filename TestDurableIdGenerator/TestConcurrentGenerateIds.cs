using System.Net.Http.Json;

namespace TestDurableIdGenerator
{
    [TestClass]
    public class TestConcurrentGenerateIds
    {
        /// <summary>
        /// 10 Parallel requests of ranges of 10000 new ids
        /// </summary>
        [TestMethod]
        public async Task ConcurrentGenerateIdsTest()
        {
            HttpClient httpClient = new();

            // reset mycounter to 0 with the MasterKey
            httpClient.DefaultRequestHeaders.Authorization = new("Bearer", "ZQ1iwmLGiVGchDpu7koAvGV9n5jNxsKA");

            var resetRes = await httpClient.GetFromJsonAsync<int>("http://localhost:7231/api/masterreset/mycounter/0/50000");

            Assert.IsTrue(resetRes == 0);

            // now use the GenerateIdsKey to parallel request new ranges of ids
            httpClient.DefaultRequestHeaders.Authorization = new("Bearer", "GzZEjv79DWcK2gVkTp3RKRoSDKJCdnwn");

            List<Task<Dictionary<string, int>>> tasks = new();

            // 10 parallel requests of ranges of 10000 new ids, wait 50000 to avoid a 202 return
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(httpClient.GetFromJsonAsync<Dictionary<string, int>>("http://localhost:7231/api/GenerateIds/mycounter/10000/50000"));
            }

            int count = 0;

            await Task.WhenAll(tasks);

            // now check every result`s range of new ids, all ids will be in the correct sequence with no duplicates or missing ids
            for (int i = 1; count < 10; count++)
            {
                var r1 = tasks.Single(r => r.Result["StartId"] == i);

                Assert.IsTrue(r1.Result["EndId"] == i + 9999);

                i += 10000;
            }
        }
    }
}