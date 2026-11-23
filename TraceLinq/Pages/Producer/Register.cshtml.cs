using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TraceLinq.Data;
using TraceLinq.Data.Models;
using TraceLinq.Helpers;

namespace TraceLinq.Pages.Producer;

public class RegisterModel : PageModel
{
    // How many times we try again if another producer took the same batch ID at the same moment.
    private const int MaxIdAttempts = 5;

    private readonly AppDbContext _db;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(AppDbContext db, ILogger<RegisterModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    [BindProperty]
    public BatchInput Input { get; set; } = new();

    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    public class BatchInput
    {
        [Required(ErrorMessage = "Product type is required.")]
        [StringLength(100, ErrorMessage = "Product type cannot be longer than 100 characters.")]
        [Display(Name = "Product type")]
        public string? ProductType { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        public decimal? Quantity { get; set; }

        [Required(ErrorMessage = "Please choose a unit.")]
        public string? Unit { get; set; }

        [Required(ErrorMessage = "Origin location is required.")]
        [StringLength(150, ErrorMessage = "Origin location cannot be longer than 150 characters.")]
        [Display(Name = "Origin location")]
        public string? OriginLocation { get; set; }

        [Required(ErrorMessage = "Production date is required.")]
        [Display(Name = "Production date")]
        public DateOnly? ProductionDate { get; set; }
    }

    public void OnGet()
    {
        Input.ProductionDate = Today;
        Input.Unit = "kg";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Input.ProductType = Input.ProductType?.Trim();
        Input.OriginLocation = Input.OriginLocation?.Trim();

        // Rules that data annotations can't express (shared with the unit tests).
        // Skip the quantity rule if binding already failed (empty or not a number).
        if (ModelState.GetFieldValidationState("Input.Quantity") != ModelValidationState.Invalid)
        {
            var quantityError = BatchRules.CheckQuantity(Input.Quantity);
            if (quantityError != null) ModelState.AddModelError("Input.Quantity", quantityError);
        }

        if (Input.Unit != null && !BatchRules.IsValidUnit(Input.Unit))
            ModelState.AddModelError("Input.Unit", "Please choose a unit from the list.");

        if (Input.ProductionDate != null)
        {
            var dateError = BatchRules.CheckProductionDate(Input.ProductionDate, Today);
            if (dateError != null) ModelState.AddModelError("Input.ProductionDate", dateError);
        }

        if (!ModelState.IsValid) return Page();

        var userId = User.UserId();

        // Compute the next ID and save. If another producer saved the same ID a moment
        // earlier, SQL Server rejects ours (primary key) and we simply try the next number.
        for (var attempt = 1; attempt <= MaxIdAttempts; attempt++)
        {
            var batchId = await BatchIdGenerator.NextIdAsync(_db, Today);

            _db.Batches.Add(new Batch
            {
                BatchId = batchId,
                ProductType = Input.ProductType!,
                Quantity = Input.Quantity!.Value,
                Unit = Input.Unit!,
                OriginLocation = Input.OriginLocation!,
                ProductionDate = Input.ProductionDate!.Value,
                ProducerId = userId,
                Status = BatchStatus.Registered
            });

            _db.AuditLogs.Add(new AuditLog
            {
                BatchId = batchId,
                UserId = userId,
                Action = "Registered batch " + batchId
            });

            try
            {
                await _db.SaveChangesAsync();
                TempData["Message"] = "Batch " + batchId + " registered.";
                return RedirectToPage("Details", new { id = batchId });
            }
            catch (DbUpdateException ex) when (BatchIdGenerator.IsDuplicateKey(ex))
            {
                _logger.LogWarning("Batch ID {BatchId} was taken, retrying (attempt {Attempt})", batchId, attempt);
                // Forget the failed rows so the next attempt starts clean.
                _db.ChangeTracker.Clear();
            }
        }

        ModelState.AddModelError("", "Could not create a unique batch ID right now. Please try again.");
        return Page();
    }
}
