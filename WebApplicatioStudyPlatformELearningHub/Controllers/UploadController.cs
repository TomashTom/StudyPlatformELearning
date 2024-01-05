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
    public async Task<IActionResult> Edit(int id, int? deletedQuestionId)
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

        
            await _context.SaveChangesAsync();
            TempData["Message"] = "Video updated successfully!";
       
        return RedirectToAction(nameof(AllVideoListDisplay));
    }


    // GET: Upload/EditQuestion/5
    public IActionResult EditQuestion(int id)
    {
        var question = _context.Questions.Include(q => q.Answers).FirstOrDefault(q => q.Id == id);
        if (question == null)
        {
            return NotFound();
        }
        return View(question); // Path to your view
    }

    // POST: Upload/EditQuestion/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(Question model)
    {
        ModelState.Remove("Video");
        ModelState.Remove("Answers");
        if (ModelState.IsValid)
        {
            // Your existing code for updating the question
            try
            {
                var questionToUpdate = await _context.Questions
              .FirstOrDefaultAsync(q => q.Id == model.Id);

                if (questionToUpdate == null)
                {
                    return NotFound();
                }

                questionToUpdate.Text = model.Text;


                _context.Update(questionToUpdate);
                await _context.SaveChangesAsync();

                // Assuming that the Question model has a foreign key property named VideoId
                return RedirectToAction("Edit", "Upload", new { id = questionToUpdate.VideoId });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(model.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
      
        return View(model);


    }

    private bool QuestionExists(int id)
    {
        return _context.Questions.Any(e => e.Id == id);
    }


    // GET: Upload/EditAnswer/5
    public IActionResult EditAnswer(int id)
    {
        var answer = _context.Answers
            .Include(a => a.Question)
            .FirstOrDefault(a => a.Id == id);
        if (answer == null)
        {
            return NotFound();
        }
        return View(answer); // Path to your view
    }

    // POST: Upload/EditAnswer/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAnswer(int id, Answer answer)
    {
        if (id != answer.Id)
        {
            return NotFound();
        }
        bool isNewAnswer = answer.Id == 0;
        if (!isNewAnswer)
        {
            ModelState.Remove("Question");
        }
        if (ModelState.IsValid)
        {
            try
            {
                // Fetch the existing answer from the database
                var existingAnswer = await _context.Answers.FindAsync(id);

                if (existingAnswer == null)
                {
                    return NotFound();
                }


                // Ensure that QuestionId is correctly set
                answer.QuestionId = existingAnswer.QuestionId;

                // Update only the properties you want to change
                existingAnswer.Text = answer.Text;
                existingAnswer.IsCorrect = answer.IsCorrect;
                existingAnswer.IncorrectMessage = answer.IncorrectMessage;

                // Save the changes
                await _context.SaveChangesAsync();

                // Redirect to the Edit Question page after updating the answer
                return RedirectToAction(nameof(EditQuestion), new { id = existingAnswer.QuestionId });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnswerExists(answer.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        foreach (var entry in ModelState.Values)
        {
            foreach (var error in entry.Errors)
            {
                // Log the error message or display it as needed
                var errorMessage = error.ErrorMessage;
                // You can log or display 'errorMessage' here
                // For example, you can log it to a file or display it in your view
                _logger.LogError(errorMessage);
                ViewBag.ErrorMessage = errorMessage;
            }
        }

        return View(answer);
    }

    private bool AnswerExists(int id)
    {
        return _context.Answers.Any(e => e.Id == id);
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






