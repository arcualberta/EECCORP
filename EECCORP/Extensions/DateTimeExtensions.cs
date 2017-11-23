using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EECCORP.Extensions
{
    // from https://stackoverflow.com/questions/38039/how-can-i-get-the-datetime-for-the-start-of-the-week
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }
    }
}