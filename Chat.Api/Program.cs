using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog (reads config from appsettings.json)
builder.Host.UseSerilog((ctx, services, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Logs HTTP requests (method/path/status + timing)
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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

app.Run();