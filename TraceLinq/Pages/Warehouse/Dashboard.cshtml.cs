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

        public async Task<IActionResult> OnPostReceiveAsync(string batchId, string locationName)
        {
            var batch = await _db.Batches.FirstOrDefaultAsync(b => b.BatchId == batchId);
            if (batch is null || batch.Status != BatchStatus.Passed)
                return RedirectToPage();

            _db.WarehouseStorages.Add(new WarehouseStorage
            {
                BatchId = batchId,
                OperatorId = User.UserId(),
                StorageLocation = locationName,
                Status = StorageStatus.InStorage
            });

            batch.Status = BatchStatus.InStorage;

            _db.AuditLogs.Add(new AuditLog { UserId = User.UserId(), Action = $"Received batch {batchId} into {locationName}" });

            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDispatchAsync(string batchId)
        {
            var storage = await _db.WarehouseStorages.FirstOrDefaultAsync(w => w.BatchId == batchId);
            if (storage is null || storage.Status != StorageStatus.InStorage)
                return RedirectToPage();

            storage.Status = StorageStatus.Dispatched;
            storage.DispatchedAt = DateTime.Now;

            var batch = await _db.Batches.FirstAsync(b => b.BatchId == batchId);
            batch.Status = BatchStatus.Dispatched;

            _db.AuditLogs.Add(new AuditLog { UserId = User.UserId(), Action = $"Dispatched batch {batchId}" });

            await _db.SaveChangesAsync();
            return RedirectToPage();
        }
        }


    }
    

