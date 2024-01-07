using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyPlatformELearningHub.Data;
using StudyPlatformELearningHub.IService;
using StudyPlatformELearningHub.Models;
using StudyPlatformELearningHub.Pagination;
using System.Linq;
using System.Threading.Tasks;

namespace StudyPlatformELearningHub.Controllers
{
    public class UserProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; 
        private readonly ILogger<EntityRolesController> _logger;

        public UserProfilesController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<EntityRolesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 5)
        {
            var usersWithRolesQuery = _context.EntityRoles
               .Where(er => er.Role != "Admin") // Exclude users with role "Admin"
               .Select(er => new EntityRole
               {
                   Id = er.Id,
                   UserId = er.UserId,
                   FirstName = er.FirstName,
                   LastName = er.LastName,
                   Email = er.Email,
                   Role = er.Role,
                   IsRoleConfirmed = er.IsRoleConfirmed,
                   IsBlocked = er.IsBlocked,
               });

            var paginatedList = await PaginatedList<EntityRole>.CreateAsync(usersWithRolesQuery, pageIndex, pageSize);

            return View(paginatedList);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BlockUser(int id, string blockReason, [FromServices] IEmailService emailService)
        {
            try
            {
                var entityRole = await _context.EntityRoles.FindAsync(id);
                if (entityRole == null)
                {
                    _logger.LogWarning("Attempted to block a non-existing user with ID: {UserId}", id);
                    return NotFound();
                }

                entityRole.IsBlocked = true;
                entityRole.BlockReason = blockReason;
                var userBlockStatus = await _context.UserBlockStatuses
                    .FirstOrDefaultAsync(ubs => ubs.UserId == entityRole.UserId);
                if (userBlockStatus != null)
                {
                    userBlockStatus.IsBlocked = true;
                }
                else
                {
                    _context.UserBlockStatuses.Add(new UserBlockStatus { UserId = entityRole.UserId, IsBlocked = true });
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("User with ID: {UserId} has been blocked", id, blockReason);
                var user = await _userManager.FindByIdAsync(entityRole.UserId);
                if (user != null)
                {
                    string emailSubject = "Account Blocked";
                    string emailBody = $"Your account has been <span style=\"color: red; font-weight: bold;\">blocked</span> for the following reason:<br /><span style=\"color: red;\">{blockReason}</span>.<br /><br />Please contact the administrator for more details at <a href=\"mailto:smolskijt@gmail.com\">smolskijt@gmail.com</a>.";



                    await emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking user with ID: {UserId}", id);
                return View("Error");
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnblockUser(int id, [FromServices] IEmailService emailService)
        {
            try
            {
                var entityRole = await _context.EntityRoles.FindAsync(id);
                if (entityRole == null)
                {
                    _logger.LogWarning("Attempted to unblock a non-existing user with ID: {UserId}", id);
                    return NotFound();
                }

                entityRole.IsBlocked = false;

                var userBlockStatus = await _context.UserBlockStatuses
                    .FirstOrDefaultAsync(ubs => ubs.UserId == entityRole.UserId);
                if (userBlockStatus != null)
                {
                    userBlockStatus.IsBlocked = false;

                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("User with ID: {UserId} has been unblocked", id);
                var user = await _userManager.FindByIdAsync(entityRole.UserId);
                if (user != null)
                {
                    await emailService.SendEmailAsync(
                        user.Email,
                        "Account Unblocked",
                        "Your account has been unblocked. You can now log in as usual."
                    );
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unblocking user with ID: {UserId}", id);
                return View("Error");
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.EntityRoles.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
      

        private bool EntityRoleExists(int id)
        {
            return _context.EntityRoles.Any(e => e.Id == id);


        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var entityRole = await _context.EntityRoles
                .FirstOrDefaultAsync(er => er.UserId == id);
            if (entityRole == null)
            {
                return NotFound();
            }

            return View(entityRole);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var entityRole = await _context.EntityRoles
                .FirstOrDefaultAsync(er => er.UserId == id);
            if (entityRole != null)
            {
                _context.EntityRoles.Remove(entityRole);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User with ID: {UserId} has been deleted.", id);


                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("IdentityUser with ID: {UserId} has been deleted.", user.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to delete IdentityUser with ID: {UserId}.", user.Id);

                    }
                }
            }
            else
            {
                _logger.LogWarning("Attempted to delete a non-existing user role with ID: {UserId}", id);
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
