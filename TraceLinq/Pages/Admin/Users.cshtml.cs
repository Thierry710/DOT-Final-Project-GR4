using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TraceLinq.Data;
using TraceLinq.Data.Models;
using TraceLinq.Data.Security;
using TraceLinq.Helpers;
using AppUser = TraceLinq.Data.Models.User;

namespace TraceLinq.Pages.Admin;

// Admin only (see Program.cs): user CRUD
public class UsersModel : PageModel
{
    private readonly AppDbContext _db;
    public UsersModel(AppDbContext db) { _db = db; }

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty] public UserInput Input { get; set; } = new();
    public List<AppUser> Users { get; set; } = new();

    [TempData] public string? Message { get; set; }
    [TempData] public string? Error { get; set; }

    public class UserInput
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required"), StringLength(50)]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Full name is required"), StringLength(100)]
        public string FullName { get; set; } = "";

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public UserRole Role { get; set; } = UserRole.Producer;

        public bool IsActive { get; set; } = true;
    }

    public async Task OnGetAsync(int? editId)
    {
        await LoadUsers();
        if (editId.HasValue)
        {
            var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == editId.Value);
            if (u != null)
                Input = new UserInput
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = u.FullName,
                    Role = u.Role,
                    IsActive = u.IsActive
                };
        }
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        Input.Username = (Input.Username ?? "").Trim();
        Input.FullName = (Input.FullName ?? "").Trim();
        bool isNew = Input.UserId == 0;
        string pw = Input.Password ?? "";

        if (isNew && pw.Length < 6)
            ModelState.AddModelError("Input.Password", "Password must be at least 6 characters.");
        if (!isNew && pw.Length > 0 && pw.Length < 6)
            ModelState.AddModelError("Input.Password", "New password must be at least 6 characters.");

        AppUser? existing = null;
        if (ModelState.IsValid)
        {
            bool taken = await _db.Users.AnyAsync(u => u.Username == Input.Username && u.UserId != Input.UserId);
            if (taken) ModelState.AddModelError("Input.Username", "That username already exists.");

            if (!isNew)
            {
                existing = await _db.Users.FirstOrDefaultAsync(u => u.UserId == Input.UserId);
                if (existing == null)
                {
                    Error = "That user no longer exists.";
                    return RedirectToPage(new { Search });
                }
            }
        }

        if (ModelState.IsValid && existing != null)
        {
            bool losesAdmin = Input.Role != UserRole.Admin || !Input.IsActive;
            if (existing.UserId == User.UserId() && losesAdmin)
                ModelState.AddModelError(string.Empty, "You cannot remove your own Admin role or deactivate yourself.");
            else if (existing.Role == UserRole.Admin && existing.IsActive && losesAdmin
                     && !await AnotherActiveAdminExists(existing.UserId))
                ModelState.AddModelError(string.Empty, "At least one active Admin must remain.");
        }

        if (!ModelState.IsValid)
        {
            await LoadUsers();
            return Page();
        }

        if (isNew)
        {
            _db.Users.Add(new AppUser
            {
                Username = Input.Username,
                FullName = Input.FullName,
                Role = Input.Role,
                IsActive = Input.IsActive,
                PasswordHash = PasswordHasher.Hash(pw)
            });
            _db.AuditLogs.Add(new AuditLog { UserId = User.UserId(), Action = "Created user " + Input.Username });
        }
        else
        {
            existing!.Username = Input.Username;
            existing.FullName = Input.FullName;
            existing.Role = Input.Role;
            existing.IsActive = Input.IsActive;
            if (pw.Length > 0) existing.PasswordHash = PasswordHasher.Hash(pw);
            _db.AuditLogs.Add(new AuditLog { UserId = User.UserId(), Action = "Updated user " + Input.Username });
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // e.g. two admins created the same username at the same moment (unique index)
            ModelState.AddModelError(string.Empty, "The user could not be saved. The username may already exist.");
            await LoadUsers();
            return Page();
        }

        Message = isNew ? "User added." : "User updated.";
        return RedirectToPage(new { Search });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (id == User.UserId())
        {
            Error = "You cannot delete yourself.";
            return RedirectToPage(new { Search });
        }

        var u = await _db.Users.FirstOrDefaultAsync(x => x.UserId == id);
        if (u == null)
        {
            Error = "That user no longer exists.";
            return RedirectToPage(new { Search });
        }

        if (u.Role == UserRole.Admin && u.IsActive && !await AnotherActiveAdminExists(u.UserId))
        {
            Error = "At least one active Admin must remain.";
            return RedirectToPage(new { Search });
        }

        string name = u.Username;
        try
        {
            _db.Users.Remove(u);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Foreign keys (batches, checks, storage, audit log) block deleting users with history.
            Error = "This user already has batches or records, so it cannot be deleted. Edit the user and untick Active instead.";
            return RedirectToPage(new { Search });
        }

        _db.AuditLogs.Add(new AuditLog { UserId = User.UserId(), Action = "Deleted user " + name });
        await _db.SaveChangesAsync();
        Message = "User deleted.";
        return RedirectToPage(new { Search });
    }

    private Task<bool> AnotherActiveAdminExists(int exceptUserId) =>
        _db.Users.AnyAsync(u => u.UserId != exceptUserId && u.Role == UserRole.Admin && u.IsActive);

    private async Task LoadUsers()
    {
        var s = (Search ?? "").Trim();
        var q = _db.Users.AsNoTracking().AsQueryable();
        if (s.Length > 0)
            q = q.Where(u => u.Username.Contains(s) || u.FullName.Contains(s));
        Users = await q.OrderBy(u => u.UserId).ToListAsync();
    }
}
