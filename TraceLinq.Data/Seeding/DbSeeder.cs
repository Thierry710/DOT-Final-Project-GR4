using TraceLinq.Data.Models;
using TraceLinq.Data.Security;

namespace TraceLinq.Data.Seeding;

/// <summary>Development-only sample data. Does nothing if users already exist.</summary>
public static class DbSeeder
{
    public const string SamplePassword = "Pass@123";

    public static void Seed(AppDbContext db)
    {
        if (!db.Users.Any())
        {
            db.Users.AddRange(
                NewUser("admin", "System Admin", UserRole.Admin),
                NewUser("producer1", "Jean Producer", UserRole.Producer),
                NewUser("inspector1", "Alice Inspector", UserRole.Inspector),
                NewUser("warehouse1", "Eric Warehouse", UserRole.Warehouse));
        }

        if (!db.Locations.Any())
        {
            db.Locations.AddRange(
                new Location { LocationName = "Section A, Shelf 1" },
                new Location { LocationName = "Section A, Shelf 2" },
                new Location { LocationName = "Section B, Shelf 1" },
                new Location { LocationName = "Section B, Shelf 3" },
                new Location { LocationName = "Cold Room 1" });
        }
        if (!db.Batches.Any())
        {
            var producer = db.Users.First(u => u.Role == UserRole.Producer);

            db.Batches.Add(new Batch
            {
                BatchId = "BATCH-001",
                ProductType = "Coffee Beans",
                Quantity = 100,
                Unit = "kg",
                OriginLocation = "Nyamagabe, Rwanda",
                ProductionDate = DateOnly.FromDateTime(DateTime.Today),
                ProducerId = producer.UserId,
                Status = BatchStatus.Registered,
            });

        }

        db.SaveChanges();
    }

    private static User NewUser(string username, string fullName, UserRole role) => new()
    {
        Username = username,
        FullName = fullName,
        Role = role,
        PasswordHash = PasswordHasher.Hash(SamplePassword),
        IsActive = true
    };
}
