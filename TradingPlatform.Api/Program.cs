using Microsoft.EntityFrameworkCore;
using TradingPlatform.Api.Middleware;
using TradingPlatform.Application.Extensions;
using TradingPlatform.Infrastructure.Extensions;
using TradingPlatform.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Run migrations when using a real database (e.g. Docker)
if (builder.Configuration["UseInMemoryDatabase"] != "true")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandling();
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

/// <summary>Exposed for integration testing with WebApplicationFactory.</summary>
public partial class Program { }
