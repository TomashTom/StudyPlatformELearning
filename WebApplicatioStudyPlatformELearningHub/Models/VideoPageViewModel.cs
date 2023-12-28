namespace StudyPlatformELearningHub.Models
{
    public class VideoPageViewModel
    {
        public VideoFilterModel FilterModel { get; set; }
        public IEnumerable<VideoViewModel> Videos { get; set; }
    }
}
