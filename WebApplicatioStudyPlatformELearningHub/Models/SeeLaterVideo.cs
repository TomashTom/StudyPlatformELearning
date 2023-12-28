using Microsoft.AspNetCore.Identity;

namespace StudyPlatformELearningHub.Models
{
    public class SeeLaterVideo
    {
        public int Id { get; set; }
        public string UserId { get; set; } 
        public int VideoId { get; set; }
       
        public IdentityUser User { get; set; }

    
        public VideoFile Video { get; set; }
        public string? Note { get; set; }


        public int? PlaylistId { get; set; } 
        public virtual Playlist Playlist { get; set; }
    }
}
