using System;
using System.Collections.Generic;

namespace HCMPo.Helpers
{
    public static class EthiopianCalendarHelper
    {
        // Ethiopian calendar months in Amharic
        public static readonly Dictionary<int, string> AmharicMonths = new Dictionary<int, string>
        {
            { 1, "መስከረም" },   // September
            { 2, "ጥቅምት" },    // October
            { 3, "ህዳር" },     // November
            { 4, "ታህሳስ" },    // December
            { 5, "ጥር" },      // January
            { 6, "የካቲት" },    // February
            { 7, "መጋቢት" },    // March
            { 8, "ሚያዝያ" },    // April
            { 9, "ግንቦት" },    // May
            { 10, "ሰኔ" },     // June
            { 11, "ሀምሌ" },    // July
            { 12, "ነሐሴ" },    // August
            { 13, "ጳጉሜን" }   // Pagumen (13th month)
        };

        // Ethiopian calendar day names in Amharic
        public static readonly Dictionary<DayOfWeek, string> AmharicDays = new Dictionary<DayOfWeek, string>
        {
            { DayOfWeek.Sunday, "እሑድ" },
            { DayOfWeek.Monday, "ሰኞ" },
            { DayOfWeek.Tuesday, "ማክሰኞ" },
            { DayOfWeek.Wednesday, "ረቡዕ" },
            { DayOfWeek.Thursday, "ሐሙስ" },
            { DayOfWeek.Friday, "ዓርብ" },
            { DayOfWeek.Saturday, "ቅዳሜ" }
        };

        /// <summary>
        /// Converts Ethiopian date to Gregorian date
        /// </summary>
        public static DateTime ToGregorianDate(int ethYear, int ethMonth, int ethDay)
        {
            try
            {
                // Validate input parameters
                if (ethYear < 1900 || ethYear > 2100)
                {
                    return new DateTime(2000, 1, 1); // Return a default Gregorian date
                }
                
                if (ethMonth < 1 || ethMonth > 13)
                {
                    return new DateTime(2000, 1, 1); // Return a default Gregorian date
                }
                
                if (ethDay < 1 || ethDay > GetDaysInEthiopianMonth(ethYear, ethMonth))
                {
                    return new DateTime(2000, 1, 1); // Return a default Gregorian date
                }
                
                // Simple approximation: Ethiopian year starts about 7-8 years behind Gregorian
                // and about 8-9 months ahead in the Gregorian calendar year
                
                int gregorianYear = ethYear + 7;
                
                // Validate the year for DateTime constructor
                if (gregorianYear < 1 || gregorianYear > 9999)
                {
                    return new DateTime(2000, 1, 1); // Return a default Gregorian date
                }
                
                // Ethiopian New Year (1 Meskerem) falls around September 11 in Gregorian calendar
                var ethiopianNewYear = new DateTime(gregorianYear, 9, 11);
                
                // Calculate total days from Ethiopian New Year
                int totalDays = 0;
                
                // Add days for complete months
                for (int month = 1; month < ethMonth; month++)
                {
                    totalDays += GetDaysInEthiopianMonth(ethYear, month);
                }
                
                // Add remaining days
                totalDays += ethDay - 1;
                
                return ethiopianNewYear.AddDays(totalDays);
            }
            catch (Exception)
            {
                // Return a default Gregorian date if conversion fails
                return new DateTime(2000, 1, 1);
            }
        }

        /// <summary>
        /// Converts Gregorian date to Ethiopian date with null handling
        /// </summary>
        public static (int Year, int Month, int Day) FromGregorianDate(DateTime? gregorianDate)
        {
            if (!gregorianDate.HasValue || gregorianDate.Value == DateTime.MinValue || gregorianDate.Value == DateTime.MaxValue)
            {
                return (2000, 1, 1); // Return a default Ethiopian date
            }
            
            return FromGregorianDate(gregorianDate.Value);
        }

