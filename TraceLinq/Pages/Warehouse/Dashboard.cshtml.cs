using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TraceLinq.Data;
using TraceLinq.Data.Models;
using TraceLinq.Helpers;

namespace TraceLinq.Pages.Warehouse;

public class DashboardModel : PageModel
{
    private readonly AppDbContext _db;

    public DashboardModel(AppDbContext db)
    {
        _db = db;
    }

    public List<Batch> ReadyToReceive { get; set; } = new();
    public List<WarehouseStorage> InStorage { get; set; } = new();
    public List<Location> Locations { get; set; } = new();

    [TempData] public string? Message { get; set; }
    [TempData] public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        ReadyToReceive = await _db.Batches
            .Where(b => b.Status == BatchStatus.Passed)
            .OrderBy(b => b.ProductionDate)
            .AsNoTracking()
            .ToListAsync();

        InStorage = await _db.WarehouseStorages
            .Include(ws => ws.Batch)
            .Where(ws => ws.Status == StorageStatus.InStorage)
            .OrderBy(ws => ws.ArrivalTime)
            .AsNoTracking()
            .ToListAsync();

        Locations = await _db.Locations
            .OrderBy(l => l.LocationName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostReceiveAsync(string? batchId, string? locationName)
    {
        batchId = batchId?.Trim();
        locationName = locationName?.Trim();

        if (string.IsNullOrEmpty(batchId))
        {
            Error = "No batch selected.";
            return RedirectToPage();
        }

        if (string.IsNullOrEmpty(locationName) || !await _db.Locations.AnyAsync(l => l.LocationName == locationName))
        {
            Error = "Please choose a valid storage location.";
            return RedirectToPage();
        }

        var batch = await _db.Batches.FirstOrDefaultAsync(b => b.BatchId == batchId);
        if (batch is null)
        {
            Error = "That batch no longer exists.";
            return RedirectToPage();
        }

        if (!BatchWorkflow.CanStore(batch.Status) || !BatchWorkflow.CanMove(batch.Status, BatchStatus.InStorage))
        {
            Error = "Batch " + batchId + " cannot be received because its status is " + batch.Status + ".";
            return RedirectToPage();
        }

        var userId = User.UserId();

        _db.WarehouseStorages.Add(new WarehouseStorage
        {
            BatchId = batchId,
            OperatorId = userId,
            StorageLocation = locationName,
            Status = StorageStatus.InStorage
        });

        batch.Status = BatchStatus.InStorage;

        _db.AuditLogs.Add(new AuditLog
        {
            BatchId = batchId,
            UserId = userId,
            Action = "Received batch " + batchId + " into " + locationName
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Happens if the batch already has a storage record (unique index on BatchId).
            Error = "Batch " + batchId + " has already been received.";
            return RedirectToPage();
        }

        Message = "Batch " + batchId + " received into " + locationName + ".";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDispatchAsync(string? batchId)
    {
        batchId = batchId?.Trim();

        var storage = await _db.WarehouseStorages
            .Include(w => w.Batch)
            .FirstOrDefaultAsync(w => w.BatchId == batchId);

        if (storage is null)
        {
            Error = "That batch is not in storage.";
            return RedirectToPage();
        }

        if (storage.Status != StorageStatus.InStorage || storage.Batch.Status != BatchStatus.InStorage)
        {
            Error = "Batch " + batchId + " cannot be dispatched because it is not in storage.";
            return RedirectToPage();
        }

        var userId = User.UserId();

        storage.Status = StorageStatus.Dispatched;
        storage.DispatchedAt = DateTime.Now;
        storage.Batch.Status = BatchStatus.Dispatched;

        _db.AuditLogs.Add(new AuditLog
        {
            BatchId = storage.BatchId,
            UserId = userId,
            Action = "Dispatched batch " + storage.BatchId
        });

        await _db.SaveChangesAsync();

        Message = "Batch " + storage.BatchId + " dispatched.";
        return RedirectToPage();
    }
}
