using MongoDB.Driver;
using SignalRChatApp.Models;
using SignalRChatApp.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// =================== Builder ===================
var builder = WebApplication.CreateBuilder(args);

// =================== MongoDB ===================
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value;

    return new MongoClient(settings.ConnectionString);
});

// =================== JWT ===================
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };

    // Required for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
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

// =================== Build App ===================
var app = builder.Build();

// =================== Middleware ===================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();          // ✅ Required before authentication & SignalR
app.UseCors();

app.UseAuthentication();   // ✅ Must come BEFORE UseAuthorization
app.UseAuthorization();

// =================== MongoDB Info ===================
var mongoSettings = builder.Configuration
    .GetSection("MongoDB")
    .Get<MongoDbSettings>();

Console.WriteLine($"MongoDB DB: {mongoSettings?.DatabaseName}");

// =================== Endpoints ===================
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();
