using Microsoft.AspNetCore.Identity;

namespace StudyPlatformELearningHub.Models
{
    public class VideoRating
    {
        public int Id { get; set; }
        public int VideoId { get; set; }
        public string UserId { get; set; }
        public int Rating { get; set; } // 1 to 5

        public VideoFile Video { get; set; }
        public IdentityUser User { get; set; }
    }
}
