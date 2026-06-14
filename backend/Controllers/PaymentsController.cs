using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.DTOs;
using EcommerceApi.Services;

namespace EcommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IRazorpayService _razorpay;

        public PaymentsController(IRazorpayService razorpay) => _razorpay = razorpay;

        [HttpPost("create-order")]
        public IActionResult CreateOrder([FromBody] CreatePaymentOrderRequest request)
        {
            if (request.Amount <= 0) return BadRequest(new { message = "Invalid amount" });

            try
            {
                var receipt = $"rcpt_{DateTime.UtcNow:yyyyMMddHHmmss}_{new Random().Next(1000, 9999)}";
                var result = _razorpay.CreateOrder(request.Amount, receipt);

                return Ok(new PaymentOrderResponse(
                    result["id"].ToString()!,
                    request.Amount,
                    "INR",
                    result["keyId"].ToString()!
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("verify")]
        public IActionResult Verify([FromBody] VerifyPaymentRequest request)
        {
            var isValid = _razorpay.VerifySignature(request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature);
            if (!isValid) return BadRequest(new { message = "Payment verification failed", verified = false });

            return Ok(new { message = "Payment verified successfully", verified = true });
        }
    }
}
