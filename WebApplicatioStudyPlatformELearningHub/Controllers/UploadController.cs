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
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize(Roles = "Teacher")]
[Authorize(Policy = "ConfirmedTeacher")]
public class UploadController : Controller
{

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<UploadController> _logger;

    public UploadController(ApplicationDbContext context, IWebHostEnvironment environment, UserManager<IdentityUser> userManager, ILogger<UploadController> logger)
    {
        _context = context;
        _environment = environment;
        _userManager = userManager;
        _logger = logger;
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
            .OrderByDescending(v => v.UploadDateTime) 
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

    [HttpGet]
    public async Task<IActionResult> Edit(int id,  int? deletedQuestionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        var creatorFullName = user?.UserName;

        ViewBag.Courses = await _context.Courses
                                   .Where(c => c.CreatorFullName == creatorFullName)
                                   .ToListAsync();

        var videoFile = await _context.VideoFiles
            .Include(v => v.Questions)
            .ThenInclude(q => q.Answers) 
            .FirstOrDefaultAsync(v => v.VideoId == id);

        //if (deletedQuestionId.HasValue)
        //{
        //    var questionToDelete = await _context.Questions.FindAsync(deletedQuestionId.Value);
        //    if (questionToDelete != null)
        //    {
        //        _context.Questions.Remove(questionToDelete);
        //        await _context.SaveChangesAsync();
        //    }
        //}
        if (videoFile == null)
        {
            return NotFound();
        }

       
        ViewBag.Categories = await _context.Categories.ToListAsync();
        ViewBag.CreatorFullName = creatorFullName;
        return View(videoFile);

    }


    // POST: Upload/Edit/5
    //[HttpPost]
    //public async Task<IActionResult> Edit(int id, VideoFile videoFile)
    //{
    //    if (id != videoFile.VideoId)
    //    {
    //        return NotFound();
    //    }

    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //    var user = await _userManager.FindByIdAsync(userId);
    //    var creatorFullName = user?.UserName;

    //    if (!ModelState.IsValid)
    //    {
    //        ViewBag.Courses = await _context.Courses
    //                                   .Where(c => c.CreatorFullName == creatorFullName)
    //                                   .ToListAsync();
    //        ViewBag.Categories = await _context.Categories.ToListAsync();
    //        ViewBag.CreatorFullName = creatorFullName;
    //        return View(videoFile);
    //    }
    //    var existingVideo = await _context.VideoFiles
    //     .Include(v => v.Questions)
    //     .ThenInclude(q => q.Answers)
    //     .FirstOrDefaultAsync(v => v.VideoId == id);

    //    if (existingVideo == null)
    //    {
    //        return NotFound();
    //    }
    //    // Update the properties you want to allow editing for
    //    existingVideo.Name = videoFile.Name;
    //    existingVideo.Description = videoFile.Description;
    //    existingVideo.Difficulty = videoFile.Difficulty;
    //    existingVideo.CategoryId = videoFile.CategoryId;
    //    existingVideo.CourseId = videoFile.CourseId;
    //    if (videoFile.Questions != null)
    //    {
    //        foreach (var question in videoFile.Questions)
    //        {
    //            var existingQuestion = existingVideo.Questions
    //                .FirstOrDefault(q => q.Id == question.Id);

    //            if (existingQuestion != null)
    //            {
    //                existingQuestion.Text = question.Text;
    //                // Update or add answers
    //                UpdateAnswers(existingQuestion, question.Answers);
    //            }
    //            else
    //            {
    //                // Add new question
    //                existingVideo.Questions.Add(new Question
    //                {
    //                    Text = question.Text,
    //                    Answers = question.Answers.Select(a => new Answer
    //                    {
    //                        Text = a.Text,
    //                        IsCorrect = a.IsCorrect
    //                    }).ToList()
    //                });
    //            }
    //        }

    //        // Remove questions not in updated model
    //        RemoveUnwantedQuestions(existingVideo, videoFile.Questions);
    //    }

    //    try
    //    {
    //        await _context.SaveChangesAsync();
    //        TempData["Message"] = "Video updated successfully!";
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError($"Error updating video: {ex}");
    //        // Handle exception
    //    }

    //    return RedirectToAction(nameof(AllVideoListDisplay));
    //}
    //private void UpdateAnswers(Question existingQuestion, ICollection<Answer> newAnswers)
    //{
    //    if (newAnswers != null)
    //    {
    //        foreach (var answer in newAnswers)
    //        {
    //            var existingAnswer = existingQuestion.Answers
    //                .FirstOrDefault(a => a.Id == answer.Id);

    //            if (existingAnswer != null)
    //            {
    //                existingAnswer.Text = answer.Text;
    //                existingAnswer.IsCorrect = answer.IsCorrect;
    //            }
    //            else
    //            {
    //                existingQuestion.Answers.Add(new Answer
    //                {
    //                    Text = answer.Text,
    //                    IsCorrect = answer.IsCorrect
    //                });
    //            }
    //        }

    //        // Remove answers not in the updated model
    //        var answersToRemove = existingQuestion.Answers
    //            .Where(a => !newAnswers.Any(ans => ans.Id == a.Id))
    //            .ToList();

    //        foreach (var answerToRemove in answersToRemove)
    //        {
    //            _context.Answers.Remove(answerToRemove);
    //        }
    //    }
    //}
    //private void RemoveUnwantedQuestions(VideoFile existingVideo, ICollection<Question> newQuestions)
    //{
    //    var questionsToRemove = existingVideo.Questions
    //        .Where(q => !newQuestions.Any(nq => nq.Id == q.Id))
    //        .ToList();

    //    foreach (var questionToRemove in questionsToRemove)
    //    {
    //        _context.Questions.Remove(questionToRemove);
    //    }
    //}
    [HttpPost]
    public async Task<IActionResult> Edit(int id, VideoFile videoFile)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var creatorFullName = user?.UserName;

        if (id != videoFile.VideoId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Courses = await _context.Courses
                                       .Where(c => c.CreatorFullName == creatorFullName)
                                       .ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.CreatorFullName = creatorFullName;
            return View(videoFile);
        }

        var existingVideo = await _context.VideoFiles
            .Include(v => v.Questions)
                .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(v => v.VideoId == id);

        if (existingVideo == null)
        {
            return NotFound();
        }

        existingVideo.Name = videoFile.Name;
        existingVideo.Description = videoFile.Description;
        existingVideo.Difficulty = videoFile.Difficulty;
        existingVideo.CategoryId = videoFile.CategoryId;
        existingVideo.CourseId = videoFile.CourseId;

        UpdateQuestionsAndAnswers(existingVideo, videoFile);

        try
        {
            await _context.SaveChangesAsync();
            TempData["Message"] = "Video updated successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating video: {ex}");
            ModelState.AddModelError("", "An error occurred while updating the video.");
            return View(videoFile);
        }

