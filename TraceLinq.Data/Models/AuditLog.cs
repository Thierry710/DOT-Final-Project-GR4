using System.ComponentModel.DataAnnotations;

namespace TraceLinq.Data.Models;

public class AuditLog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    [Required, MaxLength(200)]
    public string Action { get; set; } = null!;

    // No FK in the schema: plain reference, intentionally not a navigation.
    [MaxLength(30)]
    public string? BatchId { get; set; }

    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}
