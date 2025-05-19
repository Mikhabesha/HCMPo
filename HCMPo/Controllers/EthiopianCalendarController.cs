using Microsoft.AspNetCore.Mvc;
using HCMPo.Models;

namespace HCMPo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EthiopianCalendarController : ControllerBase
    {
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
    }

    public class EthiopianDateRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
    }
} 