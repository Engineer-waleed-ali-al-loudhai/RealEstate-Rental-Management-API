using System.ComponentModel.DataAnnotations;
using RealEstateApi.Models;

namespace RealEstateApi.DTOs
{
    public class UserRegisterDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty; // اسم المستخدم

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // البريد الإلكتروني

        [Required]
        [StringLength(100, MinimumLength = 6)] // الحد الأدنى لطلب كلمة المرور 6 خانات لأمان أعلى
        public string Password { get; set; } = string.Empty; // كلمة المرور

        [Required]
        public UserRole Role { get; set; } // تحديد الصلاحية (1=صاحب أملاك، 2=مدير، 3=مستخدم)
    }
}
