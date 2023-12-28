using Microsoft.AspNetCore.Identity;

namespace StudyPlatformELearningHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsBlockedUser { get; set; }
       
        
    }



}

