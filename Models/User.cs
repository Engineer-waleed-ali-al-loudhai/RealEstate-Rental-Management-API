
using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.Models
{
    public enum UserRole
    { 
        Owner = 1,        // صاحب الاملاك (صلاحيات كاملة ورؤية الارباح كاملة)ا
        Manager = 2,      // المدير او المحامي (إدارة الاراضي والايجارات والمحاكم)ا
        User = 3         //   مستخدم عادي او مستاجر ( رؤية فواتيرة وعقارة فقط دون تعديل)ا
    }

    public class User
    {
        [Key] 
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public byte[] PasswordHash { get; set; } = new byte[0];  //    كلمة المرور المشفرة بحماية قوية

        [Required]
        public byte[] PasswordSalt { get; set; } = new byte[0];   //   مفتاح التشفير الخاص بكل مستخدم

        [Required]
        public UserRole Role { get; set; } = UserRole.User;       //  الدور الافتراضي للمسجل الجديد


        //  حقل خاص بنظام التفعيل وحماية النسخة المدفوعة

        public bool IsAccountActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



    }
}
