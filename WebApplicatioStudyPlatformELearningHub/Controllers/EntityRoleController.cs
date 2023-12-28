using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyPlatformELearningHub.Data;
using StudyPlatformELearningHub.IService;
using StudyPlatformELearningHub.Models;
using StudyPlatformELearningHub.Pagination;

namespace StudyPlatformELearningHub.Controllers
{
    
    public class EntityRolesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EntityRolesController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public EntityRolesController(ApplicationDbContext context, ILogger<EntityRolesController> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: EntityRoles

        [HttpGet("EntityRoles/IndexPaginated")]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 5)
        {
            var teacherRoleName = "Teacher";
            var query = _context.EntityRoles
                                .Where(er => er.Role == teacherRoleName)
                                .AsNoTracking(); 

            // This now returns a paginated list, even if it's just one page
            var paginatedList = await PaginatedList<EntityRole>.CreateAsync(query, pageIndex, pageSize);
            return View("Index", paginatedList); 
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmRole(int id, [FromServices] IEmailService emailService)
        {
            var entityRole = await _context.EntityRoles.FindAsync(id);
            if (entityRole == null) return NotFound();

            entityRole.IsRoleConfirmed = true;

            // Add the "Confirmed" claim when confirming the role.
            var user = await _userManager.FindByIdAsync(entityRole.UserId);
            if (user != null)
            {
                await _userManager.AddClaimAsync(user, new Claim("Confirmed", "true"));
            }


            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnconfirmRole(int id)
        {
            var entityRole = await _context.EntityRoles.FindAsync(id);
            if (entityRole == null) return NotFound();

            entityRole.IsRoleConfirmed = false;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(int id, string blockReason, [FromServices] IEmailService emailService)
        {
            var entityRole = await _context.EntityRoles.FindAsync(id);
            if (entityRole == null) return NotFound();


            entityRole.IsBlocked = true;
            entityRole.BlockReason = blockReason;

            var userId = entityRole.UserId;
            var userBlockStatus = await _context.UserBlockStatuses
                .Where(ub => ub.UserId == userId)
                .FirstOrDefaultAsync();

            if (userBlockStatus != null)
            {
                userBlockStatus.IsBlocked = true;
            }
            else
            {

                var newUserBlockStatus = new UserBlockStatus { UserId = userId, IsBlocked = true };
                _context.UserBlockStatuses.Add(newUserBlockStatus);
            }

            await _context.SaveChangesAsync();
            var user = await _userManager.FindByIdAsync(entityRole.UserId);
            if (user != null)
            {
                string emailSubject = "Account Blocked";
                string emailBody = $"Your account has been <span style=\"color: red; font-weight: bold;\">blocked</span> for the following reason:<br /><span style=\"color: red;\">{blockReason}</span>.<br /><br />Please contact the administrator for more details at <a href=\"mailto:smolskijt@gmail.com\">smolskijt@gmail.com</a>.";
                await emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser(int id)
        {
            var entityRole = await _context.EntityRoles.FindAsync(id);
            if (entityRole == null)
            {
                _logger.LogWarning("Attempted to unblock a user that does not exist. EntityRole ID: {EntityRoleId}", id);
                return NotFound();
            }

            entityRole.IsBlocked = false;
            var userBlockStatus = await _context.UserBlockStatuses
                .Where(ub => ub.UserId == entityRole.UserId)
                .SingleOrDefaultAsync();

            if (userBlockStatus != null)
            {
                userBlockStatus.IsBlocked = false;
            }
            else
            {
                // Log this situation if necessary
                _logger.LogWarning("UserBlockStatus for user {UserId} not found.", entityRole.UserId);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("User unblocked successfully. EntityRole ID: {EntityRoleId}, UserID: {UserId}", id, entityRole.UserId);

            return RedirectToAction(nameof(Index));
        }
      
    }
}



























