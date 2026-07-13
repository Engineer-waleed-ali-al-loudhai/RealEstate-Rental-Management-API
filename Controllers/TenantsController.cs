using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.Models;

namespace RealEstateApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner,Manager")] // محمي: فقط لصاحب الأملاك أو المدير
    public class TenantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TenantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // رابط إضافة مستأجر جديد
        [HttpPost("add")]
        public async Task<IActionResult> AddTenant(Tenant tenant)
        {
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم تسجيل المستأجر الجديد بنجاح في النظام!", tenantId = tenant.Id });
        }

        // رابط جلب وعرض قائمة جميع المستأجرين
        [HttpGet("all")]
        public async Task<IActionResult> GetAllTenants()
        {
            var tenants = await _context.Tenants.ToListAsync();
            return Ok(tenants);
        }
    }
}
