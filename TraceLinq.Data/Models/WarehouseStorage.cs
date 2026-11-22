using System.ComponentModel.DataAnnotations;

namespace TraceLinq.Data.Models;

public class WarehouseStorage
{
    public int StorageId { get; set; }

    [Required, MaxLength(30)]
    public string BatchId { get; set; } = null!;

    public int OperatorId { get; set; }

    [Required, MaxLength(100)]
    public string StorageLocation { get; set; } = null!;

    public DateTime ArrivalTime { get; set; }

    public StorageStatus Status { get; set; } = StorageStatus.InStorage;

    public DateTime? DispatchedAt { get; set; }

    public Batch Batch { get; set; } = null!;
    public User Operator { get; set; } = null!;
}
