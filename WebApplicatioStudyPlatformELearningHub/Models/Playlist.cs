using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyPlatformELearningHub.Models
{
    public class Playlist
    {
        public int PlaylistId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public virtual IdentityUser User { get; set; }
        public virtual ICollection<SeeLaterVideo> SeeLaterVideos { get; set; }
        public ICollection<SeeLaterVideo> Videos { get; set; }
        public int VideoId { get; set; }
      
        



    }
}
