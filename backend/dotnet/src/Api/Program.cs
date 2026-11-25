using Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=financial_platform;Username=platform_user;Password=change-me-local-only";

builder.Services.AddDbContext<InclusivDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<InclusivDbContext>("database");

var app = builder.Build();

// Automatically apply database migrations on startup when running in container/development
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("ApplyMigrationsOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<InclusivDbContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not apply database migrations on startup. Verify database connectivity.");
    }
}

app.MapGet("/", () => Results.Ok(new
{
    service = "Financial Inclusion Platform API",
    status = "running",
    stage = "data-architecture"
}));

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
