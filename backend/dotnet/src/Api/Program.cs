var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "Financial Inclusion Platform API",
    status = "running",
    stage = "foundation"
}));
app.MapHealthChecks("/health");
app.Run();

public partial class Program;
