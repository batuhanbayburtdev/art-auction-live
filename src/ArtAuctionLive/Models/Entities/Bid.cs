namespace ArtAuctionLive.Models.Entities;

public class Bid
{
    public int Id { get; set; }

    public int AuctionId { get; set; }
    public Auction Auction { get; set; } = null!;

    public required string BidderName { get; set; }
    public decimal Amount { get; set; }
    public DateTime PlacedAtUtc { get; set; }
}
