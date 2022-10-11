using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;

namespace DurableUniqueIdGenerator.Entites
{
    public static class ResourceCounter
    {
        [Deterministic]
        [FunctionName("ResourceCounter")]
        public static void ResourceCounterEntity([EntityTrigger] IDurableEntityContext ctx)
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
    }
}
