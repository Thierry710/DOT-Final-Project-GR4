using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TraceLinq.Data;
using TraceLinq.Data.Models;

namespace TraceLinq.Pages.Admin;

// Admin only (see Program.cs): system overview.
public class DashboardModel : PageModel
{
    private readonly AppDbContext _db;
    public DashboardModel(AppDbContext db) { _db = db; }

    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public Dictionary<UserRole, int> UsersByRole { get; set; } = new();
    public Dictionary<BatchStatus, int> BatchesByStatus { get; set; } = new();
    public int TotalBatches => BatchesByStatus.Values.Sum();
    public List<AuditLog> Recent { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalUsers = await _db.Users.CountAsync();
        ActiveUsers = await _db.Users.CountAsync(u => u.IsActive);

        var roles = await _db.Users.GroupBy(u => u.Role).Select(g => new { g.Key, Count = g.Count() }).ToListAsync();
        UsersByRole = roles.ToDictionary(x => x.Key, x => x.Count);

        var statuses = await _db.Batches.GroupBy(b => b.Status).Select(g => new { g.Key, Count = g.Count() }).ToListAsync();
        BatchesByStatus = statuses.ToDictionary(x => x.Key, x => x.Count);

        Recent = await _db.AuditLogs.AsNoTracking().Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.LogId)
            .Take(8).ToListAsync();
    }

    public int Count(BatchStatus s) => BatchesByStatus.TryGetValue(s, out var n) ? n : 0;
    public int Count(UserRole r) => UsersByRole.TryGetValue(r, out var n) ? n : 0;
}
