using System.ComponentModel.DataAnnotations;

namespace TraceLinq.Data.Models;

public class Location
{
    public int LocationId { get; set; }

    [Required, MaxLength(100)]
    public string LocationName { get; set; } = null!;
}
