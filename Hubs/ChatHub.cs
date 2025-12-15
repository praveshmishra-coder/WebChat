using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SignalRChatApp.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            // Sab connected users ko message bhejo
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
