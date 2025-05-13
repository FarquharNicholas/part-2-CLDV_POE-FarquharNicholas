using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ST10439055_POE.Models;
using Microsoft.EntityFrameworkCore;

namespace ST10439055_POE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EventEaseContext _context;

        public HomeController(ILogger<HomeController> logger, EventEaseContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                ViewBag.BookingsCount = await _context.Bookings.CountAsync();
                ViewBag.EventsCount = await _context.Events.CountAsync();
                ViewBag.VenuesCount = await _context.Venues.CountAsync();

                ViewBag.RecentBookings = await _context.Bookings
                    .Include(b => b.Event)
                    .Include(b => b.Venue)
                    .OrderByDescending(b => b.BookingDate)
                    .Take(5)
                    .AsNoTracking()
                    .ToListAsync();

                ViewBag.UpcomingEvents = await _context.Events
                    .Include(e => e.Venue)
                    .Where(e => e.EventDate >= DateTime.Now.Date)
                    .OrderBy(e => e.EventDate)
                    .Take(5)
                    .AsNoTracking()
                    .ToListAsync();

                ViewBag.PopularVenues = await _context.Venues
                    .OrderByDescending(v => v.Capacity)
                    .Take(5)
                    .AsNoTracking()
                    .ToListAsync();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                TempData["ErrorMessage"] = "Failed to load dashboard data. Please try again later.";
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = "Failed to load dashboard data. Please try again later."
                });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult ViewBookings()
        {
            return RedirectToAction("Index", "Bookings");
        }

        public IActionResult ViewEvents()
        {
            return RedirectToAction("Index", "Events");
        }

        public IActionResult ViewVenues()
        {
            return RedirectToAction("Index", "Venues");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}