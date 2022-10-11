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
            System.Net.Http.Headers.AuthenticationHeaderValue authorizationHeader = req.Headers.Authorization;

            // Check that the Authorization header is present in the HTTP request and that it is in the
            // format of "Authorization: Bearer <token>"
            if (authorizationHeader == null ||
                authorizationHeader.Scheme.CompareTo("Bearer") != 0 ||
                String.IsNullOrEmpty(authorizationHeader.Parameter) ||
                (!authorizationHeader.Parameter.Equals(Environment.GetEnvironmentVariable("MasterKey")) && !authorizationHeader.Parameter.Equals(Environment.GetEnvironmentVariable("GenerateIdsKey"))))
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }

            EntityQuery queryDefinition = new EntityQuery()
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
    }
}