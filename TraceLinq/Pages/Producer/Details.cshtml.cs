using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TraceLinq.Data;
using TraceLinq.Data.Models;
using TraceLinq.Helpers;

namespace TraceLinq.Pages.Producer;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _db;

    public DetailsModel(AppDbContext db)
    {
        _db = db;
    }

    public Batch? Batch { get; set; }

    // Full URL of the public Trace page for this batch - this is what the QR code contains.
    public string TraceUrl { get; set; } = "";

    // GET /Producer/Details/BT-20261123-001
    public async Task<IActionResult> OnGetAsync(string id)
    {
        Batch = await LoadOwnBatch(id);
        if (Batch == null) return NotFound();

        TraceUrl = TraceUrlFor(Batch.BatchId);
        return Page();
    }

    // GET /Producer/Details/BT-...?handler=Qr  -> the QR image shown in <img> tags.
    public async Task<IActionResult> OnGetQrAsync(string id)
    {
        var batch = await LoadOwnBatch(id);
        if (batch == null) return NotFound();

        return File(QrCodeHelper.ToPng(TraceUrlFor(batch.BatchId)), "image/png");
    }

    // GET /Producer/Details/BT-...?handler=Download -> same image, saved as {BatchId}.png.
    public async Task<IActionResult> OnGetDownloadAsync(string id)
    {
        var batch = await LoadOwnBatch(id);
        if (batch == null) return NotFound();

        return File(QrCodeHelper.ToPng(TraceUrlFor(batch.BatchId)), "image/png", batch.BatchId + ".png");
    }

    // Returns the batch only if it belongs to the logged-in producer.
    // Someone else's batch gives null -> 404, so producers can't even tell that it exists.
    private async Task<Batch?> LoadOwnBatch(string id)
    {
        var userId = User.UserId();

        return await _db.Batches
            .Include(b => b.QualityCheck).ThenInclude(q => q!.Inspector)
            .Include(b => b.WarehouseStorage)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BatchId == id && b.ProducerId == userId);
    }

    // e.g. https://localhost:5137/Trace?batchId=BT-20261123-001
    private string TraceUrlFor(string batchId) =>
        Url.Page("/Trace/Index", pageHandler: null, values: new { batchId }, protocol: Request.Scheme)!;
}
