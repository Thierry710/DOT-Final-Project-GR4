using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TraceLinq.Data;

/// <summary>Used only by `dotnet ef` so migrations can be created without starting the web app.</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? @"Server=(localdb)\MSSQLLocalDB;Database=SupplyChainDB;Trusted_Connection=True;TrustServerCertificate=True;";
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(cs).Options;
        return new AppDbContext(options);
    }
}
