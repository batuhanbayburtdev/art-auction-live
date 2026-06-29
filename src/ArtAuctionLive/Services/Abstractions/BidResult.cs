namespace ArtAuctionLive.Services.Abstractions;

// Outcome of a bid attempt. Carries either an error message or the new state.
public record BidResult(bool Success, string? Error, decimal? NewPrice, string? Leader)
{
    public static BidResult Fail(string error) => new(false, error, null, null);
    public static BidResult Ok(decimal newPrice, string leader) => new(true, null, newPrice, leader);
}
