using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableUniqueIdGenerator.Entites
{
    public static class ResourceCounterEntity
    {

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
    }
}
