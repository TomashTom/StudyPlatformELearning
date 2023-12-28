using System.ComponentModel.DataAnnotations;

namespace StudyPlatformELearningHub.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
