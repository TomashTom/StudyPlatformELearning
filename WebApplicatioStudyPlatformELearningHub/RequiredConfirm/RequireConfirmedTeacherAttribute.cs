using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyPlatformELearningHub.Data;
using StudyPlatformELearningHub.Models;
using StudyPlatformELearningHub.RequiredConfirm;

public class RequireConfirmedRoleHandler : AuthorizationHandler<RequireConfirmedRoleRequirement>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequireConfirmedRoleHandler(UserManager<IdentityUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RequireConfirmedRoleRequirement requirement)
    {
        var user = await _userManager.GetUserAsync(context.User);
        if (user != null)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(requirement.RoleName))
            {
                var entityRole = await _context.EntityRoles.FirstOrDefaultAsync(er => er.UserId == user.Id);
                if (entityRole != null && entityRole.IsRoleConfirmed)
                {
                    context.Succeed(requirement);
                }

            }
        }
    }
}
