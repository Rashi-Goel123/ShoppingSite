using System.Security.Cryptography;
using System.Text;
using Razorpay.Api;

namespace EcommerceApi.Services
{
    public interface IRazorpayService
    {
        Dictionary<string, object> CreateOrder(decimal amount, string receipt);
        bool VerifySignature(string orderId, string paymentId, string signature);
    }

    public class RazorpayService : IRazorpayService
    {
        private readonly RazorpayClient _client;
        private readonly string _keySecret;
        private readonly string _keyId;

        public RazorpayService(IConfiguration config)
        {
            _keyId = config["Razorpay:KeyId"]!;
            _keySecret = config["Razorpay:KeySecret"]!;
            _client = new RazorpayClient(_keyId, _keySecret);
        }

        public Dictionary<string, object> CreateOrder(decimal amount, string receipt)
        {
            var options = new Dictionary<string, object>
            {
                { "amount", (int)(amount * 100) }, // Amount in paise
                { "currency", "INR" },
                { "receipt", receipt }
            };

            try
            {
                var order = _client.Order.Create(options);
                return new Dictionary<string, object>
                {
                    { "id", order["id"].ToString()! },
                    { "amount", order["amount"] },
                    { "currency", order["currency"].ToString()! },
                    { "keyId", _keyId }
                };
            }
            catch (Razorpay.Api.Errors.BadRequestError ex)
            {
                throw new Exception($"Razorpay Authentication Failed. Please check if your KeyId and KeySecret are correct. Original error: {ex.Message}");
            }
        }

        public bool VerifySignature(string orderId, string paymentId, string signature)
        {
            var text = orderId + "|" + paymentId;
            var secret = Encoding.UTF8.GetBytes(_keySecret);
            var payload = Encoding.UTF8.GetBytes(text);

            using var hmac = new HMACSHA256(secret);
            var hash = hmac.ComputeHash(payload);
            var generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return generatedSignature == signature;
        }
    }
}
