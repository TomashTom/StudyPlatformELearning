using Microsoft.AspNetCore.Mvc;
using StudyPlatformELearningHub.Data;
using StudyPlatformELearningHub.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using FoolProof;



[Authorize(Roles = "Teacher")]
[Authorize(Policy = "ConfirmedTeacher")]
public class UploadController : Controller
{

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly UserManager<IdentityUser> _userManager;

    public UploadController(ApplicationDbContext context, IWebHostEnvironment environment, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _environment = environment;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();


        // Get the current user's full name
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        var creatorFullName = user?.UserName; 
        ViewBag.Courses = await _context.Courses
                                   .Where(c => c.CreatorFullName == creatorFullName)
                                   .ToListAsync();


        ViewBag.CreatorFullName = creatorFullName;

        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Index(VideoFile videoFile, IFormFile fileUpload, IFormFile thumbnailImage, int? courseId)
    {
        if (fileUpload != null && fileUpload.Length > 0)
        {
            string uploadFolder = Path.Combine(_environment.WebRootPath, "videoFiles");
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileUpload.FileName);
            string filePath = Path.Combine(uploadFolder, uniqueFileName);
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            videoFile.UserId = userId;
            videoFile.CourseId = videoFile.CourseId;
            videoFile.Status = videoFile.Status;


            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await fileUpload.CopyToAsync(fileStream);
            }
            videoFile.UserId = userId;
            videoFile.CourseId = videoFile.CourseId;
            videoFile.Name = videoFile.Name;
            videoFile.VideoName = uniqueFileName;
            videoFile.VideoPath = Path.Combine("videoFiles", uniqueFileName);
            videoFile.UploadDateTime = DateTime.UtcNow;
            videoFile.Difficulty = videoFile.Difficulty;
            videoFile.Description = videoFile.Description;
            if (thumbnailImage != null && thumbnailImage.Length > 0)
            {
                string thumbnailFolder = Path.Combine(_environment.WebRootPath, "videoThumbnails");
                string thumbnailFileName = Guid.NewGuid().ToString() + Path.GetExtension(thumbnailImage.FileName);
                string thumbnailFilePath = Path.Combine(thumbnailFolder, thumbnailFileName);

                if (!Directory.Exists(thumbnailFolder))
                {
                    Directory.CreateDirectory(thumbnailFolder);
                }

                using (var fileStream = new FileStream(thumbnailFilePath, FileMode.Create))
                {
                    await thumbnailImage.CopyToAsync(fileStream);
                }

                videoFile.ThumbnailPath = Path.Combine("videoThumbnails", thumbnailFileName);
            }

            _context.VideoFiles.Add(videoFile);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.AddClaimAsync(user, new Claim("Confirmed", "true"));
            }


