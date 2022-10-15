using System;
using System.Net.Http;
using System.Threading.Tasks;
using DurableUniqueIdGenerator.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace DurableUniqueIdGenerator
{
    public static class IdGenerator
    {
        [FunctionName("GenerateIds")]
        public static async Task<HttpResponseMessage> GenerateIds(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "GenerateIds/{resourceId}/{count}/{waitForResultMilliseconds?}")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter, string resourceId, int count, int? waitForResultMilliseconds)
        {
            try
            {
                System.Net.Http.Headers.AuthenticationHeaderValue authorizationHeader = req.Headers.Authorization;

                // Check that the Authorization header is present in the HTTP request and that it is in the
                // format of "Authorization: Bearer <token>"
                if (authorizationHeader == null ||
                    authorizationHeader.Scheme.CompareTo("Bearer") != 0 ||
                    String.IsNullOrEmpty(authorizationHeader.Parameter) ||
                    !authorizationHeader.Parameter.Equals(Environment.GetEnvironmentVariable("GenerateIdsKey")))
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                }

                waitForResultMilliseconds = waitForResultMilliseconds ?? 1500;

                string instanceId = await starter.StartNewAsync("GenerateIdsOrchestration", null, (resourceId, count));

                return await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId, timeout: TimeSpan.FromMilliseconds(waitForResultMilliseconds.Value));
            }
            catch (Exception ex)
            {
                // TODO: log error to table storage
                Console.WriteLine(ex.ToString());
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        }

        [Deterministic]
        [FunctionName("GenerateIdsOrchestration")]
        public static async Task<GenerateResult> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            (string resourceId, int count) = context.GetInput<(string resourceId, int count)>();

            EntityId entityId = new("ResourceCounter", resourceId);
            
            // a lock is not needed because enities always execute sequencially
            int id = await context.CallEntityAsync<int>(entityId, "Get", count);

            return new GenerateResult()
            {
                StartId = id - (count - 1),
                EndId = id
            };
        }
    }
}