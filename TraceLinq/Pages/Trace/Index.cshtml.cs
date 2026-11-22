using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TraceLinq.Data;
using TraceLinq.Data.Models;

namespace TraceLinq.Pages.Trace;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string? BatchId { get; set; }

    public Batch? Batch { get; set; }
    public bool Searched { get; set; }

    public async Task OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(BatchId))
            return;

        Searched = true;
        var id = BatchId.Trim();

        Batch = await _db.Batches
            .Include(b => b.Producer)
            .Include(b => b.QualityCheck).ThenInclude(q => q!.Inspector)
            .Include(b => b.WarehouseStorage)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BatchId == id);
    }
}
