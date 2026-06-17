using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PokerTrack.Contracts;
using System.Security.Claims;

namespace PokerTrack.Web.Pages;

[Authorize]
public class SessionsModel(SessionRepository repo) : PageModel
{
    public IEnumerable<PokerSession> Sessions { get; private set; } = [];
    public IEnumerable<string> Venues { get; private set; } = [];
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }

    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string SortColumn { get; set; } = "SessionDate";
    [BindProperty(SupportsGet = true)] public string SortDirection { get; set; } = "desc";
    [BindProperty(SupportsGet = true)] public string? GameTypeFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? VenueFilter { get; set; }

    private const int PageSize = 10;

    public async Task OnGetAsync()
    {
        var userId = GetUserId();

        Venues = await repo.GetVenuesAsync(userId);

        var (sessions, totalCount) = await repo.GetPagedAsync(
            userId, CurrentPage, PageSize,
            SortColumn, SortDirection,
            GameTypeFilter, VenueFilter);

        Sessions = sessions;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
    }

    private string GetUserId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("No user ID claim found.");
}