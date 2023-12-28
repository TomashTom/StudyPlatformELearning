using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyPlatformELearningHub.Data;
using StudyPlatformELearningHub.Models;
using StudyPlatformELearningHub.Models.Statistic;
using System.Security.Claims;

namespace StudyPlatformELearningHub.Controllers.UserStatistic
{
    public class UserStatisticController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public UserStatisticController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 6;
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var totalCount = await _context.VideoFiles.CountAsync(v => v.UserId == currentUserId);

            // Calculate total pages
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            

            var videoStats = await _context.VideoFiles
                .Include(v => v.Ratings)
                .Include(v => v.UserVideoViews)
                .Include(v => v.Category)
                .Include(v => v.Course)
                .Where(v => v.UserId == currentUserId) // Filter by the current user's ID
                .Select(v => new VideoStatisticsViewModel
                {
                    Video = v,
                    ViewCount = v.UserVideoViews.Count,
                    AverageRating = v.Ratings.Any() ? v.Ratings.Average(r => r.Rating) : 0,
                    CategoryName = v.Category.Name,
                    CourseName = v.Course != null ? v.Course.Name : "No Course",
                    CreatorFullName = v.CreatorFullName
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            return View(videoStats);
        }




    }
}
