using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using System;

namespace DurableUniqueIdGenerator.Entites
{
    public static class ResourceCounter
    {
        [FunctionName("ResourceCounter")]
        public static void ResourceCounterEntity([EntityTrigger] IDurableEntityContext ctx)
        {
            switch (ctx.OperationName)
            {
                case "Get":

                    int count = ctx.GetInput<int>();
                    int endId = ctx.GetState<int>() + count;

                    ctx.SetState(endId);
                    ctx.Return(endId);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"New id range generated: [{endId - (count - 1)} - {endId}]");

                    break;

                case "Reset":

                    ctx.SetState(ctx.GetInput<int>());

                    break;

                case "Delete":

                    ctx.DeleteState();

                    break;
            }
        }
    }
}
