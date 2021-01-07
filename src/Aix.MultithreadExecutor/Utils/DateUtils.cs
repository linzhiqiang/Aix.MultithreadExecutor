using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.MultithreadExecutor.Utils
{
    internal static class DateUtils
    {
        public static long GetTimeStamp()
        {
            return GetTimeStamp(DateTime.Now);
        }

        public static long GetTimeStamp(DateTime now)
        {
            DateTime theDate = now;
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = theDate.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return (long)ts.TotalMilliseconds;

        }

        /// <summary>
        /// 时间戳转时间
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime TimeStampToDateTime(long timestamp)
        {
            //DateTime date = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            var date = TimeZoneInfo.ConvertTimeFromUtc(new System.DateTime(1970, 1, 1), TimeZoneInfo.Local);
            return date.AddMilliseconds(timestamp);
        }
    }
}
