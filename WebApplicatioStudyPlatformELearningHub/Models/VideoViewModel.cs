using System.ComponentModel.DataAnnotations;

namespace StudyPlatformELearningHub.Models
{
    public class VideoViewModel
    {
     
        public VideoFile Video { get; set; }
        public double AverageRating { get; set; }
        public int VideoId { get; set; }
        public string Name { get; set; }
        public int ViewCount { get; set; }


    }
}
