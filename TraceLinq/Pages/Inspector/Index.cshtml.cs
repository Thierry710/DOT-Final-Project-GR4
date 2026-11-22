using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TraceLinq.Data;
using TraceLinq.Data.Models;

namespace TraceLinq.Pages.Inspector;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string Filter { get; set; } = "Pending";

    public int PendingCount { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public List<Batch> Batches { get; set; } = new();

    public async Task OnGetAsync()
    {
        PendingCount = await _db.Batches.CountAsync(b => b.Status == BatchStatus.Registered);
        PassedCount = await _db.Batches.CountAsync(b => b.Status == BatchStatus.Passed);
        FailedCount = await _db.Batches.CountAsync(b => b.Status == BatchStatus.Failed);

        var query = _db.Batches
            .Include(b => b.Producer)
            .Include(b => b.QualityCheck)
            .AsQueryable();

        if (Filter == "Pending")
            query = query.Where(b => b.Status == BatchStatus.Registered);
        else if (Filter == "Passed")
            query = query.Where(b => b.Status == BatchStatus.Passed);
        else if (Filter == "Failed")
            query = query.Where(b => b.Status == BatchStatus.Failed);

        Batches = await query.OrderBy(b => b.ProductionDate).ToListAsync();
    }
}
