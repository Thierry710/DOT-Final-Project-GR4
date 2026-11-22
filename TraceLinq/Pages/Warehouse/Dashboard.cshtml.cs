using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TraceLinq.Data;
using TraceLinq.Data.Models;

namespace TraceLinq.Pages.Warehouse
{
    public  class Dashboard : PageModel
    {
        private readonly AppDbContext _db;
        public Dashboard(AppDbContext db)
        {
            _db = db;
        }

        public List<Batch> ReadyToReceive { get; set; } = new();
        public List<WarehouseStorage> InStorage { get; set; } = new();
        public List<Location> Locations { get; set; } = new();

        public async Task OnGetAsync()
        {
            ReadyToReceive = await _db.Batches
                .Where(b => b.Status == BatchStatus.Passed)
                .AsNoTracking()
                .ToListAsync();

            InStorage = await _db.WarehouseStorages
                .Include(ws => ws.Batch)
                .Where(ws => ws.Status == StorageStatus.InStorage)
                .AsNoTracking()
                .ToListAsync();

            Locations = await _db.Locations
                .AsNoTracking()
                .ToListAsync();
            }
        }
    }
    

