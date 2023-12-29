using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyPlatformELearningHub.Data;
using StudyPlatformELearningHub.Models;
using System.Diagnostics;

namespace StudyPlatformELearningHub.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class SeeLaterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SeeLaterController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var savedVideos = await _context.SeeLaterVideos
                .Where(sv => sv.User.Id == userId)
                .Include(sv => sv.Video)
                .ToListAsync();

            return View(savedVideos);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToSeeLater(int videoId, string note, int? playlistId, string newPlaylistName)
        {
            var userId = _userManager.GetUserId(User);
            Playlist playlist = null;

            if (!string.IsNullOrEmpty(newPlaylistName))
            {
                playlist = new Playlist { Name = newPlaylistName, UserId = userId, VideoId = videoId };
                _context.Playlists.Add(playlist);
                await _context.SaveChangesAsync();
            }
            else if (playlistId.HasValue)
            {
                playlist = await _context.Playlists.FindAsync(playlistId.Value);
                playlist.VideoId = videoId;
            }

            var seeLaterVideo = new SeeLaterVideo
            {
                VideoId = videoId,
                UserId = userId,
                Note = note,
                PlaylistId = playlist?.PlaylistId
            };
            _context.SeeLaterVideos.Add(seeLaterVideo);

            if (playlist != null)
            {
                playlist = await _context.Playlists
                    .Include(p => p.Videos)
                    .FirstOrDefaultAsync(p => p.PlaylistId == playlist.PlaylistId);

                if (playlist != null)
                {
                    playlist.Videos.Add(seeLaterVideo);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Play", "User", new { id = videoId });
        }
        public async Task<IActionResult> RemoveFromSeeLater(int videoId)
        {
            var userId = _userManager.GetUserId(User);

            // Find the saved video entry
            var seeLaterVideo = await _context.SeeLaterVideos
                .FirstOrDefaultAsync(sv => sv.User.Id == userId && sv.Video.VideoId == videoId);

            if (seeLaterVideo != null)
            {
                // Remove the video from the "See Later" list
                _context.SeeLaterVideos.Remove(seeLaterVideo);
                await _context.SaveChangesAsync();
            }

            
            return RedirectToAction("Index", "UserProfiles");
        }
        public async Task<IActionResult> DeletePlaylist(int playlistId)
        {
            // Find the playlist to delete
            var playlist = await _context.Playlists
                .Include(p => p.Videos) // Include the related videos
                .FirstOrDefaultAsync(p => p.PlaylistId == playlistId);

            if (playlist != null)
            {
                // Remove the associated videos
                _context.SeeLaterVideos.RemoveRange(playlist.Videos);

                // Remove the playlist
                _context.Playlists.Remove(playlist);
                await _context.SaveChangesAsync();
            }

            TempData["DeletedMessage"] = "Playlist and associated videos deleted successfully.";

            // Redirect to the user's profile or any other appropriate action
            return RedirectToAction("Index", "UserProfiles");
        }

    }
    
}
