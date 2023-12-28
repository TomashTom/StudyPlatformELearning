using Microsoft.AspNetCore.Mvc;
using StudyPlatformELearningHub.Data;
using StudyPlatformELearningHub.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StudyPlatformELearningHub.Areas.User.Controllers
{
    [Area("User")]
    public class UserContactUsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserContactUsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
            return View("~/Areas/User/Views/UserContactUs/Index.cshtml", new EmailSend());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(EmailSend emailSend)
        {
           
            if (ModelState.IsValid)
            {
                var emailMessage = new EmailMessage
                {
                    Name = emailSend.Name,
                    Email = emailSend.Email,
                    PhoneNumber = emailSend.PhoneNumber,
                    Subject = emailSend.Subject.ToString(),
                    Message = emailSend.Message,
                    SentTime = DateTime.Now
                };

                if (emailSend.Attachment != null && emailSend.Attachment.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await emailSend.Attachment.CopyToAsync(memoryStream);
                        emailMessage.AttachmentData = memoryStream.ToArray();
                        emailMessage.AttachmentName = Path.GetFileName(emailSend.Attachment.FileName);
                    }
                }
                _context.EmailMessages.Add(emailMessage);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Your message has been sent successfully.";

                return RedirectToAction(nameof(Index));
            }
            return View(new EmailSend());

        }
    }
}
