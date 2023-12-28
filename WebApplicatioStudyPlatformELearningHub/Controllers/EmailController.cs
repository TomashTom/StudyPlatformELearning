using Microsoft.AspNetCore.Mvc;
using StudyPlatformELearningHub.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using StudyPlatformELearningHub.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mime;
using StudyPlatformELearningHub.Pagination;

namespace StudyPlatformELearningHub.Controllers
{
    public class EmailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmailController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> EmailList(string searchTerm, string statusFilter, string sortOrder, int pageIndex = 1, int pageSize = 10)
        {
            ViewData["CurrentFilter"] = searchTerm;
            ViewData["StatusFilter"] = statusFilter;
            ViewData["NameSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["EmailSortParm"] = sortOrder == "email_asc" ? "email_desc" : "email_asc";
            ViewData["PhoneSortParm"] = sortOrder == "phone_asc" ? "phone_desc" : "phone_asc";
            ViewData["SubjectSortParm"] = sortOrder == "subject_asc" ? "subject_desc" : "subject_asc";
            ViewData["MessageSortParm"] = sortOrder == "message_asc" ? "message_desc" : "message_asc";
            ViewData["SentTimeSortParm"] = sortOrder == "senttime_asc" ? "senttime_desc" : "senttime_asc";
            ViewData["StatusSortParm"] = sortOrder == "status_asc" ? "status_desc" : "status_asc";


            var messagesQuery = _context.EmailMessages
                                    .Where(m => !m.IsArchived) 
                                    .AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                messagesQuery = messagesQuery.Where(m =>
                    m.Name.Contains(searchTerm) ||
                    m.Email.Contains(searchTerm) ||
                    m.PhoneNumber.Contains(searchTerm) ||
                    m.Subject.Contains(searchTerm) ||
                    m.Message.Contains(searchTerm)
                );
            }
            if (!string.IsNullOrEmpty(statusFilter))
            {
                messagesQuery = messagesQuery.Where(m => m.Status.ToString() == statusFilter);
            }
            switch (sortOrder)
            {
                case "name_desc":
                    messagesQuery = messagesQuery.OrderByDescending(m => m.Name);
                    break;
                case "name_asc":
                    messagesQuery = messagesQuery.OrderBy(m => m.Name);
                    break;
                case "email_desc":
                    messagesQuery = messagesQuery.OrderByDescending(m => m.Email);
                    break;
                case "email_asc":
                    messagesQuery = messagesQuery.OrderBy(m => m.Email);
                    break;
                case "phone_desc":
                    messagesQuery = messagesQuery.OrderByDescending(m => m.PhoneNumber);
                    break;
                case "phone_asc":
                    messagesQuery = messagesQuery.OrderBy(m => m.PhoneNumber);
                    break;
                case "subject_desc":
                    messagesQuery = messagesQuery.OrderByDescending(m => m.Subject);
                    break;
                case "subject_asc":
                    messagesQuery = messagesQuery.OrderBy(m => m.Subject);
                    break;
                case "message_desc":
                    messagesQuery = messagesQuery.OrderByDescending(m => m.Message);
                    break;
                case "message_asc":
                    messagesQuery = messagesQuery.OrderBy(m => m.Message);
                    break;
                case "senttime_desc":
                    messagesQuery = messagesQuery.OrderByDescending(m => m.SentTime);
                    break;
                case "senttime_asc":
                    messagesQuery = messagesQuery.OrderBy(m => m.SentTime);
                    break;
                case "status_desc":
                    messagesQuery = messagesQuery.OrderByDescending(m => m.Status);
                    break;
                case "status_asc":
                    messagesQuery = messagesQuery.OrderBy(m => m.Status);
                    break;
                default:
                    messagesQuery = messagesQuery.OrderBy(m => m.Name);
                    break;
            }
            var paginatedList = await PaginatedList<EmailMessage>.CreateAsync(messagesQuery, pageIndex, pageSize);
            return View(paginatedList);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int Id, EmailStatus NewStatus)
        {
            var emailMessage = await _context.EmailMessages.FindAsync(Id);
            if (emailMessage == null)
            {
                return NotFound();
            }

            emailMessage.Status = NewStatus;
            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(EmailList));
        }
		//[Authorize(Roles = "Admin")]
		[HttpPost]
        public async Task<IActionResult> Delete(int Id)
        {
            var emailMessage = await _context.EmailMessages.FindAsync(Id);
            if (emailMessage == null)
            {
                return NotFound();
            }
            _context.EmailMessages.Remove(emailMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(EmailList));
        }
        public IActionResult DownloadAttachment(int id)
        {
            var emailMessage = _context.EmailMessages.Find(id);
            if (emailMessage == null || emailMessage.AttachmentData == null || emailMessage.AttachmentData.Length == 0)
            {
               
                return NotFound();
            }
            var contentDisposition = new ContentDisposition
            {
                FileName = emailMessage.AttachmentName,
                Inline = false, 
            };

            Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

            return File(emailMessage.AttachmentData, "application/octet-stream"); 
        }
        [HttpPost]
        public async Task<IActionResult> Archive(int id)
        {
            var message = await _context.EmailMessages.FindAsync(id);
            if (message != null)
            {
                message.IsArchived = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(EmailList));
        }
        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var message = await _context.EmailMessages.FindAsync(id);
            if (message != null)
            {
                message.IsArchived = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ArchivedEmailList));
        }

        [HttpPost]
        public async Task<IActionResult> RestoreAll()
        {
            var archivedMessages = _context.EmailMessages.Where(m => m.IsArchived);
            foreach (var message in archivedMessages)
            {
                message.IsArchived = false;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ArchivedEmailList)); 
        }
        public async Task<IActionResult> ArchivedEmailList()
        {
            var archivedMessages = await _context.EmailMessages
                                                 .Where(m => m.IsArchived)
                                                 .ToListAsync();
            return View(archivedMessages);
        }
        public IActionResult ViewFullMessage(int messageId)
        {
            var fullMessage = GetFullMessageById(messageId);
            return View("ViewFullMessage", fullMessage);
        }
        private string GetFullMessageById(int messageId)
        {
            var message = _context.EmailMessages.FirstOrDefault(m => m.Id == messageId);

            if (message != null)
            {
                return message.Message; 
            }
            return string.Empty; 
        }








    }
}
