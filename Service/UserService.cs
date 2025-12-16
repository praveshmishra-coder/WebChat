using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SignalRChatApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UserService
{
    private readonly IMongoCollection<ChatUser> _users;

    public UserService(IOptions<MongoDbSettings> settings, IMongoClient client)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _users = database.GetCollection<ChatUser>("Users");
    }

    public async Task<ChatUser> GetOrCreateUser(string username)
    {
        var user = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        if (user == null)
        {
            user = new ChatUser { Username = username };
            await _users.InsertOneAsync(user);
        }
        return user;
    }

    public async Task<List<ChatUser>> GetAllUsers()
    {
        return await _users.Find(_ => true).ToListAsync();
    }
}
