namespace TraceLinq.Data;

// Validation rules for registering a batch. Kept here (not in the page) so they
// can be unit tested without a web server or database.
public static class BatchRules
{
    // Units a producer can choose from on the Register form.
    public static readonly string[] Units = { "kg", "t", "L", "bags", "crates", "pieces" };

    public const decimal MaxQuantity = 1_000_000_000m;

    public static bool IsValidUnit(string? unit) => unit != null && Units.Contains(unit);

    // Returns an error message, or null when the quantity is fine.
    public static string? CheckQuantity(decimal? quantity)
    {
        if (quantity is null)
            return "Quantity is required.";
        if (quantity <= 0)
            return "Quantity must be greater than 0.";
        if (quantity > MaxQuantity)
            return "Quantity cannot be more than 1,000,000,000.";
        // The database column is decimal(18,2): more decimals would be silently rounded.
        if (decimal.Round(quantity.Value, 2) != quantity.Value)
            return "Quantity can have at most 2 decimal places.";
        return null;
    }

    // Returns an error message, or null when the date is fine. "today" is passed in
    // so tests do not depend on the real clock.
    public static string? CheckProductionDate(DateOnly? date, DateOnly today)
    {
        if (date is null)
            return "Production date is required.";
        if (date > today)
            return "Production date cannot be in the future.";
        return null;
    }
}
