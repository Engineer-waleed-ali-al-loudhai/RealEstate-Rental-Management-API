
using Microsoft.EntityFrameworkCore;

using RealEstateApi.Models;

namespace RealEstateApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


        //   هذا السطر يخبر السيرفر بأنشاء جدول المستخدمين والصلاحيات
        public DbSet<User> Users { get; set; }

        public DbSet<Property> properties { get; set; }
    }
}
