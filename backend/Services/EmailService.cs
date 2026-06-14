using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EcommerceApi.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(string toEmail, string userName, string orderNumber, decimal totalAmount, string itemsSummary);
        Task SendShippingUpdateAsync(string toEmail, string userName, string orderNumber, string status, string? trackingInfo);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
        Task SendOtpEmailAsync(string toEmail, string otpCode);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendOrderConfirmationAsync(string toEmail, string userName, string orderNumber, decimal totalAmount, string itemsSummary)
        {
            var subject = $"Order Confirmed - {orderNumber} | Fashion Store";
            var body = $@"
<!DOCTYPE html>
<html>
<head><style>
  body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f5f5f5; margin: 0; padding: 20px; }}
  .container {{ max-width: 600px; margin: auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
  .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
  .header h1 {{ margin: 0; font-size: 24px; }}
  .content {{ padding: 30px; }}
  .order-info {{ background: #f8f9fa; border-radius: 8px; padding: 20px; margin: 20px 0; }}
  .total {{ font-size: 24px; color: #667eea; font-weight: bold; }}
  .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
</style></head>
<body>
  <div class='container'>
    <div class='header'>
      <h1>🛍️ Order Confirmed!</h1>
    </div>
    <div class='content'>
      <p>Hi <strong>{userName}</strong>,</p>
      <p>Thank you for shopping with Fashion Store! Your order has been confirmed.</p>
      <div class='order-info'>
        <p><strong>Order Number:</strong> {orderNumber}</p>
        {itemsSummary}
        <p class='total'>Total: ₹{totalAmount:N2}</p>
      </div>
      <p>We'll send you another email when your order ships. You can track your order anytime in your account.</p>
    </div>
    <div class='footer'>
      <p>Fashion Store © 2026 | All rights reserved</p>
    </div>
  </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendShippingUpdateAsync(string toEmail, string userName, string orderNumber, string status, string? trackingInfo)
        {
            var statusEmoji = status switch
            {
                "shipped" => "📦",
                "out_for_delivery" => "🚚",
                "delivered" => "✅",
                "cancelled" => "❌",
                _ => "📋"
            };

            var subject = $"{statusEmoji} Order {orderNumber} - {status.Replace("_", " ").ToUpper()}";
            var body = $@"
<!DOCTYPE html>
<html>
<head><style>
  body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f5f5f5; margin: 0; padding: 20px; }}
  .container {{ max-width: 600px; margin: auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
  .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
  .content {{ padding: 30px; }}
  .status-badge {{ display: inline-block; background: #e8f5e9; color: #2e7d32; padding: 8px 16px; border-radius: 20px; font-weight: bold; }}
  .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
</style></head>
<body>
  <div class='container'>
    <div class='header'>
      <h1>{statusEmoji} Order Update</h1>
    </div>
    <div class='content'>
      <p>Hi <strong>{userName}</strong>,</p>
      <p>Your order <strong>{orderNumber}</strong> status has been updated:</p>
      <p><span class='status-badge'>{status.Replace("_", " ").ToUpper()}</span></p>
      {(trackingInfo != null ? $"<p><strong>Details:</strong> {trackingInfo}</p>" : "")}
    </div>
    <div class='footer'>
      <p>Fashion Store © 2026</p>
    </div>
  </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Welcome to Fashion Store! 🎉";
            var body = $@"
<!DOCTYPE html>
<html>
<head><style>
  body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f5f5f5; margin: 0; padding: 20px; }}
  .container {{ max-width: 600px; margin: auto; background: white; border-radius: 12px; overflow: hidden; }}
  .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 40px; text-align: center; }}
  .content {{ padding: 30px; }}
  .coupon {{ background: linear-gradient(135deg, #ff6b6b, #feca57); color: white; padding: 20px; border-radius: 10px; text-align: center; margin: 20px 0; }}
  .coupon-code {{ font-size: 28px; font-weight: bold; letter-spacing: 3px; }}
  .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
</style></head>
<body>
  <div class='container'>
    <div class='header'>
      <h1>Welcome to Fashion Store!</h1>
      <p>Your style journey starts here ✨</p>
    </div>
    <div class='content'>
      <p>Hi <strong>{userName}</strong>,</p>
      <p>Welcome aboard! We're thrilled to have you. Here's a special welcome gift:</p>
      <div class='coupon'>
        <p>Use code</p>
        <p class='coupon-code'>WELCOME50</p>
        <p>for ₹50 off your first order!</p>
      </div>
      <p>Start exploring our latest fashion collections now.</p>
    </div>
    <div class='footer'>
      <p>Fashion Store © 2026</p>
    </div>
  </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var subject = $"Your OTP: {otpCode} - Fashion Store";
            var body = $@"
<!DOCTYPE html>
<html><body style='font-family: Arial; text-align: center; padding: 40px;'>
  <h2>Your Verification Code</h2>
  <div style='font-size: 36px; font-weight: bold; letter-spacing: 8px; color: #667eea; margin: 30px 0;'>{otpCode}</div>
  <p>This code expires in 5 minutes.</p>
  <p style='color: #666; font-size: 12px;'>If you didn't request this, please ignore this email.</p>
</body></html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _config["Smtp:SenderName"],
                    _config["Smtp:SenderEmail"]));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlBody };

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _config["Smtp:Host"],
                    int.Parse(_config["Smtp:Port"] ?? "587"),
                    SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(
                    _config["Smtp:Username"],
                    _config["Smtp:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("📧 Email sent to {To}: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send email to {To}: {Subject}", to, subject);
                // Don't throw — email failure shouldn't block the main flow
            }
        }
    }
}
