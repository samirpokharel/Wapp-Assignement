using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleLMS.Data;
using SimpleLMS.Models;

namespace SimpleLMS.Controllers;

public class CoursesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CoursesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Courses
    public async Task<IActionResult> Index()
    {
        // Seed sample courses if none exist
        if (!await _context.Courses.AnyAsync())
        {
            // await SeedSampleCourses();
        }

        var courses = await _context.Courses
            .Where(c => c.IsActive)
            .OrderBy(c => c.Title)
            .ToListAsync();
            
        return View(courses);
    }

    // GET: Courses/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .Include(c => c.Topics.OrderBy(t => t.Order))
                .ThenInclude(t => t.ContentItems.OrderBy(ci => ci.Order))
                    .ThenInclude(ci => ci.Quiz)
            .Include(c => c.Ratings.OrderByDescending(r => r.CreatedAt))
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (course == null)
        {
            return NotFound();
        }

        // Check if user is enrolled
        var userId = _userManager.GetUserId(User);
        var isEnrolled = false;
        var userRating = (CourseRating?)null;
        if (!string.IsNullOrEmpty(userId))
        {
            isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == course.Id);
            
            userRating = await _context.CourseRatings
                .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == course.Id);
        }

        ViewBag.IsEnrolled = isEnrolled;
        ViewBag.UserRating = userRating;
        return View(course);
    }

    // POST: Courses/Enroll/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int id)
    {
        try
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Account", "Login", new { area = "Identity" });
            }

            // Check if user exists
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Account", "Login", new { area = "Identity" });
            }

            // Check if already enrolled
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == id);

            if (existingEnrollment == null)
            {
                var enrollment = new Enrollment
                {
                    UserId = userId,
                    CourseId = id,
                    EnrolledAt = DateTime.UtcNow,
                    IsCompleted = false
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            // Log the error and return a user-friendly message
            return RedirectToAction(nameof(Details), new { id, error = "Enrollment failed. Please try again." });
        }
    }

    // GET: Courses/Learn/5
    public async Task<IActionResult> Learn(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var course = await _context.Courses
            .Include(c => c.Topics.OrderBy(t => t.Order))
                .ThenInclude(t => t.ContentItems.OrderBy(ci => ci.Order))
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (course == null)
        {
            return NotFound();
        }

        // Check if user is enrolled
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == id);

        if (enrollment == null)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(course);
    }

    // POST: Courses/Complete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == id);

        if (enrollment != null)
        {
            enrollment.IsCompleted = true;
            enrollment.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Courses/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Courses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,ContentPath,Instructor,Duration,Level,Price,ImageUrl")] Course course)
    {
        if (ModelState.IsValid)
        {
            course.CreatedAt = DateTime.UtcNow;
            course.IsActive = true;
            _context.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(course);
    }

    // GET: Courses/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    // POST: Courses/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,ContentPath,CreatedAt,IsActive,Instructor,Duration,Level,Price,ImageUrl")] Course course)
    {
        if (id != course.Id)
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
                if (!CourseExists(course.Id))
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

    // GET: Courses/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(m => m.Id == id);
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    // POST: Courses/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            course.IsActive = false;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool CourseExists(int id)
    {
        return _context.Courses.Any(e => e.Id == id);
    }

    // GET: Courses/Rate/5
    public async Task<IActionResult> Rate(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        // Check if user has already rated this course
        var existingRating = await _context.CourseRatings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == id);

        if (existingRating != null)
        {
            // User has already rated, redirect to edit
            return RedirectToAction(nameof(EditRating), new { id });
        }

        ViewBag.Course = course;
        return View();
    }

    // POST: Courses/Rate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int id, [Bind("Rating,Feedback")] CourseRating courseRating)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        if (ModelState.IsValid)
        {
            courseRating.UserId = userId;
            courseRating.CourseId = id;
            courseRating.CreatedAt = DateTime.UtcNow;

            _context.CourseRatings.Add(courseRating);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you for your rating and feedback!";
            return RedirectToAction(nameof(Details), new { id });
        }

        var course = await _context.Courses.FindAsync(id);
        ViewBag.Course = course;
        return View(courseRating);
    }

    // GET: Courses/EditRating/5
    public async Task<IActionResult> EditRating(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var rating = await _context.CourseRatings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == id);

        if (rating == null)
        {
            return RedirectToAction(nameof(Rate), new { id });
        }

        var course = await _context.Courses.FindAsync(id);
        ViewBag.Course = course;
        return View(rating);
    }

    // POST: Courses/EditRating/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRating(int id, [Bind("Id,Rating,Feedback")] CourseRating courseRating)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var existingRating = await _context.CourseRatings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == id);

        if (existingRating == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            existingRating.Rating = courseRating.Rating;
            existingRating.Feedback = courseRating.Feedback;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your rating has been updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        var course = await _context.Courses.FindAsync(id);
        ViewBag.Course = course;
        return View(courseRating);
    }

    // private async Task SeedSampleCourses()
    // {
    //     if (_context.Courses.Any())
    //         return;

    //     var courses = new List<Course> 
       

    //     _context.Courses.AddRange(courses);
    //     await _context.SaveChangesAsync();
    // }
} 