using System;
using Microsoft.Extensions.Logging;

namespace HCMPo.Models
{
    public class EthiopianCalendar
    {
        private static readonly int[] NumberOfDaysInMonth = { 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 5 };
        private const int EthiopianEpoch = 1723856; // JDN for 1/1/1 Ethiopian
        private readonly ILogger<EthiopianCalendar> _logger;

        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }

        public EthiopianCalendar(DateTime gregorianDate)
        {
            var jdn = ToJulianDayNumber(gregorianDate);
            FromJulianDayNumber(jdn);
        }

        public EthiopianCalendar(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        private static int ToJulianDayNumber(DateTime date)
        {
            int a = (14 - date.Month) / 12;
            int y = date.Year + 4800 - a;
            int m = date.Month + 12 * a - 3;

            int jdn = date.Day + (153 * m + 2) / 5 + 365 * y + y / 4 - y / 100 + y / 400 - 32045;
            return jdn;
        }

        private void FromJulianDayNumber(int jdn)
        {
            int r = (jdn - EthiopianEpoch) % 1461;
            int n = r % 365 + 365 * (r / 1460);

            Year = 4 * ((jdn - EthiopianEpoch) / 1461) + r / 365 - r / 1460;
            Month = n / 30 + 1;
            Day = (n % 30) + 1;
        }

        public DateTime ToGregorianDate()
        {
            int jdn = EthiopianToJDN(Year, Month, Day);
            return JDNToGregorian(jdn);
        }

        private static int EthiopianToJDN(int year, int month, int day)
        {
            // Adjusted formula to fix off-by-one-year issue
            return EthiopianEpoch + 365 * year + (year / 4) + 30 * (month - 1) + (day - 1);
        }

        private static DateTime JDNToGregorian(int jdn)
        {
            int a = jdn + 32044;
            int b = (4 * a + 3) / 146097;
            int c = a - (146097 * b) / 4;
            int d = (4 * c + 3) / 1461;
            int e = c - (1461 * d) / 4;
            int m = (5 * e + 2) / 153;

            int day = e - (153 * m + 2) / 5 + 1;
            int month = m + 3 - 12 * (m / 10);
            int year = 100 * b + d - 4800 + m / 10;

            return new DateTime(year, month, day);
        }

        public static string GetEthiopianMonthName(int month)
        {
            string[] monthNames = {
                "መስከረም", "ጥቅምት", "ኅዳር", "ታኅሳስ", "ጥር", "የካቲት",
                "መጋቢት", "ሚያዝያ", "ግንቦት", "ሰኔ", "ሐምሌ", "ነሐሴ", "ጳጉሜን"
            };
            return monthNames[month - 1];
        }

        public static string FormatEthiopianDate(int year, int month, int day)
        {
            return $"{year} {GetEthiopianMonthName(month)} {day:D2}";
        }
    }
} 