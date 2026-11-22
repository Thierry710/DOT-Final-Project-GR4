using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TraceLinq.Data;
using TraceLinq.Data.Models;
using TraceLinq.Helpers;

namespace TraceLinq.Pages.Inspector;

public class CheckModel : PageModel
{
    private readonly AppDbContext _db;

    public CheckModel(AppDbContext db)
    {
        _db = db;
    }

    public Batch? Batch { get; set; }

    [BindProperty]
    public QualityResult? Result { get; set; }

    [BindProperty]
    public string? Remarks { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        Batch = await LoadBatch(id);
        if (Batch == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        Batch = await LoadBatch(id);
        if (Batch == null) return NotFound();

        Remarks = Remarks?.Trim();

        if (Result == null)
            ModelState.AddModelError("Result", "Please choose Pass or Fail.");

        if (Result == QualityResult.Failed && string.IsNullOrEmpty(Remarks))
            ModelState.AddModelError("Remarks", "Remarks are required when the batch fails.");

        if (Remarks != null && Remarks.Length > 300)
            ModelState.AddModelError("Remarks", "Remarks cannot be longer than 300 characters.");

        if (!ModelState.IsValid) return Page();

        // Passed -> Passed, Failed -> Failed
        var newStatus = Result == QualityResult.Passed ? BatchStatus.Passed : BatchStatus.Failed;

        if (!BatchWorkflow.CanMove(Batch.Status, newStatus))
        {
            ModelState.AddModelError("", "This batch cannot be inspected because its status is " + Batch.Status + ".");
            return Page();
        }

        var oldStatus = Batch.Status;
        var userId = User.UserId();

        var check = new QualityCheck
        {
            BatchId = Batch.BatchId,
            InspectorId = userId,
            Result = Result!.Value,
            Remarks = string.IsNullOrEmpty(Remarks) ? null : Remarks
        };
        _db.QualityChecks.Add(check);

        Batch.Status = newStatus;

        _db.AuditLogs.Add(new AuditLog
        {
            BatchId = Batch.BatchId,
            UserId = userId,
            Action = "Quality check " + Result + ": " + oldStatus + " -> " + newStatus
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Happens if the batch already has a quality check (unique index on BatchId).
            ModelState.AddModelError("", "This batch has already been inspected.");
            return Page();
        }

        TempData["Message"] = "Batch " + Batch.BatchId + " marked as " + newStatus + ".";
        return RedirectToPage("Index");
    }

    private async Task<Batch?> LoadBatch(string id)
    {
        return await _db.Batches
            .Include(b => b.Producer)
            .FirstOrDefaultAsync(b => b.BatchId == id);
    }
}
