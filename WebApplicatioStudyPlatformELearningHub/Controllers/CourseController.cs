using Microsoft.AspNetCore.Mvc;
using StudyPlatformELearningHub.Data;
using StudyPlatformELearningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class CourseController : Controller
{
    private readonly ApplicationDbContext _context;

    public CourseController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Course
    public async Task<IActionResult> Index()
    {
        string loggedInUser = User.Identity.Name; // Get the logged-in user's name
        var userCourses = await _context.Courses
                                       .Where(c => c.CreatorFullName == loggedInUser)
                                       .ToListAsync();
        return View(userCourses);
    }

 
    public IActionResult Create()
    {
        // Get the name of the logged-in user
        string creatorFullName = User.Identity.Name; // Replace with your user property as needed

        // Create a new Course model with the CreatorFullName property set
        var course = new Course
        {
            CreatorFullName = creatorFullName
        };

        return View(course);
    }

    

  
   [HttpPost]
   [ValidateAntiForgeryToken]

    public async Task<IActionResult> Create(Course course)
    {
        if (ModelState.IsValid)
        {
            // Get the logged-in user's full name from the User property
            string creatorFullName = User.Identity.Name; // Replace with your user property as needed

            // Set the CreatorFullName property of the course
            course.CreatorFullName = creatorFullName;

            _context.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(course);
    }

    // POST: Course/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CourseId,Name,Description")] Course course)
    {
        if (id != course.CourseId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(course);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(course.CourseId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(course);
    }

    // GET: Course/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(m => m.CourseId == id);
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    // POST: Course/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Videos) // Include the related videos
            .FirstOrDefaultAsync(c => c.CourseId == id);

        if (course == null)
        {
            return NotFound();
        }

        _context.Courses.Remove(course); // This will also remove associated videos
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    private bool CourseExists(int id)
    {
        return _context.Courses.Any(e => e.CourseId == id);
    }
}
