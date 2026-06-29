using ArtAuctionLive.Models.Entities;

namespace ArtAuctionLive.ViewModels;

// Everything the single-auction room page needs to render its initial state.
public class AuctionRoomViewModel
{
    public int Id { get; init; }
    public required string PaintingTitle { get; init; }
    public required string Artist { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }

    public decimal CurrentPrice { get; init; }
    public string? CurrentLeader { get; init; }
    public AuctionStatus Status { get; init; }
    public DateTime EndTimeUtc { get; init; }

    public IReadOnlyList<BidRowViewModel> RecentBids { get; init; } = [];
}

public class BidRowViewModel
{
    public required string BidderName { get; init; }
    public decimal Amount { get; init; }
    public DateTime PlacedAtUtc { get; init; }
}
