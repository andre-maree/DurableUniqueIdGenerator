using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace DurableUniqueIdGenerator
{
    public static class ResetResourceCounter
    {
        [FunctionName("MasterReset")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous,"get", "post", Route = "MasterReset/{resourceId}/{id}/{waitForResultMilliseconds?}")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter, string resourceId, int id, int? waitForResultMilliseconds)
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

            waitForResultMilliseconds = waitForResultMilliseconds ?? 1500;

            string instanceId = await starter.StartNewAsync("MasterResetOrchestration", null, (resourceId, id));

            return await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId, timeout: TimeSpan.FromMilliseconds(waitForResultMilliseconds.Value));
        }

        [FunctionName("MasterResetOrchestration")]
        public static async Task<int> MasterReset(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            (string resourceId, int id) = context.GetInput<(string resourceId, int id)>();

            // lock on the resource id string
            EntityId entityId = new("ResourceCounter", resourceId);

            // lock is not needed because enities always execute sequencially
            //using (await context.LockAsync(entityId))
            //{
                await context.CallEntityAsync(entityId, "Reset", id);
            //}

            return id;
        }
    }
}