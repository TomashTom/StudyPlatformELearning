namespace StudyPlatformELearningHub.Models
{
    public class CourseViewModel
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public string CreatorFullName { get; set; }
        public string Description { get; set; }
        public DateTime UploadDateTime { get; set; }
        public double Ratings { get; set; }
        public string ThumbnailPath { get; set; }
        //public List<VideoFile> CourseVideos { get; set; }
        public List<VideoFile> Videos { get; set; }
        public int VideoId { get; set; }
        public Course Course { get; set; }


    }
}
