using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyPlatformELearningHub.Models
{
    public class VideoFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VideoId { get; set; }
        public string? Name { get; set; }
        [Required]
        [Display(Name = "Video Name")]
        public string? VideoName { get; set; }
        public string? Description { get; set; }

        [Display(Name = "Creator's Full Name")]

        public string CreatorFullName { get; set; }
        public string? VideoPath { get; set; }

        [Display(Name = "Upload Date")]
        public DateTime UploadDateTime { get; set; }
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
        public ICollection<Question>? Questions { get; set; }

        public string? UserId { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Display(Name = "Difficulty Level")]
        public VideoDifficulty Difficulty { get; set; }
        public ICollection<VideoRating> Ratings { get; set; } = new List<VideoRating>();
        public ICollection<SeeLaterVideo>? SeeLaterVideos { get; set; }
        public string? ThumbnailPath { get; set; }

        [ForeignKey("Course")]
        public int? CourseId { get; set; }
        public Course? Course { get; set; }

        [Display(Name = "Video Status")]
        public VideoStatus Status { get; set; }

        [NotMapped]
        public int ViewCount => UserVideoViews?.Count ?? 0;



        public ICollection<UserVideoView> UserVideoViews { get; set; } = new List<UserVideoView>();
    }
    public enum VideoDifficulty
    {
        Beginner,
        Medium,
        Advanced
    }
    public enum VideoStatus
    {
        Active = 1,
        Inactive = 0,
    }
}
