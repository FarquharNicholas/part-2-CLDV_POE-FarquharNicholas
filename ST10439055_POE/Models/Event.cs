using System.Collections.Generic;

namespace ST10439055_POE.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string? Description { get; set; }  
        public int? VenueId { get; set; }       

        
        public Venue? Venue { get; set; }             
        public ICollection<Bookings> Bookings { get; set; } = new List<Bookings>();  
    }
}