using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace DurableUniqueIdGenerator
{
    public static class IdGenerator
    {
        [FunctionName("ListCounterResources")]
        public static async Task<HttpResponseMessage> ListCounterResources(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "ListCounterResources")] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client)
        {
            System.Net.Http.Headers.AuthenticationHeaderValue authorizationHeader = req.Headers.Authorization;

            // Check that the Authorization header is present in the HTTP request and that it is in the
            // format of "Authorization: Bearer <token>"
            if (authorizationHeader == null ||
                authorizationHeader.Scheme.CompareTo("Bearer") != 0 ||
                String.IsNullOrEmpty(authorizationHeader.Parameter) ||
                (!authorizationHeader.Parameter.Equals(Environment.GetEnvironmentVariable("MasterKey")) && !authorizationHeader.Parameter.Equals(Environment.GetEnvironmentVariable("GetIdsKey"))))
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

        [FunctionName("DeleteCounterResource")]
        public static async Task<HttpResponseMessage> DeleteCounterResource(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "DeleteCounterResource/{resourceId}")] HttpRequestMessage req,
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

        [Deterministic]
        [FunctionName("ResourceCounter")]
        public static void TableLockEntity([EntityTrigger] IDurableEntityContext ctx)
        {
            switch (ctx.OperationName)
            {
                case "Get":

                    int newId = ctx.GetState<int>() + ctx.GetInput<int>();

                    ctx.SetState(newId);
                    ctx.Return(newId);

                    break;

                case "Reset":

                    ctx.SetState(ctx.GetInput<int>());

                    break;

                case "Delete":

                    ctx.DeleteState();

                    break;
            }
        }

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

        [FunctionName("MasterResetOrchestration")]
        public static async Task<int> MasterReset(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            (string resourceId, int id) = context.GetInput<(string resourceId, int id)>();

            // lock on the target table
            EntityId entityId = new("ResourceCounter", resourceId);

            using (await context.LockAsync(entityId))
            {
                await context.CallEntityAsync(entityId, "Reset", id);
            }

            return id;
        }

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

            waitForResultMilliseconds = waitForResultMilliseconds.HasValue ? waitForResultMilliseconds.Value : 1500;

            string instanceId = await starter.StartNewAsync("MasterResetOrchestration", null, (resourceId, id));

            return await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId, timeout: TimeSpan.FromMilliseconds(waitForResultMilliseconds.Value));
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