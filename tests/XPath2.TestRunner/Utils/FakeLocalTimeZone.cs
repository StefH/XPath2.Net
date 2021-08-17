using System;
using ReflectionMagic;

namespace XPath2.TestRunner.Utils
{
    /// <summary>
    /// https://stackoverflow.com/questions/44413407/mock-the-country-timezone-you-are-running-unit-test-from
    /// </summary>
    public class FakeLocalTimeZone : IDisposable
    {
        private readonly TimeZoneInfo _actualLocalTimeZoneInfo;

        private static void SetLocalTimeZone(TimeZoneInfo timeZoneInfo)
        {
            typeof(TimeZoneInfo).AsDynamicType().s_cachedData._localTimeZone = timeZoneInfo;
        }

        public FakeLocalTimeZone(TimeZoneInfo timeZoneInfo)
        {
            _actualLocalTimeZoneInfo = TimeZoneInfo.Local;
            SetLocalTimeZone(timeZoneInfo);
        }

        public void Dispose()
        {
            SetLocalTimeZone(_actualLocalTimeZoneInfo);
        }
    }
}