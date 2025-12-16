using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SignalRChatApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ChatService
{
    private readonly IMongoCollection<ChatMessage> _messages;

    public ChatService(IOptions<MongoDbSettings> settings, IMongoClient client)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _messages = database.GetCollection<ChatMessage>("Messages");
    }

    public async Task SaveMessage(ChatMessage message)
    {
        await _messages.InsertOneAsync(message);
    }

    public async Task<List<ChatMessage>> GetMessages(string user1, string user2)
    {
        return await _messages.Find(m =>
            (m.FromUser == user1 && m.ToUser == user2) ||
            (m.FromUser == user2 && m.ToUser == user1))
            .SortBy(m => m.Timestamp)
            .ToListAsync();
    }
}
