namespace ArtAuctionLive.Models.Entities;

public class Painting
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Artist { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
