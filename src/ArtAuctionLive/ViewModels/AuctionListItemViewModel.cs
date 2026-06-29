using ArtAuctionLive.Models.Entities;

namespace ArtAuctionLive.ViewModels;

// One row on the auctions index page.
public class AuctionListItemViewModel
{
    public int Id { get; init; }
    public required string PaintingTitle { get; init; }
    public required string Artist { get; init; }
    public string? ImageUrl { get; init; }
    public decimal CurrentPrice { get; init; }
    public AuctionStatus Status { get; init; }
}
