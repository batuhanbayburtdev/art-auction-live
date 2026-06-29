using ArtAuctionLive.Models.Entities;

namespace ArtAuctionLive.Data;

public static class DbSeeder
{
    public static void Seed(AuctionDbContext db)
    {
        // Idempotent: only seed an empty database.
        if (db.Paintings.Any())
            return;

        var paintings = new List<Painting>
        {
            new() { Title = "Starlit Harbour", Artist = "M. Avery", Description = "Oil on canvas, 1962.", ImageUrl = "/img/starlit-harbour.svg" },
            new() { Title = "Red Meadow", Artist = "L. Fontaine", Description = "Acrylic, 2018.", ImageUrl = "/img/red-meadow.svg" },
            new() { Title = "Quiet Cathedral", Artist = "H. Okonkwo", Description = "Watercolour, 2005.", ImageUrl = "/img/quiet-cathedral.svg" }
        };
        db.Paintings.AddRange(paintings);
        db.SaveChanges();

        var now = DateTime.UtcNow;
        var auctions = new List<Auction>
        {
            new()
            {
                PaintingId = paintings[0].Id,
                StartingPrice = 100m,
                CurrentPrice = 100m,
                Status = AuctionStatus.Running,
                StartTimeUtc = now,
                EndTimeUtc = now.AddMinutes(5)
            },
            new()
            {
                PaintingId = paintings[1].Id,
                StartingPrice = 250m,
                CurrentPrice = 250m,
                Status = AuctionStatus.Running,
                StartTimeUtc = now,
                EndTimeUtc = now.AddMinutes(10)
            }
        };
        db.Auctions.AddRange(auctions);
        db.SaveChanges();
    }
}
