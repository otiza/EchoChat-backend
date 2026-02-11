using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using NSwag;
using Serilog;

using Chat.Application.Abstractions.Authentication;
using Chat.Application.Abstractions.Persistence;
using Chat.Application.Auth;
using Chat.Application.Common;
using Chat.Application.Users;
using Chat.Application.Conversations.Services;
using Chat.Application.Messages.Services;

using Chat.Infrastructure.Auth;
using Chat.Infrastructure.Authentication;
using Chat.Infrastructure.Common;
using Chat.Infrastructure.Conversations;
using Chat.Infrastructure.Messages;
using Chat.Infrastructure.Persistence.Chat;
using Chat.Infrastructure.Persistence.Mongo;
using Chat.Infrastructure.Persistence.Users;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Logging --------------------
builder.Host.UseSerilog((ctx, services, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .ReadFrom.Services(services));

// -------------------- Infrastructure --------------------
builder.Services.AddMongo(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IUserContext, HttpUserContext>();

// Repositories (Sprint 1 + Sprint 2)
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<IUserRepository, MongoUserRepository>();

builder.Services.AddScoped<Chat.Application.Conversations.Ports.IConversationRepository, ConversationMongoRepository>();
builder.Services.AddScoped<Chat.Application.Messages.Ports.IMessageRepository, MessageMongoRepository>();
builder.Services.AddSingleton<Chat.Api.Realtime.InMemoryPresenceTracker>();

// Indexers
builder.Services.AddScoped<ChatMongoIndexes>();

// -------------------- Auth --------------------
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30),

            // ✅ Important for SignalR user identification
            NameClaimType = "sub"
        };

        // ✅ Important for SignalR over WebSockets (token comes via query string)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// -------------------- Application services --------------------
builder.Services.AddScoped<RegisterService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<GetMeService>();
builder.Services.AddScoped<SearchUsersService>();
builder.Services.AddScoped<CreateConversationService>();
builder.Services.AddScoped<GetMyConversationsService>();
builder.Services.AddScoped<SendMessageService>();
builder.Services.AddScoped<GetMessagesService>();

//builder.Services.AddSignalR();
builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = true;
});

// -------------------- API --------------------
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();

// NSwag (OpenAPI)
builder.Services.AddOpenApiDocument(c =>
{
    c.DocumentName = "v1";
    c.Title = "Chat API";

    c.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });
});

// Middleware
builder.Services.AddTransient<Chat.Api.Middleware.ExceptionHandlingMiddleware>();

var app = builder.Build();

// -------------------- Startup tasks (indexes) --------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

    var chatIndexer = scope.ServiceProvider.GetRequiredService<ChatMongoIndexes>();
    await chatIndexer.EnsureCreatedAsync(CancellationToken.None);

    await UserIndexes.EnsureCreatedAsync(db);
}

// -------------------- Pipeline --------------------
app.UseMiddleware<Chat.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();    // /swagger/v1/swagger.json
    app.UseSwaggerUi();  // NSwag UI
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<Chat.Api.Hubs.ChatHub>("/hubs/chat");
app.MapHealthChecks("/health");

app.MapGet("/api/version", () =>
{
    var asm = typeof(Program).Assembly.GetName();
    return Results.Ok(new
    {
        name = asm.Name,
        version = asm.Version?.ToString() ?? "unknown"
    });
});

app.MapGet("/health/mongo", async (IMongoDatabase db) =>
{
    var cmd = new BsonDocument("ping", 1);
    await db.RunCommandAsync<BsonDocument>(cmd);
    return Results.Ok(new { mongo = "ok" });
});

app.Run();