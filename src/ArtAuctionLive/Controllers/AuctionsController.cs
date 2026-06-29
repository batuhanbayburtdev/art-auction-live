using ArtAuctionLive.Data;
using ArtAuctionLive.Models.Entities;
using ArtAuctionLive.Services.Abstractions;
using ArtAuctionLive.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtAuctionLive.Controllers;

public class AuctionsController(
    AuctionDbContext db,
    IAuctionTimer timer,
    ILogger<AuctionsController> logger) : Controller
{
    public async Task<IActionResult> Index()
    {
        var auctions = await db.Auctions
            .Include(a => a.Painting)
            .OrderBy(a => a.EndTimeUtc)
            .Select(a => new AuctionListItemViewModel
            {
                Id = a.Id,
                PaintingTitle = a.Painting.Title,
                Artist = a.Painting.Artist,
                ImageUrl = a.Painting.ImageUrl,
                CurrentPrice = a.CurrentPrice,
                Status = a.Status
            })
            .ToListAsync();

        return View(auctions);
        
    }

    public async Task<IActionResult> Room(int id)
    {
        if (id <= 0)
            return BadRequest();

        var auction = await db.Auctions
            .Include(a => a.Painting)
            .Include(a => a.Bids)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction is null)
            
    
        {
            logger.LogWarning("Auction {AuctionId} not found.", id);
            return NotFound();
        }

        var vm = new AuctionRoomViewModel
        {
            Id = auction.Id,
            PaintingTitle = auction.Painting.Title,
            Artist = auction.Painting.Artist,
            Description = auction.Painting.Description,
            ImageUrl = auction.Painting.ImageUrl,
            CurrentPrice = auction.CurrentPrice,
            CurrentLeader = auction.CurrentLeader,
            Status = auction.Status,
            EndTimeUtc = auction.EndTimeUtc,
            RecentBids = auction.Bids
                .OrderByDescending(b => b.PlacedAtUtc)
                .Take(10)
                .Select(b => new BidRowViewModel
                {
                    BidderName = b.BidderName,
                    Amount = b.Amount,
                    PlacedAtUtc = b.PlacedAtUtc
                })
                .ToList()
        };

        return View(vm);
    }
    public IActionResult Create()
    {
        return View(new CreateAuctionViewModel());
    }
    // POST /Auctions/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAuctionViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var painting = new Painting
        {
            Title = model.Title.Trim(),
            Artist = model.Artist.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim()
        };

        var now = DateTime.UtcNow;
        var auction = new Auction
        {
            Painting = painting,
            StartingPrice = model.StartingPrice,
            CurrentPrice = model.StartingPrice,
            Status = AuctionStatus.Running,
            StartTimeUtc = now,
            EndTimeUtc = now.AddMinutes(model.DurationMinutes)
        };

        db.Auctions.Add(auction);
        await db.SaveChangesAsync();

        // Register with the live timer so the countdown starts immediately.
        timer.Register(auction.Id, auction.EndTimeUtc);

        logger.LogInformation("Created auction {AuctionId} for '{Title}'.", auction.Id, painting.Title);

        return RedirectToAction(nameof(Room), new { id = auction.Id });
    }
    
}
