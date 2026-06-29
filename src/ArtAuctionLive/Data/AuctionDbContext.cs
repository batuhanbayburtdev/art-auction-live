using ArtAuctionLive.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtAuctionLive.Data;

public class AuctionDbContext(DbContextOptions<AuctionDbContext> options) : DbContext(options)
{
    public DbSet<Painting> Paintings => Set<Painting>();
    public DbSet<Auction> Auctions => Set<Auction>();
    public DbSet<Bid> Bids => Set<Bid>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store the enum as readable text ("Running") instead of an int (1),
        // so the database is self-explanatory and survives enum reordering.
        modelBuilder.Entity<Auction>()
            .Property(a => a.Status)
            .HasConversion<string>();

        // Money: fix precision so SQLite/EF do not silently truncate decimals.
        modelBuilder.Entity<Auction>().Property(a => a.StartingPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Auction>().Property(a => a.CurrentPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Bid>().Property(b => b.Amount).HasPrecision(18, 2);

        // One auction has many bids. Deleting an auction deletes its bids.
        modelBuilder.Entity<Auction>()
            .HasMany(a => a.Bids)
            .WithOne(b => b.Auction)
            .HasForeignKey(b => b.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
