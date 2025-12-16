using SignalRChatApp.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Linq;
//using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;

namespace SignalRChatApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly UserService _userService;
        private readonly ChatService _chatService;

        // username -> connectionId
        private static ConcurrentDictionary<string, string> Users =
            new ConcurrentDictionary<string, string>();

        public ChatHub(UserService userService, ChatService chatService)
        {
            _userService = userService;
            _chatService = chatService;
        }

        public async Task RegisterUser(string username)
        {
            // Save user in DB
            await _userService.GetOrCreateUser(username);

            // Add to in-memory dictionary for active connections
            Users[username] = Context.ConnectionId;

            // Send updated active users list
            var allUsers = await _userService.GetAllUsers();
            await Clients.All.SendAsync("UpdateUserList", allUsers.Select(u => u.Username).ToList());
        }

        public async Task SendPrivateMessage(string toUser, string message)
        {
            var fromUser = Context.GetHttpContext()?.Request.Query["username"];

            // Send to client if online
            if (Users.TryGetValue(toUser, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", fromUser, message);
            }

            // Save message in DB
            await _chatService.SaveMessage(new ChatMessage
            {
                FromUser = fromUser,
                ToUser = toUser,
                Message = message
            });
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            var user = Users.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(user.Key))
            {
                Users.TryRemove(user.Key, out _);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task<List<ChatMessage>> GetChatHistory(string withUser)
        {
            var fromUser = Context.GetHttpContext()?.Request.Query["username"];

            if (string.IsNullOrEmpty(fromUser))
                return new List<ChatMessage>();

            return await _chatService.GetMessages(fromUser!, withUser);
        }
    }

}
