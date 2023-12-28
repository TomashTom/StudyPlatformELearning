using Xabe.FFmpeg;

namespace StudyPlatformELearningHub.Models
{
    public class HomeViewModel
    {
        public bool IsTeacher { get; set; }
        public bool IsAdmin { get; set; }
        public List<VideoFile> RandomVideos { get; set; }
    }
}
