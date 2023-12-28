using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyPlatformELearningHub.Models
{
    public class Course
    {


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseId { get; set; }
        [Required]
  
        [Display(Name = "Course Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Course Description")]
        public string? Description { get; set; }
        public string CreatorFullName { get; set; }
        [Display(Name = "Upload Date")]
        public DateTime UploadDateTime { get; set; }
        public List<VideoFile> Videos { get; set; }
        public Course()
        {
            Videos = new List<VideoFile>();
        }
        
    }



}



