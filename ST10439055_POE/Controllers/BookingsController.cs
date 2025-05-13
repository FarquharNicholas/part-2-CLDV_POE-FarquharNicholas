using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ST10439055_POE.Models;
using Microsoft.Extensions.Logging;

namespace ST10439055_POE.Controllers
{
    public class BookingsController : Controller
    {
        private readonly EventEaseContext _context;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(EventEaseContext context, ILogger<BookingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Bookings
        public async Task<IActionResult> Index(string searchString)
        {
            // Log the search string to debug
            _logger.LogInformation("Search string received: {SearchString}", searchString);

            // Set ViewData to retain search input in the view
            ViewData["CurrentFilter"] = searchString;

            var bookingsQuery = from b in _context.Bookings
                                join e in _context.Events on b.EventId equals e.EventId
                                join v in _context.Venues on b.VenueId equals v.VenueId
                                select new BookingViewModel
                                {
                                    BookingId = b.BookingId,
                                    BookingDate = b.BookingDate,
                                    CreatedDate = b.CreatedDate,
                                    EventId = b.EventId,
                                    EventName = e.EventName,
                                    EventDate = e.EventDate,
                                    EventDescription = e.Description,
                                    VenueId = b.VenueId,
                                    VenueName = v.VenueName,
                                    VenueLocation = v.Location,
                                    VenueCapacity = v.Capacity,
                                    VenueImageUrl = v.ImageUrl
                                };

            if (!string.IsNullOrEmpty(searchString))
            {
                // Trim input to handle whitespace
                searchString = searchString.Trim();

                // Try parsing searchString as an integer for BookingId
                bool isNumeric = int.TryParse(searchString, out int bookingId);

                // Use ToLower for case-insensitive search, which EF Core can translate to SQL
                bookingsQuery = bookingsQuery.Where(b =>
                    (isNumeric && b.BookingId == bookingId) ||
                    b.EventName.ToLower().Contains(searchString.ToLower()));

                _logger.LogInformation("Applying search filter: BookingId={BookingId}, EventName contains '{SearchString}'", bookingId, searchString);
            }

            var bookings = await bookingsQuery.OrderByDescending(b => b.BookingDate).ToListAsync();

            if (!bookings.Any() && !string.IsNullOrEmpty(searchString))
            {
                TempData["InfoMessage"] = "No bookings found matching your search criteria.";
                _logger.LogInformation("No bookings found for search: {SearchString}", searchString);
            }

            return View(bookings);
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (bookings == null)
            {
                return NotFound();
            }

            return View(bookings);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingDate,EventId,VenueId")] Bookings booking)
        {
            // Check for double booking (same venue and date)
            var existingDateBooking = await _context.Bookings
                .AnyAsync(b => b.VenueId == booking.VenueId && b.BookingDate == booking.BookingDate);
            if (existingDateBooking)
            {
                TempData["ErrorMessage"] = "This venue is already booked for the selected date and time.";
                ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            // Check for unique EventId and VenueId constraint
            var existingEventVenueBooking = await _context.Bookings
                .AnyAsync(b => b.EventId == booking.EventId && b.VenueId == booking.VenueId);
            if (existingEventVenueBooking)
            {
                TempData["ErrorMessage"] = "A booking for this event at this venue already exists.";
                ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    booking.CreatedDate = DateTime.Now;
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 2627)
                {
                    TempData["ErrorMessage"] = "A booking for this event at this venue already exists.";
                    ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                    ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                    return View(booking);
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred while creating the booking. Please try again.";
                    ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                    ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                    return View(booking);
                }
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookings = await _context.Bookings.FindAsync(id);
            if (bookings == null)
            {
                return NotFound();
            }
            ViewData["EventID"] = new SelectList(_context.Events, "EventId", "EventName", bookings.EventId);
            ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "Location", bookings.VenueId);
            return View(bookings);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,BookingDate")] Bookings bookings)
        {
            if (id != bookings.BookingId)
            {
                return NotFound();
            }

            // Check for double booking during edit
            var existingDateBooking = await _context.Bookings
                .AnyAsync(b => b.VenueId == bookings.VenueId && b.BookingDate == bookings.BookingDate && b.BookingId != bookings.BookingId);
            if (existingDateBooking)
            {
                TempData["ErrorMessage"] = "This venue is already booked for the selected date and time.";
                ViewData["EventID"] = new SelectList(_context.Events, "EventId", "EventName", bookings.EventId);
                ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "Location", bookings.VenueId);
                return View(bookings);
            }

            // Check for unique EventId and VenueId constraint during edit
            var existingEventVenueBooking = await _context.Bookings
                .AnyAsync(b => b.EventId == bookings.EventId && b.VenueId == bookings.VenueId && b.BookingId != bookings.BookingId);
            if (existingEventVenueBooking)
            {
                TempData["ErrorMessage"] = "A booking for this event at this venue already exists.";
                ViewData["EventID"] = new SelectList(_context.Events, "EventId", "EventName", bookings.EventId);
                ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "Location", bookings.VenueId);
                return View(bookings);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookings);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingsExists(bookings.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 2627)
                {
                    TempData["ErrorMessage"] = "A booking for this event at this venue already exists.";
                    ViewData["EventID"] = new SelectList(_context.Events, "EventId", "EventName", bookings.EventId);
                    ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "Location", bookings.VenueId);
                    return View(bookings);
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred while updating the booking. Please try again.";
                    ViewData["EventID"] = new SelectList(_context.Events, "EventId", "EventName", bookings.EventId);
                    ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "Location", bookings.VenueId);
                    return View(bookings);
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventID"] = new SelectList(_context.Events, "EventId", "EventName", bookings.EventId);
            ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "Location", bookings.VenueId);
            return View(bookings);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (bookings == null)
            {
                return NotFound();
            }

            return View(bookings);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookings = await _context.Bookings.FindAsync(id);
            if (bookings != null)
            {
                _context.Bookings.Remove(bookings);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Booking deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookingsExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}