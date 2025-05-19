using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HCMPo.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HCMPo.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class PayrollConfigurationController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PayrollConfigurationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var config = await _context.PayrollConfigurations.FirstOrDefaultAsync();
            if (config == null)
                config = new HCMPo.Models.PayrollConfiguration();
            return View(config);
        }
    }
} 