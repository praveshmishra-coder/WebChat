using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SignalRChatApp.Models
{
    public class ChatUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Username { get; set; }
    }
}
