
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.Models;

namespace RealEstateApi.Services
{
    public class InvoiceHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        // نضبط المحرك ليفحص قاعدة البيانات كل 24 ساعة (مرة يومياً) للتأكد من التاريخ
        private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1);

        public InvoiceHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // التحقق: هل اليوم هو يوم 1 في الشهر الجديد؟
                if (DateTime.UtcNow.Day == 1)
                {
                    await GenerateMonthlyInvoicesAsync();
                }

                // انتظر لمدة يوم كامل قبل الفحص القادم لمنع الضغط على المعالج
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task GenerateMonthlyInvoicesAsync()
        {
            // بما أن الخدمة تعمل في الخلفية، نحتاج لإنشاء Scope مؤقت للوصول لقاعدة البيانات
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            // 1. جلب جميع عقود الإيجار النشطة حالياً في العقارات والشقق
            var activeLeases = await context.Leases
                .Where(l => l.IsActive && l.StartDate <= DateTime.UtcNow && l.EndDate >= DateTime.UtcNow)
                .ToListAsync();

            foreach (var lease in activeLeases)
            {
                // 2. الحماية من التكرار: التحقق من عدم توليد فاتورة لهذا العقد في هذا الشهر مسبقاً
                bool invoiceExists = await context.Invoices
                    .AnyAsync(i => i.LeaseId == lease.Id && i.DueDate == currentMonthStart);

                if (!invoiceExists)
                {
                    // 3. توليد فاتورة الإيجار الجديدة تلقائياً بناءً على قيمة عقد الإيجار
                    var newInvoice = new Invoice
                    {
                        LeaseId = lease.Id,
                        AmountDue = lease.MonthlyRent,
                        AmountPaid = 0,
                        DueDate = currentMonthStart,
                        Status = InvoiceStatus.Unpaid
                    };

                    context.Invoices.Add(newInvoice);
                }
            }

            // 4. حفظ جميع الفواتير الجديدة دفعة واحدة في SQL Server
            await context.SaveChangesAsync();
        }
    }
}
