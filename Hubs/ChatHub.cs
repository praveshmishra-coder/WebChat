using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Linq;
//using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;

namespace SignalRChatApp.Hubs
{
    public class ChatHub : Hub
    {
        // username -> connectionId
        private static ConcurrentDictionary<string, string> Users =
            new ConcurrentDictionary<string, string>();

        // 1️⃣ Register user
        public async Task RegisterUser(string username)
        {
            Users[username] = Context.ConnectionId;

            // sab clients ko updated user list bhejo
            await Clients.All.SendAsync("UpdateUserList", Users.Keys.ToList());
        }

        // 2️⃣ One-to-one private message
        public async Task SendPrivateMessage(string toUser, string message)
        {
            if (Users.TryGetValue(toUser, out var connectionId))
            {
                await Clients.Client(connectionId)
                    .SendAsync("ReceiveMessage", Context.GetHttpContext()?.Request.Query["username"], message);
            }
        }

        // 3️⃣ Disconnect handle
        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            var user = Users.FirstOrDefault(x => x.Value == Context.ConnectionId);

            if (!string.IsNullOrEmpty(user.Key))
            {
                Users.TryRemove(user.Key, out _);
                await Clients.All.SendAsync("UpdateUserList", Users.Keys.ToList());
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
