using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TraceLinq.Data;
using TraceLinq.Data.Seeding;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/AccessDenied";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    o.AddPolicy("ProducerOnly", p => p.RequireRole("Producer"));
    o.AddPolicy("InspectorOnly", p => p.RequireRole("Inspector"));
    o.AddPolicy("WarehouseOnly", p => p.RequireRole("Warehouse"));
});

builder.Services.AddRazorPages(o =>
{
    // Everything requires login by default...
    o.Conventions.AuthorizeFolder("/");
    // ...except these:
    o.Conventions.AllowAnonymousToFolder("/Account");
    o.Conventions.AllowAnonymousToFolder("/Trace");   // public QR lookup
    o.Conventions.AllowAnonymousToPage("/Index");
    o.Conventions.AllowAnonymousToPage("/Privacy");
    o.Conventions.AllowAnonymousToPage("/Error");
    // Role folders: pages placed inside are protected automatically.
    o.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    o.Conventions.AuthorizeFolder("/Producer", "ProducerOnly");
    o.Conventions.AuthorizeFolder("/Inspector", "InspectorOnly");
    o.Conventions.AuthorizeFolder("/Warehouse", "WarehouseOnly");
});

var app = builder.Build();

// Create/upgrade the database on startup; sample users only in Development.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    if (app.Environment.IsDevelopment()) DbSeeder.Seed(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.Run();
