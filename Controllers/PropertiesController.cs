using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.DTOs;
using RealEstateApi.Models;

namespace RealEstateApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PropertiesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // 1. رابط إضافة عقار أو أرض (محمي: فقط لصاحب الملك أو المدير)
        [HttpPost("add")]
        [Authorize(Roles = "Owner,Manager")] // قفل أمني صارم بناءً على الأدوار
        [Consumes("multipart/form-data")] // لاستقبال الملفات والصور
        public async Task<IActionResult> AddProperty([FromForm] CreatePropertyDto dto)
        {
            string uniqueFileName = string.Empty;

            // معالجة وحفظ صورة الأرض في مجلد السيرفر المحلي
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // دمج الـ Guid لمنع تداخل أسماء الصور
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(dto.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(fileStream);
                }
            }

            var property = new Property
            {
                Title = dto.Title,
                Type = dto.Type,
                Description = dto.Description,
                PriceOrRent = dto.PriceOrRent,
                ImagePath = uniqueFileName != string.Empty ? $"/uploads/{uniqueFileName}" : string.Empty,
                Status = PropertyStatus.Available
            };

            _context.properties.Add(property);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إضافة العقار وصورته بنجاح تام في النظام المدفوع!", propertyId = property.Id, path = property.ImagePath });
        }

        // رابط عرض وجلب جميع العقارات والأراضي (متاح لكل مستخدم مسجل)
        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAllProperties()
        {
            var properties = await _context.properties.ToListAsync();

            // جلب رابط السيرفر الحالي تلقائياً (سواء كان localhost أو دومين على الإنترنت)
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            // تحويل مسارات الصور إلى روابط كاملة قابلة للفتح مباشرة في المتصفح
            var result = properties.Select(p => new
            {
                p.Id,
                p.Title,
                p.Type,
                TypeName = p.Type.ToString(), // لإظهار الكلمة بالعربية أو الإنجليزية في الشاشة
                p.Description,
                p.PriceOrRent,
                p.Status,
                StatusName = p.Status.ToString(),
                ImageUrl = !string.IsNullOrEmpty(p.ImagePath) ? baseUrl + p.ImagePath : "" // رابط الصورة الكامل
            });

            return Ok(result);
        }
    }
}
