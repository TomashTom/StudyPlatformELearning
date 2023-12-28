using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyPlatformELearningHub.Models
{
    public class Comment
    {

        [Key]
        public int CommentId { get; set; }


        [ForeignKey("User")]
        public string UserId { get; set; }

        [ForeignKey("Video")]
        public int VideoId { get; set; }
        [Required]
        [StringLength(maximumLength: 500, ErrorMessage = "Comment is too long")]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Navigation properties
        public VideoFile Video { get; set; }
        public string Nickname { get; set; }

        public virtual Comment ParentComment { get; set; }

       
        public int? ParentCommentId { get; set; }
        public virtual ICollection<Comment> Replies { get; set; }

        public ICollection<UserLike> Likes { get; set; }
        public ICollection<UserHeart> Hearts { get; set; }

        [NotMapped]
        public bool UserLiked { get; set; }
        [NotMapped]
        public bool UserHearted { get; set; }
        [NotMapped]
        public int LikeCount { get; set; }

        [NotMapped]
        public int HeartCount { get; set; }



    }
}
