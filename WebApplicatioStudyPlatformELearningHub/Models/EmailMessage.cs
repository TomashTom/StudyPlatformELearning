using System.ComponentModel.DataAnnotations;

namespace StudyPlatformELearningHub.Models
{
    public class EmailMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }

        public string PhoneNumber { get; set; }

        public byte[] AttachmentData { get; set; } 
        public string AttachmentName { get; set; }
        public bool IsArchived { get; set; }

        public DateTime SentTime { get; set; }
        public EmailStatus Status { get; set; } = EmailStatus.New;
    }
    public enum EmailStatus
    {
        New,
        Done,
        WorkInProgress,
        NeedMoreDetail
       
    }
}