        /// <summary>
        /// Converts Gregorian date to Ethiopian date
        /// </summary>
        public static (int Year, int Month, int Day) FromGregorianDate(DateTime gregorianDate)
        {
            try
            {
                // Handle edge cases
                if (gregorianDate == DateTime.MinValue || gregorianDate == DateTime.MaxValue)
                {
                    return (2000, 1, 1); // Return a default Ethiopian date
                }

                // Validate input date range
                if (gregorianDate.Year < 1900 || gregorianDate.Year > 2100)
                {
                    return (2000, 1, 1); // Return a default Ethiopian date for out-of-range years
                }

                // Simple conversion - can be improved for more accuracy
                int ethYear = gregorianDate.Year - 7;
                
                // Adjust for Ethiopian calendar starting in September
                if (gregorianDate.Month >= 9)
                {
                    ethYear = gregorianDate.Year - 7;
                }
                else
                {
                    ethYear = gregorianDate.Year - 8;
                }
                
                // Validate Ethiopian year
                if (ethYear < 1900 || ethYear > 2100)
                {
                    return (2000, 1, 1); // Return a default Ethiopian date
                }
                
                // Ethiopian New Year for this Ethiopian year
                var gregorianYearForEthiopianNewYear = ethYear + 7;
                
                // Validate the year for DateTime constructor
                if (gregorianYearForEthiopianNewYear < 1 || gregorianYearForEthiopianNewYear > 9999)
                {
                    return (2000, 1, 1); // Return a default Ethiopian date
                }
                
                var ethiopianNewYear = new DateTime(gregorianYearForEthiopianNewYear, 9, 11);
                
                // Calculate days since Ethiopian New Year
                var daysSinceNewYear = (int)(gregorianDate - ethiopianNewYear).TotalDays;
                
                if (daysSinceNewYear < 0)
                {
                    // Date is in previous Ethiopian year
                    ethYear--;
                    
                    // Validate the new Ethiopian year
                    if (ethYear < 1900 || ethYear > 2100)
                    {
                        return (2000, 1, 1); // Return a default Ethiopian date
                    }
                    
                    gregorianYearForEthiopianNewYear = ethYear + 7;
                    
                    // Validate the year for DateTime constructor
                    if (gregorianYearForEthiopianNewYear < 1 || gregorianYearForEthiopianNewYear > 9999)
                    {
                        return (2000, 1, 1); // Return a default Ethiopian date
                    }
                    
                    ethiopianNewYear = new DateTime(gregorianYearForEthiopianNewYear, 9, 11);
                    daysSinceNewYear = (int)(gregorianDate - ethiopianNewYear).TotalDays;
                }
                
                // Calculate month and day
                int ethMonth = 1;
                while (daysSinceNewYear >= GetDaysInEthiopianMonth(ethYear, ethMonth))
                {
                    daysSinceNewYear -= GetDaysInEthiopianMonth(ethYear, ethMonth);
                    ethMonth++;
                    
                    if (ethMonth > 13)
                    {
                        ethMonth = 1;
                        ethYear++;
                        
                        // Validate the new Ethiopian year
                        if (ethYear > 2100)
                        {
                            return (2000, 1, 1); // Return a default Ethiopian date
                        }
                        break;
                    }
                }
                
                int ethDay = daysSinceNewYear + 1;
                
                // Validate the calculated Ethiopian date
                if (ethDay < 1 || ethDay > GetDaysInEthiopianMonth(ethYear, ethMonth))
                {
                    return (2000, 1, 1); // Return a default Ethiopian date
                }
                
                return (ethYear, ethMonth, ethDay);
            }
            catch (Exception)
            {
                // Return a default Ethiopian date if conversion fails
                return (2000, 1, 1);
            }
        }

        /// <summary>
        /// Gets number of days in Ethiopian month
        /// </summary>
        public static int GetDaysInEthiopianMonth(int ethYear, int ethMonth)
        {
            if (ethMonth >= 1 && ethMonth <= 12)
                return 30;
            else if (ethMonth == 13)
                return IsEthiopianLeapYear(ethYear) ? 6 : 5;
            else
                throw new ArgumentException("Invalid Ethiopian month");
        }

        /// <summary>
        /// Checks if Ethiopian year is leap year
        /// </summary>
        public static bool IsEthiopianLeapYear(int ethYear)
        {
            // Ethiopian leap year occurs every 4 years
            return (ethYear % 4) == 3;
        }

        /// <summary>
        /// Gets Amharic month name
        /// </summary>
        public static string GetAmharicMonthName(int month)
        {
            return AmharicMonths.ContainsKey(month) ? AmharicMonths[month] : month.ToString();
        }

        /// <summary>
        /// Gets Amharic day name
        /// </summary>
        public static string GetAmharicDayName(DayOfWeek dayOfWeek)
        {
            return AmharicDays[dayOfWeek];
        }

        /// <summary>
        /// Formats Ethiopian date in Amharic
        /// </summary>
        public static string FormatEthiopianDate(int year, int month, int day)
        {
            var gregorianDate = ToGregorianDate(year, month, day);
            return $"{GetAmharicDayName(gregorianDate.DayOfWeek)}፣ {GetAmharicMonthName(month)} {day}፣ {year}";
        }

