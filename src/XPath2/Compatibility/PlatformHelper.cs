using System;

namespace Wmhelp.XPath2.Compatibility
{
    internal static class PlatformHelper
    {
        private static readonly Lazy<bool> IsRunningOnMonoValue = new Lazy<bool>(() => Type.GetType("Mono.Runtime") != null);

        public static bool IsRunningOnMono()
        {
            return IsRunningOnMonoValue.Value;
        }
    }
}