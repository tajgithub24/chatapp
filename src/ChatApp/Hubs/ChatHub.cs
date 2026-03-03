using Microsoft.AspNetCore.SignalR;
using ChatApp.Data;
using ChatApp.Models;

namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;
        private static readonly Dictionary<string, string> _userConnections = new();

        public ChatHub(ApplicationDbContext db) => _db = db;

        public override Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext().Session.GetInt32("UserId")?.ToString();
            if (userId != null) _userConnections[userId] = Context.ConnectionId;
            return base.OnConnectedAsync();
        }

        public async Task SendPrivateMessage(string receiverId, string message)
        {
            var senderId = Context.GetHttpContext().Session.GetInt32("UserId");
            var senderName = Context.GetHttpContext().Session.GetString("UserName");

            if (senderId == null) return;

            // SAVE TO DATABASE
            var chatMsg = new ChatMessage
            {
                SenderId = senderId.Value,
                ReceiverId = int.Parse(receiverId),
                Message = message
            };
            _db.ChatMessages.Add(chatMsg);
            await _db.SaveChangesAsync();

            if (_userConnections.TryGetValue(receiverId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceivePrivateMessage", senderId.ToString(), senderName, message, null);
            }
            await Clients.Caller.SendAsync("ReceivePrivateMessage", receiverId, senderName, message, null);
        }
    }
}