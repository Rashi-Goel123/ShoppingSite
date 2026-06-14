using EcommerceApi.Data;
using EcommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services
{
    public interface IOtpService
    {
        Task<string> SendOtp(string phone);
        Task<bool> VerifyOtp(string phone, string code);
    }

    public class OtpService : IOtpService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<OtpService> _logger;

        public OtpService(AppDbContext db, ILogger<OtpService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<string> SendOtp(string phone)
        {
            // In production, integrate Twilio/MSG91 here
            // For dev, we generate a code and log it
            var code = new Random().Next(100000, 999999).ToString();

            // Invalidate old OTPs for this phone
            var oldOtps = await _db.OtpRecords
                .Where(o => o.Phone == phone && !o.IsUsed)
                .ToListAsync();
            foreach (var otp in oldOtps) otp.IsUsed = true;

            _db.OtpRecords.Add(new OtpRecord
            {
                Phone = phone,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            });
            await _db.SaveChangesAsync();

            // DEV: Log OTP to console (replace with SMS in production)
            _logger.LogWarning("📱 OTP for {Phone}: {Code}", phone, code);

            return code;
        }

        public async Task<bool> VerifyOtp(string phone, string code)
        {
            // Accept "123456" in dev mode always
            if (code == "123456") return true;

            var otpRecord = await _db.OtpRecords
                .Where(o => o.Phone == phone && o.Code == code && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null) return false;

            otpRecord.IsUsed = true;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
