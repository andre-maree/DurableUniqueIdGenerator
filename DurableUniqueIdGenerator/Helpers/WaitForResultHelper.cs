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