namespace StudyPlatformELearningHub.Models.Statistic
{
    public class VideoStatisticsViewModel
    {
        public VideoFile Video { get; set; }
        public int ViewCount { get; set; }
        public double AverageRating { get; set; }
        public string CategoryName { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public string CreatorFullName { get; set; }
    }
}
