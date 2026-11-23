using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TraceLinq.Data;
using TraceLinq.Data.Models;
using TraceLinq.Helpers;

namespace TraceLinq.Pages.Producer;

public class DashboardModel : PageModel
{
    private readonly AppDbContext _db;

    public DashboardModel(AppDbContext db)
    {
        _db = db;
    }

    // "All" or a BatchStatus name, e.g. "Registered", "InStorage".
    [BindProperty(SupportsGet = true)]
    public string Filter { get; set; } = "All";

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public Dictionary<BatchStatus, int> Counts { get; set; } = new();
    public int TotalCount { get; set; }
    public List<Batch> Batches { get; set; } = new();

    public int CountFor(BatchStatus status) => Counts.TryGetValue(status, out var n) ? n : 0;

    public async Task OnGetAsync()
    {
        var userId = User.UserId();

        // Only ever look at the logged-in producer's own batches.
        var mine = _db.Batches.Where(b => b.ProducerId == userId);

        Counts = await mine
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
        TotalCount = Counts.Values.Sum();

        var query = mine;

        // IsDefined stops odd values like "7" from being treated as a status.
        if (Enum.TryParse<BatchStatus>(Filter, ignoreCase: true, out var status) && Enum.IsDefined(status))
            query = query.Where(b => b.Status == status);
        else
            Filter = "All";

        Search = Search?.Trim();
        if (!string.IsNullOrEmpty(Search))
        {
            query = query.Where(b => b.BatchId.Contains(Search)
                                  || b.ProductType.Contains(Search)
                                  || b.OriginLocation.Contains(Search));
        }

        Batches = await query
            .OrderByDescending(b => b.CreatedAt)
            .ThenByDescending(b => b.BatchId)
            .AsNoTracking()
            .ToListAsync();
    }
}
