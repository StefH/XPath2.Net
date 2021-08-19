using System.Globalization;
using System.Reflection;

namespace XPath2.TestRunner.Utils
{
    public static class GlobalizationUtils
    {
        public static bool UseNls()
        {
            return (typeof(CultureInfo).Assembly.GetType("System.Globalization.GlobalizationMode")
                ?.GetProperty("UseNls", BindingFlags.Static | BindingFlags.NonPublic)
                ?.GetValue(null) as bool?) == true;
        }
    }
}