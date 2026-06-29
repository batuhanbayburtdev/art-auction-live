namespace ArtAuctionLive.Models.Entities;

public enum AuctionStatus
{
    Pending,   // created, not started yet
    Running,   // accepting bids, countdown active
    Finished   // timer expired, winner locked in
}
