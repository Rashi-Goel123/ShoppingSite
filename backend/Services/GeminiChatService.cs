using System.Text;
using System.Text.Json;

namespace EcommerceApi.Services
{
    public interface IGeminiChatService
    {
        Task<string> GetResponseAsync(string userMessage, string? context = null);
    }

    public class GeminiChatService : IGeminiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly ILogger<GeminiChatService> _logger;

        private const string SystemPrompt = @"You are a helpful customer support assistant for 'Fashion Store', an online fashion e-commerce platform. 
You help customers with:
- Product recommendations (we sell Men's Wear, Women's Wear, Footwear, Accessories, Kids' Wear, Beauty & Grooming)
- Order status inquiries
- Return and refund policies (7-day return policy, full refund within 5 business days)
- Size guide help (we follow standard Indian sizing)
- Shipping info (free shipping above ₹499, delivery in 3-7 business days)
- Coupon and discount inquiries
- General fashion advice

Be friendly, concise, and helpful. Use emojis occasionally. Keep responses under 150 words.
If asked about something outside fashion/shopping, politely redirect to fashion topics.";

        public GeminiChatService(IConfiguration config, IHttpClientFactory httpClientFactory, ILogger<GeminiChatService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiKey = config["Gemini:ApiKey"]!;
            _model = config["Gemini:Model"] ?? "gemini-2.0-flash";
            _logger = logger;
        }

        public async Task<string> GetResponseAsync(string userMessage, string? context = null)
        {
            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

                var fullPrompt = SystemPrompt;
                if (!string.IsNullOrEmpty(context))
                    fullPrompt += $"\n\nAdditional context: {context}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = $"{fullPrompt}\n\nCustomer message: {userMessage}" } }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 300
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API error: {StatusCode} - {Body}", response.StatusCode, responseBody);
                    return "I'm sorry, I'm having trouble connecting right now. Please try again in a moment! 😊";
                }

                using var doc = JsonDocument.Parse(responseBody);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "I couldn't generate a response. Could you rephrase your question?";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini chat error");
                return "Oops! Something went wrong on my end. Please try again! 🙏";
            }
        }
    }
}
