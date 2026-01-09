using SignalRChatApp.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SignalRChatApp.Hubs
{
    [Authorize]
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

        // =================== HELPERS ===================
        private string GetUsername()
        {
            return Context.User?
                .Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == JwtRegisteredClaimNames.UniqueName)
                ?.Value!;
        }

        // =================== REGISTER USER ===================
        public async Task RegisterUser()
        {
            var username = GetUsername();
            if (string.IsNullOrEmpty(username))
                return;

            Users[username] = Context.ConnectionId;

            var allUsers = await _userService.GetAllUsers();
            await Clients.All.SendAsync(
                "UpdateUserList",
                allUsers.Select(u => u.Username).ToList()
            );
        }

        // =================== SEND PRIVATE MESSAGE ===================
        public async Task SendPrivateMessage(string toUser, string message)
        {
            var fromUser = GetUsername();
            if (string.IsNullOrEmpty(fromUser))
                return;

            if (Users.TryGetValue(toUser, out var connectionId))
            {
                await Clients.Client(connectionId)
                    .SendAsync("ReceiveMessage", fromUser, message);
            }

            await _chatService.SaveMessage(new ChatMessage
            {
                FromUser = fromUser,
                ToUser = toUser,
                Message = message
            });
        }

        // =================== CHAT HISTORY ===================
        public async Task<List<ChatMessage>> GetChatHistory(string withUser)
        {
            var fromUser = GetUsername();
            if (string.IsNullOrEmpty(fromUser))
                return new List<ChatMessage>();

            return await _chatService.GetMessages(fromUser, withUser);
        }

        // =================== DISCONNECT ===================
        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            var user = Users.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(user.Key))
            {
                Users.TryRemove(user.Key, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
