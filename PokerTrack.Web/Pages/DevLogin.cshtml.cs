using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

public class DevLoginModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "dev-user-001"),
            new("http://schemas.microsoft.com/identity/claims/objectidentifier", "dev-user-001"),
            new(ClaimTypes.Name, "Dev User")
        };

        var identity = new ClaimsIdentity(claims, "DevAuth");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("DevAuth", principal);
        return RedirectToPage("/Dashboard");
    }
}