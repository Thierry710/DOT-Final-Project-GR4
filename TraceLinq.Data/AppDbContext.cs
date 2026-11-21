using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TraceLinq.Data.Models;

namespace TraceLinq.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<QualityCheck> QualityChecks => Set<QualityCheck>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<WarehouseStorage> WarehouseStorages => Set<WarehouseStorage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // The DB stores 'In Storage' (with a space), so these enums need explicit mapping.
    private static readonly ValueConverter<BatchStatus, string> BatchStatusConverter = new(
        v => v == BatchStatus.InStorage ? "In Storage" : v.ToString(),
        v => v == "In Storage" ? BatchStatus.InStorage : Enum.Parse<BatchStatus>(v));

    private static readonly ValueConverter<StorageStatus, string> StorageStatusConverter = new(
        v => v == StorageStatus.InStorage ? "In Storage" : v.ToString(),
        v => v == "In Storage" ? StorageStatus.InStorage : Enum.Parse<StorageStatus>(v));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Role).HasMaxLength(20).HasConversion<string>();
            // ValueGeneratedNever so an explicit IsActive = false is sent (not swallowed by the default).
            e.Property(x => x.IsActive).HasDefaultValueSql("1").ValueGeneratedNever();
            e.Property(x => x.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Batch>(e =>
        {
            e.ToTable("Batches");
            e.Property(x => x.BatchId).ValueGeneratedNever();
            e.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
            e.Property(x => x.ProductionDate).HasColumnType("date");
            e.Property(x => x.Status).HasMaxLength(20).HasConversion(BatchStatusConverter)
                .HasDefaultValueSql("'Registered'");
            e.Property(x => x.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            e.HasOne(x => x.Producer).WithMany(u => u.Batches)
                .HasForeignKey(x => x.ProducerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QualityCheck>(e =>
        {
            e.ToTable("QualityChecks");
            e.HasIndex(x => x.BatchId).IsUnique();
            e.Property(x => x.Result).HasMaxLength(10).HasConversion<string>();
            e.Property(x => x.CheckedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            e.HasOne(x => x.Batch).WithOne(b => b.QualityCheck)
                .HasForeignKey<QualityCheck>(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Inspector).WithMany(u => u.QualityChecks)
                .HasForeignKey(x => x.InspectorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Location>(e =>
        {
            e.ToTable("Locations");
            e.HasIndex(x => x.LocationName).IsUnique();
        });

        modelBuilder.Entity<WarehouseStorage>(e =>
        {
            e.ToTable("WarehouseStorage");
            e.HasIndex(x => x.BatchId).IsUnique();
            e.Property(x => x.ArrivalTime).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            e.Property(x => x.DispatchedAt).HasColumnType("datetime");
            e.Property(x => x.Status).HasMaxLength(20).HasConversion(StorageStatusConverter)
                .HasDefaultValueSql("'In Storage'");
            e.HasOne(x => x.Batch).WithOne(b => b.WarehouseStorage)
                .HasForeignKey<WarehouseStorage>(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Operator).WithMany(u => u.WarehouseStorages)
                .HasForeignKey(x => x.OperatorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLog");
            e.Property(x => x.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            e.HasOne(x => x.User).WithMany(u => u.AuditLogs)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
