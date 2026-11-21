using System.ComponentModel.DataAnnotations;

namespace TraceLinq.Data.Models;

public class QualityCheck
{
    public int CheckId { get; set; }

    [Required, MaxLength(30)]
    public string BatchId { get; set; } = null!;

    public int InspectorId { get; set; }

    public QualityResult Result { get; set; }

    [MaxLength(300)]
    public string? Remarks { get; set; }

    public DateTime CheckedAt { get; set; }

    public Batch Batch { get; set; } = null!;
    public User Inspector { get; set; } = null!;
}
