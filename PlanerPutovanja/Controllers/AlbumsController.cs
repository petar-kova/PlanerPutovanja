using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanerPutovanja.Models;

namespace PlanerPutovanja.Controllers
{
    [Authorize]
    public class AlbumsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AlbumsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        public async Task<IActionResult> Index()
        {
            var albums = await _context.TripAlbums
                .Include(a => a.Trip)
                .Include(a => a.Photos)
                .Where(a => a.Trip.UserId == CurrentUserId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(albums);
        }

        public async Task<IActionResult> Create(int tripId)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();

            var album = new TripAlbum
            {
                TripId = tripId,
                Title = $"Uspomene: {trip.Name}",
                Rating = 5
            };

            ViewBag.TripName = trip.Name;
            ViewBag.TripId = trip.Id;

            return View(album);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int tripId,
            TripAlbum album,
            List<IFormFile>? photos,
            List<string>? captions)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == CurrentUserId);

            if (trip == null) return NotFound();

            album.TripId = tripId;
            album.CreatedAt = DateTime.UtcNow;

            ModelState.Remove(nameof(TripAlbum.Trip));
            ModelState.Remove(nameof(TripAlbum.Photos));
            ModelState.Remove(nameof(TripAlbum.TripId));
            ModelState.Remove(nameof(TripAlbum.CoverImagePath));

            if (!ModelState.IsValid)
            {
                ViewBag.TripName = trip.Name;
                ViewBag.TripId = trip.Id;
                return View(album);
            }

            _context.TripAlbums.Add(album);
            await _context.SaveChangesAsync();

            if (photos != null && photos.Count > 0)
            {
                var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "trips", tripId.ToString());

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var displayOrder = 1;

                for (var i = 0; i < photos.Count; i++)
                {
                    var photo = photos[i];

                    if (photo == null || photo.Length == 0)
                    {
                        continue;
                    }

                    var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(extension))
                    {
                        continue;
                    }

                    if (photo.Length > 5 * 1024 * 1024)
                    {
                        continue;
                    }

                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadFolder, fileName);

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }

                    var imagePath = $"/uploads/trips/{tripId}/{fileName}";

                    var caption = captions != null && captions.Count > i
                        ? captions[i]
                        : null;

                    var tripPhoto = new TripPhoto
                    {
                        TripAlbumId = album.Id,
                        ImagePath = imagePath,
                        Caption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim(),
                        DisplayOrder = displayOrder,
                        UploadedAt = DateTime.UtcNow
                    };

                    _context.TripPhotos.Add(tripPhoto);

                    if (string.IsNullOrWhiteSpace(album.CoverImagePath))
                    {
                        album.CoverImagePath = imagePath;
                    }

                    displayOrder++;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = album.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var album = await _context.TripAlbums
                .Include(a => a.Trip)
                .Include(a => a.Photos.OrderBy(p => p.DisplayOrder))
                .FirstOrDefaultAsync(a => a.Id == id && a.Trip.UserId == CurrentUserId);

            if (album == null) return NotFound();

            return View(album);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var album = await _context.TripAlbums
                .Include(a => a.Trip)
                .Include(a => a.Photos)
                .FirstOrDefaultAsync(a => a.Id == id && a.Trip.UserId == CurrentUserId);

            if (album == null) return NotFound();

            var tripId = album.TripId;

            foreach (var photo in album.Photos)
            {
                DeletePhysicalFile(photo.ImagePath);
            }

            _context.TripAlbums.Remove(album);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var photo = await _context.TripPhotos
                .Include(p => p.TripAlbum)
                .ThenInclude(a => a.Trip)
                .FirstOrDefaultAsync(p => p.Id == id && p.TripAlbum.Trip.UserId == CurrentUserId);

            if (photo == null) return NotFound();

            var albumId = photo.TripAlbumId;

            DeletePhysicalFile(photo.ImagePath);

            _context.TripPhotos.Remove(photo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = albumId });
        }

        private void DeletePhysicalFile(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) return;

            var relativePath = imagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}