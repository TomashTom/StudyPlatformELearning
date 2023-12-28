using System.ComponentModel.DataAnnotations;

namespace StudyPlatformELearningHub.Models
{
    public class UserBlockStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public bool IsBlocked { get; set; }
    }
}
