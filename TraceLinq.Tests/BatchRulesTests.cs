using TraceLinq.Data;

namespace TraceLinq.Tests;

public class BatchRulesTests
{
    private static readonly DateOnly Today = new(2026, 11, 23);

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    public void Quantity_must_be_greater_than_zero(string value)
    {
        Assert.Equal("Quantity must be greater than 0.", BatchRules.CheckQuantity(decimal.Parse(value)));
    }

    [Fact]
    public void Quantity_is_required()
    {
        Assert.NotNull(BatchRules.CheckQuantity(null));
    }

    [Fact]
    public void Quantity_rejects_more_than_two_decimals()
    {
        Assert.NotNull(BatchRules.CheckQuantity(1.005m));
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("100")]
    [InlineData("2500.50")]
    public void Valid_quantities_pass(string value)
    {
        Assert.Null(BatchRules.CheckQuantity(decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void Production_date_cannot_be_in_the_future()
    {
        Assert.Equal("Production date cannot be in the future.",
            BatchRules.CheckProductionDate(Today.AddDays(1), Today));
    }

    [Fact]
    public void Production_date_today_or_earlier_is_fine()
    {
        Assert.Null(BatchRules.CheckProductionDate(Today, Today));
        Assert.Null(BatchRules.CheckProductionDate(Today.AddDays(-30), Today));
    }

    [Fact]
    public void Unit_must_be_from_the_list()
    {
        Assert.True(BatchRules.IsValidUnit("kg"));
        Assert.False(BatchRules.IsValidUnit("boxes"));
        Assert.False(BatchRules.IsValidUnit(null));
    }
}
