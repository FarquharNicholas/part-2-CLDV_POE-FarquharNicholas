using System.ComponentModel.DataAnnotations.Schema;

namespace ST10439055_POE.Models
{ 
    public class Venue
    {
    public int VenueId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? ImageUrl { get; set; }

    [NotMapped]
    public IFormFile? ImageFile { get; set; }

    // Navigation properties
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<Bookings> Bookings { get; set; } = new List<Bookings>(); // Add this line
    }
}