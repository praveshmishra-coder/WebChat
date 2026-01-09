namespace SignalRChatApp.Models
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 60;
        public string Issuer { get; set; } = "SignalRChatApp";
        public string Audience { get; set; } = "SignalRChatAppUsers";
    }
}
