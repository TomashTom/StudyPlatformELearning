namespace StudyPlatformELearningHub.Models
{
    public class UserVideoView
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int VideoId { get; set; }
        public DateTime ViewDate { get; set; }

        public ApplicationUser User { get; set; }
        public VideoFile Video { get; set; }
    }
}
