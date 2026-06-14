using Microsoft.AspNetCore.SignalR;
using EcommerceApi.Data;
using EcommerceApi.Models;
using EcommerceApi.Services;

namespace EcommerceApi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IGeminiChatService _gemini;
        private readonly AppDbContext _db;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IGeminiChatService gemini, AppDbContext db, ILogger<ChatHub> logger)
        {
            _gemini = gemini;
            _db = db;
            _logger = logger;
        }

        public async Task SendMessage(int userId, string message)
        {
            _logger.LogInformation("Chat message from user {UserId}: {Message}", userId, message);

            // Save user message
            _db.ChatMessages.Add(new ChatMessage
            {
                UserId = userId,
                Sender = "user",
                Message = message
            });
            await _db.SaveChangesAsync();

            // Get AI response
            var botReply = await _gemini.GetResponseAsync(message);

            // Save bot message
            _db.ChatMessages.Add(new ChatMessage
            {
                UserId = userId,
                Sender = "bot",
                Message = botReply
            });
            await _db.SaveChangesAsync();

            // Send reply to the calling client
            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                sender = "bot",
                message = botReply,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task LoadHistory(int userId)
        {
            var messages = _db.ChatMessages
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.Timestamp)
                .Take(50)
                .OrderBy(m => m.Timestamp)
                .Select(m => new { m.Sender, m.Message, m.Timestamp })
                .ToList();

            await Clients.Caller.SendAsync("ChatHistory", messages);
        }
    }

    public class OrderTrackingHub : Hub
    {
        public async Task JoinOrderGroup(int orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }

        public async Task LeaveOrderGroup(int orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }
    }

    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
    }
}
