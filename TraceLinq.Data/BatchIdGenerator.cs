using System.Globalization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace TraceLinq.Data;

// Creates batch IDs in the format BT-yyyyMMdd-NNN (e.g. BT-20261123-001),
// the same format the seeder uses. The counter NNN restarts at 001 every day.
//
// Uniqueness: BatchId is the primary key of the Batches table, so SQL Server
// itself refuses a duplicate. If two producers register at the same moment they
// can both compute the same "next" ID; one save succeeds and the other fails
// with a duplicate-key error. The caller catches that (IsDuplicateKey) and tries
// again with a freshly computed ID. See Pages/Producer/Register.cshtml.cs.
public static class BatchIdGenerator
{
    public const string Prefix = "BT";

    // "BT-20261123-"
    public static string PrefixFor(DateOnly date) =>
        Prefix + "-" + date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "-";

    // (2026-11-23, 7) -> "BT-20261123-007"
    public static string Format(DateOnly date, int number) =>
        PrefixFor(date) + number.ToString("000", CultureInfo.InvariantCulture);

    // Looks at the IDs already used and returns the next free counter for that day.
    // IDs from other days or in an unexpected format are ignored.
    public static int NextNumber(IEnumerable<string> existingIds, DateOnly date)
    {
        var prefix = PrefixFor(date);
        var max = 0;

        foreach (var id in existingIds)
        {
            if (!id.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            var counter = id.Substring(prefix.Length);
            if (int.TryParse(counter, NumberStyles.None, CultureInfo.InvariantCulture, out var n) && n > max)
                max = n;
        }

        return max + 1;
    }

    // Reads today's IDs from the database and returns the next one, e.g. "BT-20261123-002".
    public static async Task<string> NextIdAsync(AppDbContext db, DateOnly date)
    {
        var prefix = PrefixFor(date);

        var todaysIds = await db.Batches
            .Where(b => b.BatchId.StartsWith(prefix))
            .Select(b => b.BatchId)
            .ToListAsync();

        return Format(date, NextNumber(todaysIds, date));
    }

    // True when a save failed because the primary key / a unique index already exists.
    // 2627 = PRIMARY KEY / UNIQUE constraint violation, 2601 = unique index violation.
    public static bool IsDuplicateKey(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2627 or 2601 };
}
