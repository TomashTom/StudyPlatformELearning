namespace StudyPlatformELearningHub.Models
{
    public class VideoFilterModel
    {
        public List<int> CategoryIds { get; set; } = new List<int>();
        public List<string> CreatorNames { get; set; } = new List<string>();
        public List<string> DifficultyLevels { get; set; } = new List<string>();
        public string DateRange { get; set; }
        public double? RatingFilter { get; set; }
    }
}
