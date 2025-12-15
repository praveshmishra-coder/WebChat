using MongoDB.Driver;
using SignalRChatApp.Models;
using SignalRChatApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// =================== MongoDB ===================
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration
        .GetSection("MongoDB")
        .Get<MongoDbSettings>();

    return new MongoClient(settings!.ConnectionString);
});

// =================== Services ===================
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =================== CORS ===================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true) // dev mode
    );
});

var app = builder.Build();

// =================== Middleware ===================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();