        return RedirectToAction(nameof(AllVideoListDisplay));
    }



    private void UpdateQuestionsAndAnswers(VideoFile existingVideo, VideoFile videoFile)
    {
        // Check for null
        if (videoFile.Questions == null) return;

        // Track the IDs of new/updated questions to identify deletions later
        var updatedQuestionIds = new HashSet<int>(videoFile.Questions.Select(q => q.Id));

        // Update and Add New Questions and Answers
        foreach (var question in videoFile.Questions)
        {
            var existingQuestion = existingVideo.Questions.FirstOrDefault(q => q.Id == question.Id);

            if (existingQuestion != null)
            {
                // Update existing question
                existingQuestion.Text = question.Text;
                UpdateAnswers(existingQuestion, question);
            }
            else
            {
                // Add new question
                existingVideo.Questions.Add(question);
            }
        }

        // Remove Questions and their Answers that were deleted
        foreach (var existingQuestion in existingVideo.Questions.ToList())
        {
            if (!updatedQuestionIds.Contains(existingQuestion.Id))
            {
                _context.Questions.Remove(existingQuestion);
            }
        }
    }

    private void UpdateAnswers(Question existingQuestion, Question updatedQuestion)
    {
        // Check for null
        if (updatedQuestion.Answers == null) return;

        // Track the IDs of new/updated answers to identify deletions later
        var updatedAnswerIds = new HashSet<int>(updatedQuestion.Answers.Select(a => a.Id));

        // Update and Add New Answers
        foreach (var answer in updatedQuestion.Answers)
        {
            var existingAnswer = existingQuestion.Answers.FirstOrDefault(a => a.Id == answer.Id);

            if (existingAnswer != null)
            {
                // Update existing answer
                existingAnswer.Text = answer.Text;
                existingAnswer.IsCorrect = answer.IsCorrect;
            }
            else
            {
                // Add new answer
                existingQuestion.Answers.Add(answer);
            }
        }

        // Remove Answers that were deleted
        foreach (var existingAnswer in existingQuestion.Answers.ToList())
        {
            if (!updatedAnswerIds.Contains(existingAnswer.Id))
            {
                existingQuestion.Answers.Remove(existingAnswer);
                _context.Answers.Remove(existingAnswer);
            }
        }
    }
    //[HttpPost]
    //public async Task<IActionResult> DeleteQuestion(int questionId)
    //{
    //    var question = await _context.Questions.FindAsync(questionId);
    //    if (question != null)
    //    {
    //        _context.Questions.Remove(question);
    //        await _context.SaveChangesAsync();
    //        return Json(new { success = true });
    //    }
    //    return Json(new { success = false });
    //}





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






