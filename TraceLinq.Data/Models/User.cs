using System.ComponentModel.DataAnnotations;

namespace TraceLinq.Data.Models;

public class User
{
    public int UserId { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = null!;

    [Required, MaxLength(200)]
    public string PasswordHash { get; set; } = null!;

    [Required, MaxLength(100)]
    public string FullName { get; set; } = null!;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    public ICollection<QualityCheck> QualityChecks { get; set; } = new List<QualityCheck>();
    public ICollection<WarehouseStorage> WarehouseStorages { get; set; } = new List<WarehouseStorage>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
