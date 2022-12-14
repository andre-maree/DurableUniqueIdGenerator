using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace DurableUniqueIdGenerator
{
    public static class ListResourceCounters
    {
        [FunctionName("ListResourceCounters")]
        public static async Task<HttpResponseMessage> ListResourceCountersClient(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "ListResourceCounters")] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client)
        {
            try
            {
                // Check that the Authorization header is present in the HTTP request and that it is in the format of "Authorization: Bearer <token>"
                if (!req.CheckEitherGenerateIdsKeyOrMasterKey())
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                }

                EntityQuery queryDefinition = new()
                {
                    PageSize = 99999999,
                    FetchState = true,
                    EntityName = "ResourceCounter"
                };

                EntityQueryResult entityQueryResult = await client.ListEntitiesAsync(queryDefinition, default);

                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(entityQueryResult.Entities))
                };
            }
            catch (Exception ex)
            {
                // TODO: log error to table storage
                Console.WriteLine(ex.ToString());
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        }
    }
}