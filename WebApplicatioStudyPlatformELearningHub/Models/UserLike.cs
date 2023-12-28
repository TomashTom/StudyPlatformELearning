namespace StudyPlatformELearningHub.Models
{
    public class UserLike
    {
        public string UserId { get; set; }
        public int CommentId { get; set; }
        public Comment Comment { get; set; }

    }
}
