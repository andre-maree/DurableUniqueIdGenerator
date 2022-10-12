using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DurableUniqueIdGenerator.Helpers
{
    public static class WaitForResultHelper
    {
        private const int DefaultWaitForResultTime = 1500;

        public static void SetWaitForResult(this ref int? waitForResultMilliseconds)
        {
            waitForResultMilliseconds = waitForResultMilliseconds ?? DefaultWaitForResultTime;
        }
    }
}