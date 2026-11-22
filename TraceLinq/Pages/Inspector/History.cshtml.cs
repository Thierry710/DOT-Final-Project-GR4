using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TraceLinq.Data;
using TraceLinq.Data.Models;

namespace TraceLinq.Pages.Inspector;

public class HistoryModel : PageModel
{
    private readonly AppDbContext _db;

    public HistoryModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string Filter { get; set; } = "All";

    public List<QualityCheck> Checks { get; set; } = new();

    public async Task OnGetAsync()
    {
        var query = _db.QualityChecks
            .Include(c => c.Batch)
            .Include(c => c.Inspector)
            .AsQueryable();

        if (Filter == "Pass")
            query = query.Where(c => c.Result == QualityResult.Passed);
        else if (Filter == "Fail")
            query = query.Where(c => c.Result == QualityResult.Failed);

        Checks = await query.OrderByDescending(c => c.CheckedAt).ToListAsync();
    }
}
