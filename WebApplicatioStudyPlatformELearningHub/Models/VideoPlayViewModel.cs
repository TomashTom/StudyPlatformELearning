using StudyPlatformELearningHub.Data;

namespace StudyPlatformELearningHub.Models
{
    public class VideoPlayViewModel
    {
        public  IEnumerable<CourseViewModel> courseViewModels { get; set; }

        public VideoFile Video { get; set; }
        public int? UserRating { get; set; }
        public bool IsVideoSaved { get; set; }
        public List<Playlist> Playlists { get; set; }
        public List<VideoFile> RecommendedVideos { get; set; }
        public List<Question> Questions { get; set; }
        public string NewComment { get; set; }
        public List<Comment> Comments { get; set; }
        public List<VideoFile> Videos { get; set; }
        public List<CourseViewModel> RecommendedCourses { get; set; }
        public ApplicationDbContext Context { get; set; }








    }
}
