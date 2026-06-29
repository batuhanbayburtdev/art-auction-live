using ArtAuctionLive.Data;
using ArtAuctionLive.Hubs;
using ArtAuctionLive.Models.Entities;
using ArtAuctionLive.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuctionLive.Services;

// Singleton background loop. Ticks once per second: broadcasts the remaining
// time for every running auction and finishes any whose deadline has passed.
public class AuctionTimerHostedService(
    AuctionTimer timer,
    IServiceScopeFactory scopeFactory,
    IHubContext<AuctionHub> hub,
    ILogger<AuctionTimerHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Rebuild in-memory state from the DB after a (re)start.
        await RestoreRunningAuctions(stoppingToken);

        using var ticker = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (await ticker.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await TickAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // One bad tick must not kill the loop.
                logger.LogError(ex, "Auction tick failed.");
            }
        }
    }

    private async Task RestoreRunningAuctions(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        var running = await db.Auctions
            .Where(a => a.Status == AuctionStatus.Running)
            .Select(a => new { a.Id, a.EndTimeUtc })
            .ToListAsync(ct);

        foreach (var a in running)
            timer.Register(a.Id, a.EndTimeUtc);
    }

    private async Task TickAsync(CancellationToken ct)
    {
        foreach (var (auctionId, endTimeUtc) in timer.Snapshot())
        {
            var remaining = timer.GetRemainingSeconds(auctionId) ?? 0;

            if (remaining > 0)
            {
                // Push the current countdown to everyone in this auction room.
                await hub.Clients.Group(GroupName(auctionId))
                    .SendAsync("TimerTick", auctionId, remaining, ct);
            }
            else
            {
                await FinishAuction(auctionId, ct);
            }
        }
    }

    private async Task FinishAuction(int auctionId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        var auction = await db.Auctions.FirstOrDefaultAsync(a => a.Id == auctionId, ct);
        if (auction is { Status: AuctionStatus.Running })
        {
            auction.Status = AuctionStatus.Finished;
            await db.SaveChangesAsync(ct);
        }

        timer.Remove(auctionId);

        await hub.Clients.Group(GroupName(auctionId))
            .SendAsync("AuctionFinished", auctionId, auction?.CurrentLeader, auction?.CurrentPrice, ct);
    }

    private static string GroupName(int auctionId) => $"auction-{auctionId}";
}
