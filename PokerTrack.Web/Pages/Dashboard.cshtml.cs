using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using PokerTrack.Contracts;

namespace PokerTrack.Web.Pages;

[Authorize]
public class DashboardModel(SessionRepository repo) : PageModel
{
    public SessionAnalytics? Analytics { get; private set; }
    public IEnumerable<PokerSession> RecentSessions { get; private set; } = [];
    public string ChartLabelsJson { get; private set; } = "[]";
    public string ChartDataJson { get; private set; } = "[]";
    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new InvalidOperationException("No user ID claim found.");

        // Analytics are pre-computed by the Worker — this is just a simple read
        Analytics = await repo.GetAnalyticsAsync(userId);
        var all = await repo.GetByUserAsync(userId);
        RecentSessions = all.Take(10);

        // Build cumulative profit data for the chart, oldest to newest
        var chronological = all.OrderBy(s => s.SessionDate).ToList();
        decimal runningTotal = 0;
        var labels = new List<string>();
        var data = new List<decimal>();

        foreach (var s in chronological)
        {
            runningTotal += s.Profit;
            labels.Add(s.SessionDate.ToString("MM/dd"));
            data.Add(runningTotal);
        }

        ChartLabelsJson = System.Text.Json.JsonSerializer.Serialize(labels);
        ChartDataJson = System.Text.Json.JsonSerializer.Serialize(data);
    }

}
