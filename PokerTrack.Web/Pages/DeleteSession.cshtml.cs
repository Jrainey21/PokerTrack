using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace PokerTrack.Web.Pages;
//Authorize ensures unauthenticated users can't hit this URL directly.
//and delete session that don't belong to them.
[Authorize]
public class DeleteSessionModel(SessionRepository repo) : PageModel
{
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = GetUserId();
        //userID is passed to DeleteAsync so a user can only delete thier own sessions.
        await repo.DeleteAsync(id, userId);
        return RedirectToPage("/Dashboard");
    }

    private string GetUserId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("No user ID claim found.");
}