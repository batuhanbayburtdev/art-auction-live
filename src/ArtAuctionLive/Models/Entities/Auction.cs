namespace ArtAuctionLive.Models.Entities;

public class Auction
{
    public int Id { get; set; }

    public int PaintingId { get; set; }
    public Painting Painting { get; set; } = null!;

    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public string? CurrentLeader { get; set; }

    public AuctionStatus Status { get; set; } = AuctionStatus.Pending;

    // Always UTC. The browser localises for display.
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }

    public List<Bid> Bids { get; set; } = [];
}
