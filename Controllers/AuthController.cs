using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RealEstateApi.Data;
using RealEstateApi.DTOs;
using RealEstateApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RealEstateApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. رابط تسجيل مستخدم جديد مع تشفير كلمة المرور وحمايتها
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            // التحقق من أن اسم المستخدم غير مكرر في النظام
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
            {
                return BadRequest("اسم المستخدم هذا مسجل مسبقاً في النظام.");
            }

            // توليد تشفير وتمليح معقد لكلمة المرور لحماية تامة
            using var hmac = new HMACSHA512();

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password)),
                PasswordSalt = hmac.Key,
                Role = request.Role,
                IsAccountActive = true // الإعداد الافتراضي لتفعيل الحساب عند الصنع
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("تم تسجيل المستخدم الجديد بنجاح وحماية حسابه برمجياً.");
        }

        // 2. رابط تسجيل الدخول وتوليد مفتاح الـ Token للمستخدم
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());
            if (user == null)
            {
                return BadRequest("اسم المستخدم أو كلمة المرور غير صحيحة.");
            }

            // فك تشفير كلمة المرور ومقارنتها بما هو مخزن بقاعدة البيانات
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                    return BadRequest("اسم المستخدم أو كلمة المرور غير صحيحة.");
            }

            // حماية النسخة المدفوعة: إذا تم إيقاف الحساب لعدم السداد يتوقف النظام فوراً
            if (!user.IsAccountActive)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "هذا الحساب تم إيقافه مؤقتاً، يرجى تفعيل النسخة المدفوعة من المطور.");
            }

            // توليد مفتاح الحماية الرقمي المشفر
            string token = CreateToken(user);
            return Ok(new { message = "تم تسجيل الدخول بنجاح تام!", token = token });
        }

        // ميثود داخلية لتوليد وصناعة الـ JWT Token المشفر الحامل للصلاحيات
        private string CreateToken(User user)
        {
            // حفظ هوية وصلاحية المستخدم داخل المفتاح بشكل آمن ومشفر
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()) // تخزين الدور: Owner أو Manager أو User
            };

            var keySecret = _configuration.GetSection("AppSettings:Token").Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keySecret ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7), // صلاحية المفتاح تنتهي تلقائياً بعد 7 أيام لحماية النظام
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
