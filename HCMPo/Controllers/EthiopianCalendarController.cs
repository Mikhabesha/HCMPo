using Microsoft.AspNetCore.Mvc;
using HCMPo.Models;
using System;

namespace HCMPo.Controllers
{
    public class EthiopianCalendarController : Controller
    {
        [HttpGet]
        public IActionResult ConvertEthiopianToGregorian(int year, int month, int day)
        {
            try
            {
                var ethiopianDate = new EthiopianCalendar(year, month, day);
                var gregorianDate = ethiopianDate.ToGregorianDate();
                return Json(new { success = true, gregorianDate = gregorianDate.ToString("yyyy-MM-dd"), formattedGregorianDate = gregorianDate.ToString("MMM d, yyyy") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("ToGregorian")]
        public IActionResult ToGregorian([FromBody] EthiopianDateRequest request)
        {
            try
            {
                var ethiopianDate = new EthiopianCalendar(request.Year, request.Month, request.Day);
                var gregorianDate = ethiopianDate.ToGregorianDate();
                return Ok(gregorianDate.ToString("yyyy-MM-dd"));
            }
            catch
            {
                return BadRequest("Invalid Ethiopian date");
            }
        }

        [HttpGet("TestConversion")]
        public IActionResult TestConversion(int year = 2017, int month = 10, int day = 3)
        {
            try
            {
                var ethiopianDate = new EthiopianCalendar(year, month, day);
                var gregorianDate = ethiopianDate.ToGregorianDate();
                
                return Ok(new
                {
                    EthiopianDate = $"{year}/{month}/{day}",
                    GregorianDate = gregorianDate.ToString("yyyy-MM-dd"),
                    DayOfWeek = gregorianDate.DayOfWeek.ToString()
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }

    public class EthiopianDateRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
    }
} 