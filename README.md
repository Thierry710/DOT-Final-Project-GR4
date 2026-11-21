# DOT-Final-Project-GR4 (TraceLinq)

Supply chain traceability system: one ASP.NET Core Razor Pages app (`TraceLinq`) using the EF Core data library (`TraceLinq.Data`). `TraceLinq.Api` is currently unused.

## Setup
1. Install the .NET 10 SDK and SQL Server (LocalDB is fine).
2. If your SQL Server is not `(localdb)\MSSQLLocalDB`, change `ConnectionStrings:DefaultConnection` in `TraceLinq/appsettings.json` (or set the `ConnectionStrings__DefaultConnection` environment variable).
3. `dotnet tool restore`
4. `dotnet run --project TraceLinq` - the database is created automatically and sample users are added (Development only).

## Sample logins (password for all: Pass@123, Development only)
admin (Admin), producer1 (Producer), inspector1 (Inspector), warehouse1 (Warehouse)

## Auth
Cookie authentication with the roles Admin, Producer, Inspector, Warehouse. Pages inside `Pages/Admin`, `Pages/Producer`, `Pages/Inspector` and `Pages/Warehouse` are protected automatically by role (see `TraceLinq/Program.cs`). `Pages/Account` and `Pages/Trace` are public. In a page: `User.UserId()`, `User.FullName()`, `User.RoleName()`.

## Database changes
Edit the models in `TraceLinq.Data`, then:
`dotnet ef migrations add <Name> --project TraceLinq.Data --startup-project TraceLinq`
Migrations are applied on startup. Tell the group before renaming tables or columns.
