using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


public class BaseController : Controller
{
    
    private readonly UserManager<IdentityUser> _userManager;

    public BaseController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["IsAdmin"] = user != null && await _userManager.IsInRoleAsync(user, "Admin");
            ViewData["IsTeacher"] = user != null && await _userManager.IsInRoleAsync(user, "Teacher");
        }

        await base.OnActionExecutionAsync(context, next);
    }
}
