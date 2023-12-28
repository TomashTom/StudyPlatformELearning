namespace StudyPlatformELearningHub.Models
{
    public class UserHeart
    {
        public string UserId { get; set; }

        public int CommentId { get; set; }
        public Comment Comment { get; set; }
    }
}
