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

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new InvalidOperationException("No user ID claim found.");

        // Analytics are pre-computed by the Worker — this is just a simple read
        Analytics = await repo.GetAnalyticsAsync(userId);
        var all = await repo.GetByUserAsync(userId);
        RecentSessions = all.Take(10);
    }
}
