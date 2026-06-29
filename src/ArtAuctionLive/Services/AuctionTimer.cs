using System.Collections.Concurrent;
using ArtAuctionLive.Services.Abstractions;

namespace ArtAuctionLive.Services;

// Singleton. Holds the live deadline for each running auction in memory.
// Thread-safe because the background loop and incoming bids touch it concurrently.
public class AuctionTimer : IAuctionTimer
{
    private readonly ConcurrentDictionary<int, DateTime> _deadlines = new();

    public void Register(int auctionId, DateTime endTimeUtc)
        => _deadlines[auctionId] = endTimeUtc;

    public void Extend(int auctionId, DateTime newEndTimeUtc)
        => _deadlines[auctionId] = newEndTimeUtc;

    public int? GetRemainingSeconds(int auctionId)
    {
        if (!_deadlines.TryGetValue(auctionId, out var end))
            return null;

        var remaining = (int)Math.Ceiling((end - DateTime.UtcNow).TotalSeconds);
        return remaining < 0 ? 0 : remaining;
    }

    // Used by the background loop to walk every tracked auction each tick.
    public IReadOnlyDictionary<int, DateTime> Snapshot() => _deadlines;

    // Stop tracking once an auction is finished.
    public void Remove(int auctionId) => _deadlines.TryRemove(auctionId, out _);
}
