using SignalRChatApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// SignalR add
builder.Services.AddSignalR();

var app = builder.Build();

app.UseDefaultFiles(); // index.html auto load
app.UseStaticFiles();

// SignalR endpoint
app.MapHub<ChatHub>("/chatHub");

app.Run();
