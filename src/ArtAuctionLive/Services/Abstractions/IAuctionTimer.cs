namespace ArtAuctionLive.Services.Abstractions;

public interface IAuctionTimer
{
    // Start tracking an auction with its initial deadline.
    void Register(int auctionId, DateTime endTimeUtc);

    // Bump the deadline (overtime extension on a last-second bid).
    void Extend(int auctionId, DateTime newEndTimeUtc);

    // Seconds left for an auction, or null if it is not being tracked.
    int? GetRemainingSeconds(int auctionId);
}
