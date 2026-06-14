using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using EcommerceApi.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IOtpService _otpService;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, IOtpService otpService, IJwtService jwtService, IEmailService emailService, IConfiguration config)
        {
            _db = db;
            _otpService = otpService;
            _jwtService = jwtService;
            _emailService = emailService;
            _config = config;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Phone) || request.Phone.Length < 10)
                return BadRequest(new { message = "Invalid phone number" });

            var code = await _otpService.SendOtp(request.Phone);
            return Ok(new { message = "OTP sent successfully", dev_otp = code });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var isValid = await _otpService.VerifyOtp(request.Phone, request.Code);
            if (!isValid)
                return Unauthorized(new { message = "Invalid or expired OTP" });

            // Find or create user
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone);
            if (user == null)
            {
                user = new User
                {
                    Phone = request.Phone,
                    Name = "User",
                    Role = "user"
                };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                // Send welcome email if they have one later
            }

            var token = _jwtService.GenerateToken(user.Id, user.Phone, user.Role);
            return Ok(new AuthResponse(token, MapUserDto(user)));
        }

        [HttpPost("admin-register")]
        public async Task<IActionResult> AdminRegister([FromBody] AdminRegisterRequest request)
        {
            // Validate invite code
            var validCodes = _config.GetSection("AdminInviteCodes").Get<string[]>() ?? Array.Empty<string>();
            if (!validCodes.Contains(request.InviteCode))
                return StatusCode(403, new { message = "Invalid invite code" });

            // Check if email already exists
            if (await _db.Users.AnyAsync(u => u.Email == request.Email))
                return Conflict(new { message = "Email already registered" });

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = HashPassword(request.Password),
                Role = "admin"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user.Id, user.Phone, user.Role);
            return Created("", new AuthResponse(token, MapUserDto(user)));
        }

        [HttpPost("admin-login")]
        public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.Role == "admin");
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials" });

            var token = _jwtService.GenerateToken(user.Id, user.Phone, user.Role);
            return Ok(new AuthResponse(token, MapUserDto(user)));
        }
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            return Ok(MapUserDto(user));
        }

        // Helpers
        private int? GetUserId()
        {
            var claim = User.FindFirst("userId");
            return claim != null ? int.Parse(claim.Value) : null;
        }

        private static UserDto MapUserDto(User u) =>
            new(u.Id, u.Phone, u.Name, u.Email, u.Avatar, u.Role);

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private static bool VerifyPassword(string password, string? hash)
        {
            if (hash == null) return false;
            return HashPassword(password) == hash;
        }
    }
}
