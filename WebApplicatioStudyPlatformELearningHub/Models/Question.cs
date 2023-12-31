﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyPlatformELearningHub.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int VideoId { get; set; }
        public VideoFile Video { get; set; }
        public List<Answer> Answers { get; set; }
        public bool IsEditing { get; set; }
    }
}
