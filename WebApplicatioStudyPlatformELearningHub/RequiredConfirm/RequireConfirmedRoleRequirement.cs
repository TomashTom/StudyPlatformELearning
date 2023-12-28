
using Microsoft.AspNetCore.Authorization;
namespace StudyPlatformELearningHub.RequiredConfirm
{
    public class RequireConfirmedRoleRequirement : IAuthorizationRequirement
    {
        public string RoleName { get; }

        public RequireConfirmedRoleRequirement(string roleName)
        {
            RoleName = roleName;
        }
    }
}
