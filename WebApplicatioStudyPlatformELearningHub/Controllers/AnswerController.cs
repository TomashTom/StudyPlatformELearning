using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyPlatformELearningHub.Data;
using StudyPlatformELearningHub.Models;

namespace StudyPlatformELearningHub.Controllers
{
    public class AnswerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnswerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Answer/Edit/5
        public IActionResult Edit(int id)
        {
            var answer = _context.Answers.FirstOrDefault(a => a.Id == id);
            if (answer == null)
            {
                return NotFound();
            }

            return View("~/Views/Upload/EditAnswer.cshtml", answer);
        }

        // POST: Answer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Answer answer)
        {
            if (id != answer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(answer);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnswerExists(answer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Edit", "Upload", new { id = answer.Question.VideoId });
            }
            return View("~/Views/Upload/EditAnswer.cshtml", answer);
        }


        private bool AnswerExists(int id)
        {
            return _context.Answers.Any(e => e.Id == id);
        }
    }
}
