using ArtAuctionLive.Data;
using ArtAuctionLive.Hubs;
using ArtAuctionLive.Services;
using ArtAuctionLive.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Anchor the SQLite file to the content root so every launch method resolves
// to the same database file.
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "artauction.db");
builder.Services.AddDbContext<AuctionDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Application services
builder.Services.AddScoped<IBidService, BidService>();

// One AuctionTimer instance, exposed both as itself (for the background loop)
// and as IAuctionTimer (for the hub) so both share the same state.
builder.Services.AddSingleton<AuctionTimer>();
builder.Services.AddSingleton<IAuctionTimer>(sp => sp.GetRequiredService<AuctionTimer>());
builder.Services.AddHostedService<AuctionTimerHostedService>();

var app = builder.Build();

// Apply migrations and seed on startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<AuctionHub>("/auctionHub");

app.Run();
