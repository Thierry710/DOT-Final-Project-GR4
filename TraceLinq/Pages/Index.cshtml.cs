using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TraceLinq.Helpers;

namespace TraceLinq.Pages;

public class IndexModel : PageModel
{
    // Signed-in users go straight to their dashboard; visitors see the landing page.
    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return LocalRedirect(RoleHome.For(User.RoleName()));
        return Page();
    }
}
