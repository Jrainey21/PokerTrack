using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using PokerTrack.Contracts;

namespace PokerTrack.Web.Pages;

[Authorize]
public class LogSessionModel(SessionRepository repo) : PageModel
{
    //tells Razor Pages to auto map form fields to this object on POST
    [BindProperty]
    public InputModel Input { get; set; } = new();
    //Contains what the form needs and carries validation attributes.
    public class InputModel
    {
        [Required]
        public DateTime SessionDate { get; set; } = DateTime.Today;

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

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new InvalidOperationException("No user ID claim found.");

        //Map form input to domain model before passing to repository.
        var session = new PokerSession
        {
            UserId           = userId,
            SessionDate      = Input.SessionDate,
            VenueName        = Input.VenueName,
            GameType         = Input.GameType,
            StakesDescription = Input.StakesDescription,
            BuyInAmount      = Input.BuyInAmount,
            CashOutAmount    = Input.CashOutAmount,
            DurationMinutes  = Input.DurationMinutes,
            Notes            = Input.Notes
        };

        await repo.CreateAsync(session);
        return RedirectToPage("/Dashboard");
    }
}
