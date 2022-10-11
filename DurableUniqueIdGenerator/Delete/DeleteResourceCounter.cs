using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace DurableUniqueIdGenerator
{
    public static class DeleteResourceCounter
    {
        [FunctionName("DeleteResourceCounter")]
        public static async Task<HttpResponseMessage> DeleteResourceCounterClient(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "DeleteResourceCounter/{resourceId}")] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client, string resourceId)
        {
            System.Net.Http.Headers.AuthenticationHeaderValue authorizationHeader = req.Headers.Authorization;

            // Check that the Authorization header is present in the HTTP request and that it is in the
            // format of "Authorization: Bearer <token>"
            if (authorizationHeader == null ||
                authorizationHeader.Scheme.CompareTo("Bearer") != 0 ||
                String.IsNullOrEmpty(authorizationHeader.Parameter) ||
                !authorizationHeader.Parameter.Equals(Environment.GetEnvironmentVariable("MasterKey")))
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }

            EntityId entityId = new("ResourceCounter", resourceId);

            await client.SignalEntityAsync(entityId, "Delete");

            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
    }
}