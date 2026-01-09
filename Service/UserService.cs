using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SignalRChatApp.Models;
using System;
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

    // REGISTER
    public async Task<ChatUser> Register(string username, string email, string password)
    {
        var existingUser = await _users
            .Find(u => u.Email == email)
            .FirstOrDefaultAsync();

        if (existingUser != null)
            throw new Exception("Email already registered");

        var user = new ChatUser
        {
            Username = username,
            Email = email,
            PasswordHash = PasswordHasher.HashPassword(password)
        };

        await _users.InsertOneAsync(user);
        return user;
    }

    // LOGIN
    public async Task<ChatUser> Login(string email, string password)
    {
        var user = await _users
            .Find(u => u.Email == email)
            .FirstOrDefaultAsync();

        if (user == null)
            throw new Exception("Invalid email or password");

        if (!PasswordHasher.Verify(password, user.PasswordHash))
            throw new Exception("Invalid email or password");

        return user;
    }

    public async Task<List<ChatUser>> GetAllUsers()
    {
        return await _users.Find(_ => true).ToListAsync();
    }
}
