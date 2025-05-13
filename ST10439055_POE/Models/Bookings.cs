using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10439055_POE.Models
{
    public class Bookings
    {
        
        public int BookingId { get; set; }  
        public int EventId { get; set; }   
        public int VenueId { get; set; } 
        public DateTime BookingDate { get; set; } = DateTime.Now;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public Event? Event { get; set; }
        public Venue? Venue { get; set; }
    }
}