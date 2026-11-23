using TraceLinq.Data;

namespace TraceLinq.Tests;

public class BatchIdGeneratorTests
{
    private static readonly DateOnly Day = new(2026, 11, 23);

    [Fact]
    public void Format_matches_seeder_format()
    {
        Assert.Equal("BT-20261123-001", BatchIdGenerator.Format(Day, 1));
        Assert.Equal("BT-20261123-042", BatchIdGenerator.Format(Day, 42));
    }

    [Fact]
    public void NextNumber_starts_at_1_when_no_batches_today()
    {
        Assert.Equal(1, BatchIdGenerator.NextNumber(Array.Empty<string>(), Day));
    }

    [Fact]
    public void NextNumber_continues_after_highest_counter_even_with_gaps()
    {
        var ids = new[] { "BT-20261123-001", "BT-20261123-005", "BT-20261123-002" };
        Assert.Equal(6, BatchIdGenerator.NextNumber(ids, Day));
    }

    [Fact]
    public void NextNumber_ignores_other_days_and_bad_ids()
    {
        var ids = new[] { "BT-20261122-009", "BT-20261123-abc", "XX-20261123-050", "BT-20261123-003" };
        Assert.Equal(4, BatchIdGenerator.NextNumber(ids, Day));
    }

    [Fact]
    public void NextNumber_goes_past_999()
    {
        // The ID just gets one digit longer; still fits the 30-character column.
        Assert.Equal(1000, BatchIdGenerator.NextNumber(new[] { "BT-20261123-999" }, Day));
        Assert.Equal("BT-20261123-1000", BatchIdGenerator.Format(Day, 1000));
    }
}
