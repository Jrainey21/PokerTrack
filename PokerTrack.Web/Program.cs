using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using PokerTrack.Web;
using System.Data;
using Serilog;
using Serilog.Formatting.Compact;

// Bootstrap logger captures startup errors before DI container is built
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Replace default logger with Serilog
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning);

    if (context.HostingEnvironment.IsDevelopment())
    {
        configuration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}");
    }
    else
    {
        configuration.WriteTo.Console(new CompactJsonFormatter());
    }

    // Always send to Seq regardless of environment
    configuration.WriteTo.Seq("http://localhost:5341");
});

// Cookie auth for local dev — swap for Entra ID SSO when tenant is available
builder.Services.AddAuthentication("DevAuth")
    .AddCookie("DevAuth");
builder.Services.AddAuthorization();

// Razor Pages handles page routing, SignalR handles real-time dashboard updates
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddApplicationInsightsTelemetry();
// Transient IDbConnection so each request gets its own SqlConnection via Dapper
builder.Services.AddTransient<IDbConnection>(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("Sql")));
builder.Services.AddTransient<SessionRepository>();

// Redirect unauthenticated users to dev login instead of default /Account/Login
builder.Services.ConfigureApplicationCookie(o => o.LoginPath = "/DevLogin");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Root URL redirects to DevLogin which auto-signs in and lands on Dashboard
app.MapGet("/", () => Results.Redirect("/DevLogin"));
app.MapRazorPages();

// Worker calls this after updating analytics — triggers SignalR push to all browsers
app.MapPost("/internal/analytics-updated", async (
    IHubContext<AnalyticsHub> hub,
    HttpRequest request) =>
{
    var userId = await request.ReadFromJsonAsync<string>();
    await hub.Clients.All.SendAsync("AnalyticsUpdated", userId);
    return Results.Ok();
});

app.MapHub<AnalyticsHub>("/analyticsHub");
app.MapControllers();

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Web app terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}