using System.ComponentModel.DataAnnotations;

namespace StudyPlatformELearningHub.Models
{
    public class UserProfileViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }
        public List<SeeLaterVideo> SavedVideos { get; set; }
        public List<Playlist> Playlists { get; set; }
    }
}
