using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using PokerTrack.Contracts;

namespace PokerTrack.Web.Pages;

[Authorize]
public class EditSessionModel(SessionRepository repo) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        // Id is carried through the form as a hidden field so OnPostAsync
        // knows which session row to update
        public int Id { get; set; }

        [Required]
        public DateTime SessionDate { get; set; }

        [Required, MaxLength(100)]
        public string VenueName { get; set; } = string.Empty;

        [Required]
        public string GameType { get; set; } = "Cash";

        [MaxLength(20)]
        public string StakesDescription { get; set; } = string.Empty;

        [Required, Range(0, 100000)]
        public decimal BuyInAmount { get; set; }

        [Required, Range(0, 100000)]
        public decimal CashOutAmount { get; set; }

        [Required, Range(1, 2880)]
        public int DurationMinutes { get; set; }

        public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = GetUserId();
        var session = await repo.GetByIdAsync(id, userId);

        if (session is null) return NotFound();

        // Map the domain model to the input model to pre-populate the form
        Input = new InputModel
        {
            Id = session.Id,
            SessionDate = session.SessionDate,
            VenueName = session.VenueName,
            GameType = session.GameType,
            StakesDescription = session.StakesDescription,
            BuyInAmount = session.BuyInAmount,
            CashOutAmount = session.CashOutAmount,
            DurationMinutes = session.DurationMinutes,
            Notes = session.Notes
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var userId = GetUserId();

        var session = new PokerSession
        {
            Id = Input.Id,
            UserId = userId,
            SessionDate = Input.SessionDate,
            VenueName = Input.VenueName,
            GameType = Input.GameType,
            StakesDescription = Input.StakesDescription,
            BuyInAmount = Input.BuyInAmount,
            CashOutAmount = Input.CashOutAmount,
            DurationMinutes = Input.DurationMinutes,
            Notes = Input.Notes
        };

        await repo.UpdateAsync(session);
        return RedirectToPage("/Dashboard");
    }

    private string GetUserId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("No user ID claim found.");
}