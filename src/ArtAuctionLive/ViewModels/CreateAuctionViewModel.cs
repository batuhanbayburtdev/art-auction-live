using System.ComponentModel.DataAnnotations;

namespace ArtAuctionLive.ViewModels;

// Bound to the create-auction form. Validation attributes drive both
// client-side hints and server-side ModelState checks.
public class CreateAuctionViewModel
{
    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Artist { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Url, StringLength(500)]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    [Required]
    [Range(1, 1_000_000, ErrorMessage = "Starting price must be between 1 and 1,000,000.")]
    [Display(Name = "Starting price (€)")]
    public decimal StartingPrice { get; set; }

    [Required]
    [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes.")]
    [Display(Name = "Duration (minutes)")]
    public int DurationMinutes { get; set; } = 60;
}
