using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using StudyPlatformELearningHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StudyPlatformELearningHub.Data;
using Microsoft.EntityFrameworkCore;
using Xabe.FFmpeg;

namespace StudyPlatformELearningHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();

            var featuredVideos = GetRandomFeaturedVideos(4);
            _logger.LogInformation($"Number of featured videos: {featuredVideos?.Count}");
            var model = new HomeViewModel
            {
                RandomVideos = featuredVideos,
                IsTeacher = roles.Contains("Teacher"),
                IsAdmin = roles.Contains("Admin")
            };

            return View(model); 
        }
        private List<VideoFile> GetRandomFeaturedVideos(int count)
        {
            var featuredVideos = _context.VideoFiles
                .OrderBy(o => Guid.NewGuid()) // Shuffle the videos randomly
                .Take(count)
                .ToList();

            return featuredVideos;
        }









    }
}