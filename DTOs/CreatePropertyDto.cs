using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using RealEstateApi.Models;

namespace RealEstateApi.DTOs
{
    public class CreatePropertyDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public PropertyType Type { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal PriceOrRent { get; set; }

        // حقل استقبال ملف الصورة الفعلي من جهاز المستخدم
        public IFormFile? ImageFile { get; set; }
    }
}
