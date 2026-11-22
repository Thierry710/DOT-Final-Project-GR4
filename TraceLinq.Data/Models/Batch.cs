using System.ComponentModel.DataAnnotations;

namespace TraceLinq.Data.Models;

public class Batch
{
    /// <summary>Generated in code, e.g. BT-yyyyMMdd-NNN. Not database-generated.</summary>
    [Key, MaxLength(30)]
    public string BatchId { get; set; } = null!;

    [Required, MaxLength(100)]
    public string ProductType { get; set; } = null!;

    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Quantity { get; set; }

    [Required, MaxLength(20)]
    public string Unit { get; set; } = null!;

    [Required, MaxLength(150)]
    public string OriginLocation { get; set; } = null!;

    public DateOnly ProductionDate { get; set; }

    public int ProducerId { get; set; }

    public BatchStatus Status { get; set; } = BatchStatus.Registered;

    public DateTime CreatedAt { get; set; }

    public User Producer { get; set; } = null!;
    public QualityCheck? QualityCheck { get; set; }
    public WarehouseStorage? WarehouseStorage { get; set; }
}
