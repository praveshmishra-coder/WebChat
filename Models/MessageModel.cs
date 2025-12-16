using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SignalRChatApp.Models
{
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
