namespace StudyPlatformELearningHub.Models
{
    public class QuestionViewModel
    {
        public List<Question> Questions { get; set; } = new List<Question>();
        public class Question
        {
            public string? Text { get; set; }
            public string? Video { get; set; }
            public List<Answer> Answers { get; set; } = new List<Answer>();
        }

        public class Answer
        {
            public string? Text { get; set; }
            public bool IsCorrect { get; set; }
            public string? IncorrectMessage { get; set; }
        }
    }

}

