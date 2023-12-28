
namespace StudyPlatformELearningHub.Models

{
    public class EntityRole
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsRoleConfirmed { get; set; } = false;
        public bool IsBlocked { get; set; } = false;
        public string? BlockReason { get; set; }

       

    }
}

