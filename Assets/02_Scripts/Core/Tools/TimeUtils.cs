using System;
using UnityEngine;

namespace StrangeSpace
{
    public class TimeUtils
    {
        public static string TicksToString(long links)
        {
 // Convert ticks to TimeSpan
           TimeSpan time = TimeSpan.FromTicks(links);
           
           // Start date: Year 0, Month 1, Day 1
           int year = 0;
           int month = 1;
           int day = 1;

           // First handle the days
           long totalDays = (long)time.TotalDays;

           // Helper for days in each month
           int[] daysInMonthCommon = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

           while (totalDays >= 365)
           {
               bool isLeap = IsLeapYear(year);
               if (totalDays >= (isLeap ? 366 : 365))
               {
                   totalDays -= isLeap ? 366 : 365;
                   year++;
               }
               else
               {
                   break;
               }
           }

           // Now calculate month and day
           while (true)
           {
               int daysThisMonth = daysInMonthCommon[month - 1];
               if (month == 2 && IsLeapYear(year)) daysThisMonth = 29;

               if (totalDays >= daysThisMonth)
               {
                   totalDays -= daysThisMonth;
                   month++;
                   if (month > 12)
                   {
                       month = 1;
                       year++;
                   }
               }
               else
               {
                   break;
               }
           }

           day += (int)totalDays;

           // Now add hours, minutes, seconds
           int hour = time.Hours;
           int minute = time.Minutes;
           int second = time.Seconds;

           return $"Year {year} Day {day:D3} Time {hour:D2}:{minute:D2}:{second:D2}";
        }
                
        private static bool IsLeapYear(int year)
        {
            // Gregorian leap year rule
            return (year % 4 == 0 && (year % 100 != 0 || year % 400 == 0));
        }
    }
}
