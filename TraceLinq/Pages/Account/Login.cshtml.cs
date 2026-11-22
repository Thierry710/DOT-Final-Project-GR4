using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TraceLinq.Data;
using TraceLinq.Data.Models;
using TraceLinq.Data.Security;
using TraceLinq.Helpers;

namespace TraceLinq.Pages.Account;

public class LoginModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ILogger<LoginModel> _logger;
    public LoginModel(AppDbContext db, ILogger<LoginModel> logger) { _db = db; _logger = logger; }

    [BindProperty, Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Password is required"), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? Error { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return LocalRedirect(RoleHome.For(User.RoleName()));
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        TraceLinq.Data.Models.User? user;
        try
        {
            var name = Username.Trim();
            user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == name && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error during login");
            Error = "The service is temporarily unavailable. Please try again later.";
            return Page();
        }

        if (user is null || !PasswordHasher.Verify(Password, user.PasswordHash))
        {
            Error = "Invalid username or password.";
            return Page();
        }

        var role = user.Role.ToString();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new("FullName", user.FullName),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        _db.AuditLogs.Add(new AuditLog { UserId = user.UserId, Action = "Login" });
        await _db.SaveChangesAsync();

        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            return LocalRedirect(ReturnUrl);
        return LocalRedirect(RoleHome.For(role));
    }
}