        /// <summary>
        /// Gets current Ethiopian date
        /// </summary>
        public static (int Year, int Month, int Day) GetCurrentEthiopianDate()
        {
            return FromGregorianDate(DateTime.Today);
        }

        /// <summary>
        /// Gets list of months for dropdown
        /// </summary>
        public static List<(int Value, string Text)> GetMonthsForDropdown()
        {
            var months = new List<(int Value, string Text)>();
            for (int i = 1; i <= 13; i++)
            {
                months.Add((i, $"{i} - {AmharicMonths[i]}"));
            }
            return months;
        }

        /// <summary>
        /// Gets list of years for dropdown
        /// </summary>
        public static List<int> GetYearsForDropdown(int startYear = 2017, int endYear = 2030)
        {
            var years = new List<int>();
            
            for (int year = startYear; year <= endYear; year++)
            {
                years.Add(year);
            }
            return years;
        }

        /// <summary>
        /// Validates Ethiopian date
        /// </summary>
        public static bool IsValidEthiopianDate(int year, int month, int day)
        {
            if (year < 1 || month < 1 || month > 13 || day < 1)
                return false;
                
            var daysInMonth = GetDaysInEthiopianMonth(year, month);
            return day <= daysInMonth;
        }

        /// <summary>
        /// Converts Ethiopian date to short string format
        /// </summary>
        public static string ToShortDateString(int year, int month, int day)
        {
            return $"{day:D2}/{month:D2}/{year}";
        }

        /// <summary>
        /// Formats a Gregorian date to show both Ethiopian and Gregorian
        /// </summary>
        public static string FormatDateWithEthiopian(DateTime gregorianDate, string format = "MMM dd, yyyy")
        {
            try
            {
                // Check if the date is valid
                if (gregorianDate == DateTime.MinValue || gregorianDate == DateTime.MaxValue)
                {
                    return "Invalid Date";
                }

                var ethiopianDate = FromGregorianDate(gregorianDate);
                var ethiopianFormatted = FormatEthiopianDate(ethiopianDate.Year, ethiopianDate.Month, ethiopianDate.Day);
                var gregorianFormatted = gregorianDate.ToString(format);
                
                return $"{gregorianFormatted}<br><small class='text-muted'>({ethiopianFormatted})</small>";
            }
            catch (Exception)
            {
                // If Ethiopian conversion fails, just return the Gregorian date
                return gregorianDate.ToString(format);
            }
        }

        /// <summary>
        /// Formats a Gregorian date to show Ethiopian date only
        /// </summary>
        public static string FormatEthiopianOnly(DateTime gregorianDate)
        {
            var ethiopianDate = FromGregorianDate(gregorianDate);
            return FormatEthiopianDate(ethiopianDate.Year, ethiopianDate.Month, ethiopianDate.Day);
        }

        /// <summary>
        /// Formats a Gregorian date to show Ethiopian date with short format
        /// </summary>
        public static string FormatEthiopianShort(DateTime gregorianDate)
        {
            var ethiopianDate = FromGregorianDate(gregorianDate);
            return $"{GetAmharicMonthName(ethiopianDate.Month)} {ethiopianDate.Day}, {ethiopianDate.Year}";
        }

        /// <summary>
        /// Formats a Gregorian date to show both Ethiopian and Gregorian in a compact format
        /// </summary>
        public static string FormatDateCompact(DateTime gregorianDate)
        {
            var ethiopianDate = FromGregorianDate(gregorianDate);
            var ethiopianShort = $"{ethiopianDate.Month}/{ethiopianDate.Day}/{ethiopianDate.Year}";
            var gregorianShort = gregorianDate.ToString("MM/dd/yyyy");
            
            return $"{gregorianShort} <small class='text-muted'>({ethiopianShort})</small>";
        }

        /// <summary>
        /// Formats a nullable Gregorian date
        /// </summary>
        public static string FormatDateWithEthiopian(DateTime? gregorianDate, string format = "MMM dd, yyyy")
        {
            if (!gregorianDate.HasValue)
                return "N/A";
                
            return FormatDateWithEthiopian(gregorianDate.Value, format);
        }

        /// <summary>
        /// Formats a nullable Gregorian date for Ethiopian only
        /// </summary>
        public static string FormatEthiopianOnly(DateTime? gregorianDate)
        {
            if (!gregorianDate.HasValue)
                return "N/A";
                
            return FormatEthiopianOnly(gregorianDate.Value);
        }
    }
} 