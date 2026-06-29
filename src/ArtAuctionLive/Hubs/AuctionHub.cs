using ArtAuctionLive.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace ArtAuctionLive.Hubs;

public class AuctionHub(IBidService bidService, IAuctionTimer timer) : Hub
{
    private static string GroupName(int auctionId) => $"auction-{auctionId}";

    // Client calls this after connecting (and after any reconnect) to join a room.
    public async Task JoinAuction(int auctionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(auctionId));

        // Send the joining client the current countdown immediately,
        // so it does not wait up to a second for the next tick.
        var remaining = timer.GetRemainingSeconds(auctionId);
        if (remaining is not null)
            await Clients.Caller.SendAsync("TimerTick", auctionId, remaining.Value);
    }

    public async Task LeaveAuction(int auctionId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(auctionId));

    // Client calls this to bid. The hub does NOT contain bidding rules —
    // it delegates to BidService and broadcasts the outcome.
    public async Task PlaceBid(int auctionId, string bidderName, decimal amount)
    {
        var result = await bidService.PlaceBidAsync(auctionId, bidderName, amount);

        if (!result.Success)
        {
            // Tell only the bidder why it failed.
            await Clients.Caller.SendAsync("BidRejected", result.Error);
            return;
        }

        // Overtime: a bid in the final seconds extends the deadline.
        var remaining = timer.GetRemainingSeconds(auctionId) ?? 0;
        if (remaining <= 30)
            timer.Extend(auctionId, DateTime.UtcNow.AddSeconds(30));

        // Broadcast the new price/leader to everyone in the room.
        await Clients.Group(GroupName(auctionId))
            .SendAsync("BidPlaced", auctionId, result.NewPrice, result.Leader, bidderName);
    }
}
