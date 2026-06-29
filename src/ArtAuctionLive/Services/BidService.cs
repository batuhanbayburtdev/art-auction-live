using System.Collections.Concurrent;
using ArtAuctionLive.Data;
using ArtAuctionLive.Models.Entities;
using ArtAuctionLive.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ArtAuctionLive.Services;

public class BidService(AuctionDbContext db) : IBidService
{
    private const int MaxNameLength = 40;
    private const decimal MaxBid = 1_000_000_000m; // sanity ceiling

    // One lock per auction: bids on the SAME auction are serialised,
    // bids on DIFFERENT auctions still run in parallel.
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> Locks = new();

    public async Task<BidResult> PlaceBidAsync(
        int auctionId, string bidderName, decimal amount, CancellationToken ct = default)
    {
        bidderName = bidderName?.Trim() ?? string.Empty;

        // Validation — never trust the client. The HTML maxlength is UX only.
        if (string.IsNullOrWhiteSpace(bidderName))
            return BidResult.Fail("A bidder name is required.");
        if (bidderName.Length > MaxNameLength)
            return BidResult.Fail($"Name must be {MaxNameLength} characters or fewer.");
        if (amount <= 0)
            return BidResult.Fail("Bid amount must be positive.");
        if (amount > MaxBid)
            return BidResult.Fail("Bid amount is too large.");

        var gate = Locks.GetOrAdd(auctionId, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            // Re-read inside the lock: the price may have changed while waiting.
            var auction = await db.Auctions.FirstOrDefaultAsync(a => a.Id == auctionId, ct);

            if (auction is null)
                return BidResult.Fail("Auction not found.");
            if (auction.Status != AuctionStatus.Running)
                return BidResult.Fail("This auction is not open for bidding.");
            if (DateTime.UtcNow >= auction.EndTimeUtc)
                return BidResult.Fail("This auction has ended.");
            if (amount <= auction.CurrentPrice)
                return BidResult.Fail($"Bid must be higher than the current price ({auction.CurrentPrice:0.00}).");

            db.Bids.Add(new Bid
            {
                AuctionId = auctionId,
                BidderName = bidderName,
                Amount = amount,
                PlacedAtUtc = DateTime.UtcNow
            });

            auction.CurrentPrice = amount;
            auction.CurrentLeader = bidderName;

            await db.SaveChangesAsync(ct);
            return BidResult.Ok(amount, bidderName);
        }
        finally
        {
            gate.Release();
        }
    }
}
