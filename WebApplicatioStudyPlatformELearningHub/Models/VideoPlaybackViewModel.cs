namespace StudyPlatformELearningHub.Models
{
    public class VideoPlaybackViewModel
    {
        public VideoFile Video { get; set; }
        public List<Question> Questions { get; set; }
        public double AverageRating { get; set; }
    }
}
