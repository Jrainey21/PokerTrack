using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using PokerTrack.Web;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("DevAuth")
    .AddCookie("DevAuth");

builder.Services.AddAuthorization();
//Razor pages handles page routinng and SignalR will provide real-time dashboard updates
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddTransient<IDbConnection>(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("Sql")));

builder.Services.AddTransient<SessionRepository>();
//Redirect unuathenticated users to devlogin instead of default.
builder.Services.ConfigureApplicationCookie(o => o.LoginPath = "/DevLogin");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
//Root URL redirects to DevLogin...lands on dashboard
app.MapGet("/", () => Results.Redirect("/DevLogin"));
app.MapRazorPages();

//Worker calls this after updating analytics-- triggers SingalR
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

app.Run();