using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyPlatformELearningHub.Data;
using StudyPlatformELearningHub.Models.Statistic;
using System.Drawing.Printing;

namespace StudyPlatformELearningHub.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 6;
            var totalVideos = await _context.VideoFiles.CountAsync();
            var totalPages = (int)Math.Ceiling(totalVideos / (double)pageSize);
            var videoStats = await _context.VideoFiles
                .Include(v => v.Ratings)
                .Include(v => v.UserVideoViews)
                .Include(v => v.Category)
                .Include(v => v.Course)
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
