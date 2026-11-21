using System.Security.Claims;

namespace TraceLinq.Helpers;

// In a PageModel or view: User.UserId()  User.FullName()  User.RoleName()
public static class UserExtensions
{
    public static int UserId(this ClaimsPrincipal u) =>
        int.TryParse(u.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public static string FullName(this ClaimsPrincipal u) =>
        u.FindFirstValue("FullName") ?? u.Identity?.Name ?? "";

    public static string RoleName(this ClaimsPrincipal u) =>
        u.FindFirstValue(ClaimTypes.Role) ?? "";
}

public static class RoleHome
{
    // Where each role lands after login
    public static string For(string? role) => role switch
    {
        "Admin"     => "/Admin/Dashboard",
        "Producer"  => "/Producer/Dashboard",
        "Inspector" => "/Inspector/Dashboard",
        "Warehouse" => "/Warehouse/Dashboard",
        _           => "/Account/Login"
    };
}