            return RedirectToAction("Success", new { id = videoFile.VideoId });
        }
        else
        {

            ModelState.AddModelError("", "You must select a video file.");
        }





        return View(videoFile);
    }




    public IActionResult Success(int id)
    {
        var videoFile = _context.VideoFiles
            .Include(v => v.Category)
            .FirstOrDefault(v => v.VideoId == id);
        if (videoFile == null)
        {
            return NotFound("Video not found.");
        }

        var questions = _context.Questions
                                .Where(q => q.VideoId == id)
                                .Include(q => q.Answers)
                                .ToList();

        var viewModel = new VideoPlaybackViewModel
        {
            Video = videoFile,
            Questions = questions
        };

        return View(viewModel);
    }

    //
    [Authorize]
    public IActionResult AllVideoListDisplay(int page = 1)
    {
        const int pageSize = 6;
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var userVideos = _context.VideoFiles
            .Where(v => v.UserId == userId)
            .Include(v => v.Category)
            .Include(v => v.Course)
            .OrderByDescending(v => v.UploadDateTime) // Order by upload date, for example
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Calculate total page count
        int totalVideos = _context.VideoFiles.Count(v => v.UserId == userId);
        int totalPages = (int)Math.Ceiling((double)totalVideos / pageSize);

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(userVideos);
    }




    ////

    [HttpPost]
    public async Task<IActionResult> DeleteVideo(int id)
    {
        var videoFile = await _context.VideoFiles.FindAsync(id);
        if (videoFile == null)
        {
            TempData["Error"] = "Video not found.";
            return RedirectToAction(nameof(AllVideoListDisplay));
        }

        try
        {
            _context.VideoFiles.Remove(videoFile);
            await _context.SaveChangesAsync();

            var filePath = Path.Combine(_environment.WebRootPath, "videoFiles", videoFile.VideoName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            TempData["Message"] = "Video deleted successfully!";
        }
        catch (Exception ex)
        {
            // Log the exception
            TempData["Error"] = $"An error occurred while deleting the video: {ex.Message}";
        }

        return RedirectToAction(nameof(AllVideoListDisplay));
    }

    // GET: Upload/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var videoFile = await _context.VideoFiles.FindAsync(id);
        if (videoFile == null)
        {
            return NotFound();
        }
        ViewBag.Courses = _context.Courses.ToList(); // Add this line
        return View(videoFile);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, VideoFile videoFile, IFormFile newVideoFile)
    {
        // Find the video record to edit
        var existingVideo = await _context.VideoFiles.FindAsync(id);

        if (existingVideo == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                }
            }

            ViewBag.Courses = await _context.Courses.ToListAsync();
            return View(videoFile);
        }

        // Update video properties based on user input
        existingVideo.Name = videoFile.Name;
        existingVideo.Description = videoFile.Description;
        existingVideo.Difficulty = videoFile.Difficulty;

        if (newVideoFile != null && newVideoFile.Length > 0)
        {
            // Handle the new video file upload (if provided)
            string uploadFolder = Path.Combine(_environment.WebRootPath, "videoFiles");
            string newVideoFileName = Guid.NewGuid().ToString() + Path.GetExtension(newVideoFile.FileName);
            string newVideoFilePath = Path.Combine(uploadFolder, newVideoFileName);

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            using (var fileStream = new FileStream(newVideoFilePath, FileMode.Create))
            {
                await newVideoFile.CopyToAsync(fileStream);
            }

            // Update the video file properties
            existingVideo.VideoName = newVideoFileName;
            existingVideo.VideoPath = Path.Combine("videoFiles", newVideoFileName);
        }

        // Save changes to the database
        await _context.SaveChangesAsync();

        TempData["Message"] = "Video updated successfully!";
        return RedirectToAction(nameof(AllVideoListDisplay));
    }





    private bool VideoFileExists(int id)
    {
        return _context.VideoFiles.Any(e => e.VideoId == id);
    }

    public async Task<IActionResult> TakeQuiz()
    {
        var questions = await _context.Questions.Include(q => q.Answers).ToListAsync();
        return View(questions);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitQuiz(int[] answers)
    {
        int score = 0;
        foreach (var answerId in answers)
        {
            var answer = await _context.Answers.FirstOrDefaultAsync(a => a.Id == answerId);
            if (answer != null && answer.IsCorrect)
            {
                score++;
            }
        }


        return RedirectToAction("QuizResults", new { score = score });
    }

    public async Task<IActionResult> Playback(int videoId)
    {

        var video = await _context.VideoFiles
        .FirstOrDefaultAsync(v => v.VideoId == videoId);
        if (video == null)
        {

            return NotFound($"Video with ID {videoId} not found.");
        }


        var questions = await _context.Questions
                                      .Where(q => q.VideoId == videoId)
                                      .Include(q => q.Answers)
                                      .ToListAsync();


        var viewModel = new VideoPlaybackViewModel
        {
            Video = video,
            Questions = questions
        };


        return View(viewModel);
    }
    [HttpPost]
    public async Task<IActionResult> SaveQuestions(QuizViewModel model)
    {
        if (ModelState.IsValid)
        {
            foreach (var question in model.Questions)
            {
                _context.Questions.Add(question);

            }
            await _context.SaveChangesAsync();
            return RedirectToAction("QuestionsSavedSuccessfully");
        }
        return View(model);
    }
    [HttpGet]
    public async Task<IActionResult> SearchCategories(string term)
    {
        var categories = await _context.Categories
            .Where(c => c.Name.Contains(term))
            .ToListAsync();

        return Json(categories.Select(c => new { id = c.CategoryId, text = c.Name, description = c.Description }));
    }
    [HttpPost]
    public IActionResult ChangeVideoStatus(int id, VideoStatus status)
    {
        var video = _context.VideoFiles.FirstOrDefault(v => v.VideoId == id);

        if (video != null)
        {
            video.Status = status;
            _context.SaveChanges();
            
        }

        return RedirectToAction("AllVideoListDisplay");
    }















}






