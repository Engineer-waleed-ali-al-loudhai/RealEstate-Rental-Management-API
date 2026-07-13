using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.Models;

namespace RealEstateApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // يتطلب تسجيل الدخول أولاً
    public class LeasesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. رابط إنشاء عقد إيجار جديد (متاح للمدير والمالك فقط)
        [HttpPost("create")]
        [Authorize(Roles = "Owner,Manager")]
        public async Task<IActionResult> CreateLease(Lease lease)
        {
            // تغيير حالة العقار تلقائياً إلى "مؤجر" بمجرد توقيع العقد
            var property = await _context.Properties.FindAsync(lease.PropertyId);
            if (property != null)
            {
                property.Status = PropertyStatus.Rented;
            }

            _context.Leases.Add(lease);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إنشاء عقد الإيجار وتأجير العقار برمجياً!", leaseId = lease.Id });
        }

        // 2. شاشة التقارير المالية المحمية (متاحة حصرياً لصاحب الأملاك Owner مثل والدك)
        [HttpGet("financial-report")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetFinancialReport()
        {
            var invoices = await _context.Invoices.Include(i => i.Lease).ThenInclude(l => l!.Property).ToListAsync();

            var totalExpected = invoices.Sum(i => i.AmountDue);
            var totalCollected = invoices.Sum(i => i.AmountPaid);
            var totalOutstanding = totalExpected - totalCollected; // المبالغ المتأخرة في السوق

            return Ok(new
            {
                TotalExpectedRevenue = totalExpected,
                TotalCollectedRevenue = totalCollected,
                TotalUnpaidDebts = totalOutstanding, // الديون المتأخرة عند المستأجرين المتهربين
                DetailedInvoices = invoices.Select(i => new {
                    i.Id,
                    PropertyTitle = i.Lease?.Property?.Title,
                    i.AmountDue,
                    i.AmountPaid,
                    i.DueDate,
                    Status = i.Status.ToString()
                })
            });
        }
    }
}
