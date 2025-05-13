using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10439055_POE.Models;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace ST10439055_POE.Controllers
{
    public class VenuesController : Controller
    {
        private readonly EventEaseContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VenuesController> _logger;

        public VenuesController(EventEaseContext context, IConfiguration configuration, ILogger<VenuesController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // GET: Venues
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venues.ToListAsync());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,VenueName,Location,Capacity,ImageFile")] Venue venue)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (venue.ImageFile != null && venue.ImageFile.Length > 0)
                    {
                        venue.ImageUrl = await UploadImageToBlobAsync(venue.ImageFile);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Venue image is required.";
                        ModelState.AddModelError("ImageFile", "Venue image is required.");
                        return View(venue);
                    }

                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating venue: {VenueName}", venue.VenueName);
                    TempData["ErrorMessage"] = "An error occurred while creating the venue. Please try again.";
                    return View(venue);
                }
            }
            return View(venue);
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,ImageUrl,ImageFile")] Venue venue)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (venue.ImageFile != null && venue.ImageFile.Length > 0)
                    {
                        venue.ImageUrl = await UploadImageToBlobAsync(venue.ImageFile);
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating venue: {VenueId}", venue.VenueId);
                    TempData["ErrorMessage"] = "An error occurred while updating the venue. Please try again.";
                    return View(venue);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                TempData["ErrorMessage"] = "The venue could not be found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Check for active bookings
                var hasBookings = await _context.Bookings.AnyAsync(b => b.VenueId == id);
                if (hasBookings)
                {
                    TempData["ErrorMessage"] = "Cannot delete this venue because it has active bookings.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                // Check for associated events
                var hasEvents = await _context.Events.AnyAsync(e => e.VenueId == id);
                if (hasEvents)
                {
                    TempData["ErrorMessage"] = "Cannot delete this venue because it is associated with one or more events.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venue deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting venue: {VenueId}", id);
                TempData["ErrorMessage"] = "A database error occurred while deleting the venue. It may be referenced by other records.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting venue: {VenueId}", id);
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the venue. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }

        private async Task<string> UploadImageToBlobAsync(IFormFile imageFile)
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=st10439055;AccountKey=6Boqk6isoEfVH9i9eIcRLphz0P38tr/IoQzAmJr8T0qGMz3+qTcKCPRtrz8FsFn22tMtIGFCxMBY+AStgtK5/Q==;EndpointSuffix=core.windows.net";
            var containerName = "cldv6211st10439055";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

            var blobName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var blobClient = containerClient.GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = imageFile.ContentType
            };

            using (var stream = imageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });
            }

            return blobClient.Uri.ToString();
        }
    }
}