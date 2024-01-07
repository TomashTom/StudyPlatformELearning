using Microsoft.AspNetCore.Mvc;
using StudyPlatformELearningHub.Data;
using StudyPlatformELearningHub.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace StudyPlatformELearningHub.Areas.User.Controllers
{
    [Area("User")]
    public class VideoController : Controller
    {
        private readonly ApplicationDbContext _context;
        

        public VideoController(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task<IActionResult> Index(int? categoryId, string dateRangeFilter, string search,
                                       List<string> names, List<string> creators,
                                       List<string> difficultyLevels, List<int> filterStars, int pageIndex = 1, int pageSize = 6)
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.CreatorFullNames = await _context.VideoFiles.Select(v => v.CreatorFullName).Distinct().ToListAsync();

            var videosQuery = _context.VideoFiles.Include(v => v.Category).AsQueryable();

            // Apply Category Filter
            if (categoryId.HasValue && categoryId > 0)
            {
                videosQuery = videosQuery.Where(v => v.CategoryId == categoryId.Value);
            }
            // Apply Time Range Filter
            //if (!string.IsNullOrEmpty(timeRange))
            //{
            //    DateTime cutoffDate = DateTime.UtcNow.AddDays(-int.Parse(timeRange));
            //    videosQuery = videosQuery.Where(v => v.UploadDateTime >= cutoffDate);
            //}
            // Apply Time Range Filter
            // Apply Time Range Filter
            if (!string.IsNullOrEmpty(dateRangeFilter))
            {
                // Parse the selected date range to an integer
                if (int.TryParse(dateRangeFilter, out int dateRangeValue))
                {
                    DateTime currentDate = DateTime.UtcNow;
                    DateTime lowerBound = currentDate;
                    DateTime upperBound = currentDate;

                    switch (dateRangeValue)
                    {
                        case 7:
                            lowerBound = currentDate.AddDays(-7);
                            break;
                        case 30:
                            // For the last 30 days, excluding the most recent 7 days
                            lowerBound = currentDate.AddDays(-30);
                            upperBound = currentDate.AddDays(-7);
                            break;
                        case 60:
                            // For the last 60 days, excluding the most recent 30 days
                            lowerBound = currentDate.AddDays(-60);
                            upperBound = currentDate.AddDays(-30);
                            break;
                            // Add more cases for additional date range options if needed
                    }

                    videosQuery = videosQuery.Where(v => v.UploadDateTime >= lowerBound && v.UploadDateTime < upperBound);
                }
            }




            // Apply Search Filter
            if (!string.IsNullOrEmpty(search))
            {
                videosQuery = videosQuery.Where(v => v.Name.Contains(search) || v.Description.Contains(search));
            }

            // Apply Names Filter
            if (names != null && names.Count > 0)
            {
                videosQuery = videosQuery.Where(v => names.Contains(v.Name));
            }

            // Apply Creators Filter
            if (creators != null && creators.Count > 0)
            {
                videosQuery = videosQuery.Where(v => creators.Contains(v.CreatorFullName));
            }

            // Apply Difficulty Levels Filter
            if (difficultyLevels != null && difficultyLevels.Count > 0)
            {
                var parsedDifficulties = difficultyLevels.Select(dl => Enum.Parse<VideoDifficulty>(dl, true)).ToList();
                videosQuery = videosQuery.Where(v => parsedDifficulties.Contains(v.Difficulty));
            }

            // Apply Rating Filter
            if (filterStars.Count > 0)
            {
                
                int rating = filterStars.First();
                videosQuery = videosQuery.Where(v => v.Ratings.Any() && Math.Ceiling(v.Ratings.Average(r => r.Rating)) == rating);
            }
           
            int totalVideos = await videosQuery.CountAsync();
            var videos = await videosQuery.Skip((pageIndex - 1) * pageSize)
                                  .Take(pageSize)
                                  .Select(v => new VideoViewModel
                                  {
                                      Video = v,
                                      AverageRating = v.Ratings.Any() ? v.Ratings.Average(r =>                r.Rating) : 0,
                                      ViewCount = v.UserVideoViews.Count
                                  })
                                  .AsQueryable()
                                  .ToListAsync();
            ViewBag.CurrentFilterCategoryId = categoryId;
            ViewBag.CurrentFilterTimeRange = dateRangeFilter;
            ViewBag.CurrentFilterSearch = search;
            ViewBag.CurrentFilterNames = names;
            ViewBag.CurrentFilterCreators = creators;
            ViewBag.CurrentFilterDifficultyLevels = difficultyLevels;
            ViewBag.CurrentFilterStars = filterStars;
            ViewBag.TotalVideos = totalVideos;
            ViewBag.PageIndex = pageIndex;
            ViewBag.PageSize = pageSize;
            return View(videos);
        }

        [HttpPost]
        public IActionResult Index(VideoFilterModel filterModel)
        {
            TempData["SelectedCategoryIds"] = filterModel.CategoryIds;
            TempData["SelectedCreatorNames"] = filterModel.CreatorNames;
            TempData["SelectedDifficultyLevels"] = filterModel.DifficultyLevels;
            TempData["SelectedDateRange"] = filterModel.DateRange;
            TempData["SelectedRatingFilter"] = filterModel.RatingFilter;

            return RedirectToAction(nameof(Index));
        }





        public async Task<IActionResult> Play(int id, int? CourseId)
        {
            
            Console.WriteLine($"AllComments: {ViewBag.AllComments}");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userNickname = await GetUserNicknameAsync(userId);
           

            ViewData["Nickname"] = userNickname ?? "DefaultNickname";

            var currentVideo = await _context.VideoFiles
                    .FirstOrDefaultAsync(v => v.VideoId == id);


            if (currentVideo == null)
            {
                return NotFound($"Video with ID {id} not found.");
            }

            if (!User.Identity.IsAuthenticated)
            {
                // Create the return URL for after successful login
                string returnUrl = Url.Action("Play", "Video", new { area = "User", id = id });

                // Redirect to the login page in the 'Identity' area
                return RedirectToPage("/Account/Login", new { area = "Identity", ReturnUrl = returnUrl });
            }
          
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var video = await _context.VideoFiles
                     .Include(v => v.Category)
                     .Include(v => v.Ratings)
                     .Include(v => v.UserVideoViews)
                     .Include(v => v.SeeLaterVideos)
                     .FirstOrDefaultAsync(v => v.VideoId == id);

            if(video == null)
{
                return NotFound($"Video with ID {id} not found.");
            }

           
            // Check if the user has already viewed this video
            var hasViewed = _context.UserVideoViews.Any(vv => vv.UserId == userId && vv.VideoId == id);

            if (!hasViewed)
            {
                // User hasn't viewed this video before, so record this new view
                var userVideoView = new UserVideoView
                {
                    UserId = userId,
                    VideoId = id,
                    ViewDate = DateTime.UtcNow
                };
                _context.UserVideoViews.Add(userVideoView);

                await _context.SaveChangesAsync();
            }

            // Check if the user has already rated the video

            var userRating = video.Ratings.FirstOrDefault(r => r.UserId == userId);
            var isVideoSaved = await _context.SeeLaterVideos
                .AnyAsync(sv => sv.VideoId == id && sv.UserId == userId);
            var recommendedVideos = await GetRecommendedVideos(video.CategoryId, video.CreatorFullName, currentVideo.VideoId);
          


            var questions = await _context.Questions
                                  .Where(q => q.VideoId == id)
                                  .Include(q => q.Answers)
                                  .ToListAsync();
            var comments = await _context.Comments
                .Where(c => c.VideoId == id && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Include(c => c.Replies)
                .ToListAsync();
            foreach (var comment in comments)
            {
                comment.Replies = await _context.Comments
                    .Where(c => c.ParentCommentId == comment.CommentId) // Get replies for this comment
                    .ToListAsync();

                string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                comment.UserLiked = _context.UserLikes.Any(ul => ul.UserId == currentUserId && ul.CommentId == comment.CommentId);
                comment.UserHearted = _context.UserHearts.Any(uh => uh.UserId == currentUserId && uh.CommentId == comment.CommentId);
                comment.LikeCount = _context.UserLikes.Count(ul => ul.CommentId == comment.CommentId);
                comment.HeartCount = _context.UserHearts.Count(ul => ul.CommentId == comment.CommentId);

            }
            //
            var coursesWithVideos = _context.Courses
              .Where(p => p.CourseId == video.CourseId)
              .Include(c => c.Videos)
              .ToList();
            


            var courseViewModels = coursesWithVideos.Select(course => new CourseViewModel
            {
                Course = course,
                Videos = course.Videos.Where(video => video.CourseId != null).ToList()
            });
          

            //
            var viewModel = new VideoPlayViewModel
            {
                Video = video,
                UserRating = userRating?.Rating,
                IsVideoSaved = isVideoSaved,
                Playlists = await _context.Playlists
                        .Where(p => p.UserId == userId)
                        .ToListAsync(),
                RecommendedVideos = await GetRecommendedVideos(video.CategoryId, video.CreatorFullName, currentVideo.VideoId) ?? new List<VideoFile>(),
                Questions = await _context.Questions
                      .Where(q => q.VideoId == id)
                      .Include(q => q.Answers)
                      .ToListAsync(),
                Comments = comments,
                Videos = recommendedVideos,
                courseViewModels = courseViewModels
               
            };

            viewModel.Context = _context;
            return View("Play", viewModel);
        }
        private async Task<string> GetUserNicknameAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.UserName; 
        }
        [HttpPost]
        public async Task<IActionResult> AddComment(int videoId, string content, string nickname)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var comment = new Comment
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                VideoId = videoId,
                Content = content,
                Nickname = nickname
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("Nickname", nickname);
            return RedirectToAction("Play", new { id = videoId });
        }
      
        


        [HttpPost]
        [Area("User")]
        public async Task<IActionResult> CheckAnswer(int questionId, List<int> answerIds, int videoId)
        {
            // Get the correct answers for the question
            var correctAnswers = await _context.Answers
                                               .Where(a => a.QuestionId == questionId && a.IsCorrect)
                                               .ToListAsync();

            // Determine if the user's answers are correct
            bool isCorrect = !answerIds.Except(correctAnswers.Select(a => a.Id)).Any() &&
                             !correctAnswers.Select(a => a.Id).Except(answerIds).Any();

            // If the answer is incorrect, fetch the incorrect answer message
            if (!isCorrect)
            {
                var incorrectAnswer = await _context.Answers
                                                     .Where(a => answerIds.Contains(a.Id) && !a.IsCorrect)
                                                     .FirstOrDefaultAsync();

                TempData["IncorrectAnswerMessage"] = incorrectAnswer?.IncorrectMessage ?? "Incorrect answer. No specific feedback provided.";
            }

            // Store results in TempData and redirect
            TempData["IsCorrect"] = isCorrect;
            TempData["CheckedAnswers"] = answerIds.ToArray();
            TempData["LastCheckedQuestionId"] = questionId;

            return RedirectToAction("Play", new { id = videoId });
        }




        [HttpPost]
        public IActionResult FilterVideos(List<int> categoryIds)
        {
            var filteredVideos = _context.VideoFiles
                .Include(v => v.Category)
                .Where(v => categoryIds.Contains(v.CategoryId))
                .ToList();

            var distinctNames = filteredVideos.Select(v => v.Name).Distinct().ToList();

            return Json(new { data = distinctNames });
        }
        [HttpPost]
        public async Task<IActionResult> RateVideo(int videoId, int rating)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingRating = await _context.VideoRatings
                .FirstOrDefaultAsync(vr => vr.VideoId == videoId && vr.UserId == userId);

            if (existingRating != null)
            {
                //existingRating.Rating = rating;
                return View("AlreadyRatedView");
            }
            else
            {
                var newRating = new VideoRating { VideoId = videoId, UserId = userId, Rating = rating };
                _context.VideoRatings.Add(newRating);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Play", new { id = videoId });
        }
        public async Task<IActionResult> EditComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();
            return View(comment);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(int commentId, string content)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var comment = await _context.Comments.FindAsync(commentId);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (comment == null || comment.UserId != userId)
            {
                return NotFound();
            }

            comment.Content = content;
            await _context.SaveChangesAsync();

            // Redirect back to the original page
            // Assuming that the video ID is available, otherwise you need to pass it in as a parameter
            return RedirectToAction("Play", new { id = comment.VideoId });
        }





        [HttpPost]
        public async Task<IActionResult> AddCommentReply(int videoId, string content, int? parentCommentId, string nickname)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Explicit check for ParentCommentId to ensure it's not null
            if (!parentCommentId.HasValue)
            {
                // If ParentCommentId is null, return an error view or handle accordingly
                // Do NOT create a top-level comment here as it should be a reply
                return View("Error", new ErrorViewModel { RequestId = "ParentCommentId is required for a reply." });
            }

            var reply = new Comment
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                VideoId = videoId,
                Content = content,
                ParentCommentId = parentCommentId, // Ensure this is set for replies
                Nickname = nickname,
                CreatedAt = DateTime.UtcNow // Set the created time for the reply
            };

            _context.Comments.Add(reply);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("Nickname", nickname);
            return RedirectToAction("Play", new { id = videoId });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (comment == null || comment.UserId != userId)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Play", new { id = comment.VideoId });
        }



        
        [HttpPost]
        public async Task<IActionResult> LikeComment(int commentId, int videoId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = await _context.Comments
                           .Include(c => c.Likes)
                           .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null)
            {
                return NotFound();
            }

            var userLike = comment.Likes.FirstOrDefault(l => l.UserId == userId);
            if (userLike != null)
            {
                // User has already liked the comment, so unlike it
                comment.Likes.Remove(userLike);
                comment.UserLiked = false; // Update UserLiked property
            }
            else
            {
                // User hasn't liked the comment yet, so add a like
                comment.Likes.Add(new UserLike { UserId = userId, CommentId = commentId });
                comment.UserLiked = true; // Update UserLiked property
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Play", new { id = videoId });
        }

        [HttpPost]
        public async Task<IActionResult> HeartComment(int commentId, int videoId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = await _context.Comments
                           .Include(c => c.Hearts)
                           .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null)
            {
                return NotFound();
            }

            var userHeart = comment.Hearts.FirstOrDefault(h => h.UserId == userId);
            if (userHeart != null)
            {
                // User has already hearted the comment, so unheart it
                comment.Hearts.Remove(userHeart);
                comment.UserHearted = false; // Update UserHearted property
            }
            else
            {
                // User hasn't hearted the comment yet, so add a heart
                comment.Hearts.Add(new UserHeart { UserId = userId, CommentId = commentId });
                comment.UserHearted = true; // Update UserHearted property
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Play", new { id = videoId });
        }

        [HttpPost]
        public async Task<IActionResult> UnlikeComment(int commentId, int videoId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = await _context.Comments
                           .Include(c => c.Likes)
                           .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null)
            {
                return NotFound();
            }

            var userLike = comment.Likes.FirstOrDefault(l => l.UserId == userId);
            if (userLike != null)
            {
                // User has already liked the comment, so remove the like
                comment.Likes.Remove(userLike);
                comment.UserLiked = false; // Update UserLiked property

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Play", new { id = videoId });
        }

        [HttpPost]
        public async Task<IActionResult> UnheartComment(int commentId, int videoId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = await _context.Comments
                           .Include(c => c.Hearts)
                           .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null)
            {
                return NotFound();
            }

            var userHeart = comment.Hearts.FirstOrDefault(h => h.UserId == userId);
            if (userHeart != null)
            {
                comment.Hearts.Remove(userHeart);
                comment.UserHearted = false; // Update UserHearted property

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Play", new { id = videoId });
        }
       
        private async Task<List<VideoFile>> GetRecommendedVideos(int categoryId, string creatorFullName, int currentVideoId)
        {
            return await _context.VideoFiles
                                 .Where(v => v.CategoryId == categoryId
                                             && v.CreatorFullName == creatorFullName
                                             && v.VideoId != currentVideoId) // Exclude the current video
                                 .OrderByDescending(v => v.UploadDateTime) // Sort by newest
                                 .ToListAsync();
        }
        














    }

}
