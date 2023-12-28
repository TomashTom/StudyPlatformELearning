using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyPlatformELearningHub.Data;
using StudyPlatformELearningHub.Models;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using StudyPlatformELearningHub.Service;
using StudyPlatformELearningHub.IService;
using System.Text.Encodings.Web;

namespace StudyPlatformELearningHub.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class UserProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;

        public UserProfilesController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

      
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var userRole = await _context.EntityRoles.FirstOrDefaultAsync(er => er.UserId == userId);

            // Get playlists for the user and include videos
            var playlists = await _context.Playlists
                .Where(p => p.UserId == userId)
                .Include(p => p.SeeLaterVideos)
                .ThenInclude(sv => sv.Video)
                .Select(p => new Playlist
                {
                    PlaylistId = p.PlaylistId,
                    Name = p.Name,
                    Videos = p.SeeLaterVideos.Select(sv => new SeeLaterVideo
                    {
                        Video = sv.Video,
                       Note = sv.Note,
                    }).ToList()
                }).ToListAsync();
            //int pageSize = 2; // Display 2 playlists per page
            //int totalPlaylists = playlists.Count();
            //int totalPages = (int)Math.Ceiling((double)totalPlaylists / pageSize);

            //// Set the current page
            //int currentPage = page ?? 1;

            var viewModel = new UserProfileViewModel
            {
                Id = userId,
                Email = user?.Email,
                FirstName = userRole?.FirstName,
                LastName = userRole?.LastName,
                Playlists = playlists,


            };
            //ViewData["CurrentPage"] = currentPage;
            //ViewData["TotalPages"] = totalPages;

            return View( viewModel);
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", viewModel);
            }

            var user = await _userManager.FindByIdAsync(viewModel.Id);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View("Index", viewModel);
            }

            // Update ApplicationUser properties
            user.Email = viewModel.Email;
            user.UserName = viewModel.Email;

            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("Index", viewModel);
            }

            // Update EntityRole properties
            var userRole = await _context.EntityRoles.FirstOrDefaultAsync(er => er.UserId == viewModel.Id);
            if (userRole != null)
            {
                userRole.FirstName = viewModel.FirstName;
                userRole.LastName = viewModel.LastName;

                _context.EntityRoles.Update(userRole);
                await _context.SaveChangesAsync();
            }

            // Check and update password if NewPassword is provided
            if (!string.IsNullOrWhiteSpace(viewModel.NewPassword))
            {
                if (!await _userManager.CheckPasswordAsync(user, viewModel.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View("Index", viewModel);
                }

                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, viewModel.CurrentPassword, viewModel.NewPassword);
                if (!passwordChangeResult.Succeeded)
                {
                    foreach (var error in passwordChangeResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View("Index", viewModel);
                }
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFirstName(string id, string firstName, UserProfileViewModel viewModel)
        {
            if (string.IsNullOrEmpty(firstName))
            {
                // Handle the case where first name is not provided.
                return View("Index", viewModel);
            }

            var userRole = await _context.EntityRoles.FirstOrDefaultAsync(er => er.UserId == id);
            if (userRole != null)
            {
                userRole.FirstName = firstName;

                _context.EntityRoles.Update(userRole);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLastName(string id, string lastName, UserProfileViewModel viewModel)
        {
            if (string.IsNullOrEmpty(lastName))
            {
                // Handle the case where last name is not provided.
                return View("Index", viewModel);
            }

            var userRole = await _context.EntityRoles.FirstOrDefaultAsync(er => er.UserId == id);
            if (userRole != null)
            {
                userRole.LastName = lastName;

                _context.EntityRoles.Update(userRole);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmail(string id, string email, string currentPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(currentPassword))
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    ModelState.AddModelError("Email", "Email cannot be empty.");
                }

                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    ModelState.AddModelError("currentPassword", "Current password cannot be empty.");
                }

                return View("Index"); // Or the appropriate view with model
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View("Index"); // Or the appropriate view with model
            }

            if (!await _userManager.CheckPasswordAsync(user, currentPassword))
            {
                ModelState.AddModelError("currentPassword", "The current password is incorrect.");
                return View("Index", new UserProfileViewModel { Email = user.Email }); // Or the appropriate view with model
            }

            user.Email = email;
            var setEmailResult = await _userManager.SetEmailAsync(user, email);

            if (!setEmailResult.Succeeded)
            {
                foreach (var error in setEmailResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("Index", new UserProfileViewModel { Email = user.Email }); // Or the appropriate view with model
            }

            // Optionally send an email confirmation link
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

            await _emailService.SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return RedirectToAction(nameof(Index));
        }



        //new
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View("ForgotPassword");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                // Generate a random 6-digit numeric code
                Random random = new Random();
                int code = random.Next(100000, 999999);

                // Convert the code to a string
                string codeAsString = code.ToString("D6");
                var callbackUrl = Url.Action(nameof(ResetPassword), "UserProfiles", new { userId = user.Id, code = codeAsString }, protocol: HttpContext.Request.Scheme);

                

                var emailContent = $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a><br />Your code is: {codeAsString}";



                // Use your EmailService to send the email
                var emailSent = await _emailService.SendEmailAsync(model.Email, "Reset Password", emailContent);

                if (!emailSent)
                {
                    ViewData["ErrorMessage"] = "There was an error sending the password reset email.";
                    return View(model);
                }

                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            return View(model);
        }




        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult ResetPassword([FromQuery] string code = null)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                // Password reset was successful, redirect to the index page
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
            }

            return View();
        }


        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        ///

        private bool EntityRoleExists(int id)
        {
            return _context.EntityRoles.Any(e => e.Id == id);
        }
    }
}
