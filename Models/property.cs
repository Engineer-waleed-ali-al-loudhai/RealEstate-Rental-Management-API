using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.Models
{
    public enum PropertyType
    {
        Apartment = 1,      // شقة
        CommercialShop = 2, // محل تجاري
        UnbuiltLand = 3     // أرض غير مبنية / أرض بيضاء
    }

    public enum PropertyStatus
    {
        Available = 1,     // متاح
        Rented = 2,        // مؤجر
        UnderDispute = 3   // تحت النزاع والمشاكل (خاص بوضع المحاكم لديك)
    }

    public class Property
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty; // عنوان العقار (مثال: أرض الجراف)

        [Required]
        public PropertyType Type { get; set; } // نوع العقار

        [Required]
        public string Description { get; set; } = string.Empty; // وصف الأرض وحدودها وصورتها الواقعية

        public string ImagePath { get; set; } = string.Empty; // رابط ملف الصورة على السيرفر

        [Required]
        public PropertyStatus Status { get; set; } = PropertyStatus.Available;

        [Required]
        public decimal PriceOrRent { get; set; } // القيمة المالية

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
