using System;

namespace BayesianPG.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly int[] DaysInNonLeapYearMonth = [ 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 ];

        public static int DaysInMonth(this DateTime date)
        {
            int daysInMonth = DateTimeExtensions.DaysInNonLeapYearMonth[date.Month - 1];
            if ((date.Month == 2) && DateTime.IsLeapYear(date.Year))
            {
                daysInMonth = 29;
            }
            return daysInMonth;
        }

        public static DateTime FromExcel(int excelDayNumber)
        {
            return new DateTime(1900, 1, 1).AddDays(excelDayNumber - 2);
        }
    }
}
