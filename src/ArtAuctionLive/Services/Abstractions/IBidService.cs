namespace ArtAuctionLive.Services.Abstractions;

public interface IBidService
{
    Task<BidResult> PlaceBidAsync(int auctionId, string bidderName, decimal amount, CancellationToken ct = default);
}
