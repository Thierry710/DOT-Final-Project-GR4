using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using TraceLinq.Data;
using TraceLinq.Data.Models;

namespace TraceLinq.Pages.Trace
{
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

            Batch = await _db.Batches
                .Include(b => b.Producer)
                .Include(b => b.QualityCheck)
                .Include(b => b.WarehouseStorage)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BatchId == BatchId.Trim());

            {
                
            }
        }
        public void OnGet()
        {
        }
    }
}
