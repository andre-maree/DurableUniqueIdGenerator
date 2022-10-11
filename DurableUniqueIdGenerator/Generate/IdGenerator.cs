using System;
using System.Net.Http;
using System.Threading.Tasks;
using DurableUniqueIdGenerator.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace DurableUniqueIdGenerator
{
    public static class IdGenerator
    {
        [FunctionName("GenerateIdsOrchestration")]
        public static async Task<GenerateResult> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            (string resourceId, int count) = context.GetInput<(string resourceId, int count)>();

            // lock on the target table
            EntityId entityId = new("ResourceCounter", resourceId);
            int id = -1;

            using (await context.LockAsync(entityId))
            {
                id = await context.CallEntityAsync<int>(entityId, "Get", count);
            }

            return new GenerateResult()
            {
                StartId = id - (count - 1),
                EndId = id
            };
        }
        
        [FunctionName("GenerateIds")]
        public static async Task<HttpResponseMessage> GenerateIds(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "GenerateIds/{resourceId}/{count}/{waitForResultMilliseconds?}")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter, string resourceId, int count, int? waitForResultMilliseconds)
        {
            System.Net.Http.Headers.AuthenticationHeaderValue authorizationHeader = req.Headers.Authorization;

            // Check that the Authorization header is present in the HTTP request and that it is in the
            // format of "Authorization: Bearer <token>"
            if (authorizationHeader == null ||
                authorizationHeader.Scheme.CompareTo("Bearer") != 0 ||
                String.IsNullOrEmpty(authorizationHeader.Parameter) ||
                !authorizationHeader.Parameter.Equals(Environment.GetEnvironmentVariable("GetIdsKey")))
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }

            waitForResultMilliseconds = waitForResultMilliseconds.HasValue ? waitForResultMilliseconds.Value : 1500;

            string instanceId = await starter.StartNewAsync("GenerateIdsOrchestration", null, (resourceId, count));

            return await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId, timeout: TimeSpan.FromMilliseconds(waitForResultMilliseconds.Value));
        }
    }
}