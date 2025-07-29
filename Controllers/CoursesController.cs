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
            await SeedSampleCourses();
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
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (course == null)
        {
            return NotFound();
        }

        // Check if user is enrolled
        var userId = _userManager.GetUserId(User);
        var isEnrolled = false;
        if (!string.IsNullOrEmpty(userId))
        {
            isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == course.Id);
        }

        ViewBag.IsEnrolled = isEnrolled;
        return View(course);
    }

    // POST: Courses/Enroll/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
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
                EnrolledAt = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id });
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

    private async Task SeedSampleCourses()
    {
        var sampleCourses = new List<Course>
        {
            new Course
            {
                Title = "Web Development Fundamentals",
                Description = "Learn the basics of HTML, CSS, and JavaScript to build modern websites. Perfect for beginners who want to start their journey in web development.",
                ContentPath = "/courses/web-development-fundamentals",
                ContentType = ContentType.Text,
                Content = "# Web Development Fundamentals\n\n## Introduction\n\nWelcome to the world of web development! In this course, you'll learn the fundamental building blocks of the web.\n\n## What You'll Learn\n\n- HTML structure and semantics\n- CSS styling and layout\n- JavaScript basics and interactivity\n- Responsive design principles\n\n## Course Structure\n\n1. **HTML Basics** - Learn to structure web content\n2. **CSS Styling** - Make your pages beautiful\n3. **JavaScript Fundamentals** - Add interactivity\n4. **Responsive Design** - Work on all devices\n\n## Getting Started\n\nStart with the HTML module and work your way through each section. Practice with the provided exercises to reinforce your learning.",
                Instructor = "Sarah Johnson",
                Duration = 8,
                Level = "Beginner",
                Price = 0.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                IsActive = true
            },
            new Course
            {
                Title = "Advanced JavaScript Mastery",
                Description = "Master advanced JavaScript concepts including ES6+, async programming, and modern frameworks. Take your JavaScript skills to the next level.",
                ContentPath = "/courses/advanced-javascript",
                ContentType = ContentType.Video,
                VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                Instructor = "Michael Chen",
                Duration = 12,
                Level = "Advanced",
                Price = 49.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                IsActive = true
            },
            new Course
            {
                Title = "React.js Complete Guide",
                Description = "Build modern web applications with React.js. Learn component-based architecture, hooks, and state management.",
                ContentPath = "/courses/react-complete-guide",
                ContentType = ContentType.PDF,
                PdfFilePath = "/pdfs/react-complete-guide.pdf",
                Instructor = "Emily Rodriguez",
                Duration = 15,
                Level = "Intermediate",
                Price = 29.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                IsActive = true
            },
            new Course
            {
                Title = "Python for Data Science",
                Description = "Learn Python programming and data analysis techniques. Master pandas, numpy, and matplotlib for data visualization.",
                ContentPath = "/courses/python-data-science",
                ContentType = ContentType.Text,
                Content = "# Python for Data Science\n\n## Course Overview\n\nThis comprehensive course will teach you Python programming with a focus on data science applications.\n\n## Key Topics\n\n- Python fundamentals\n- Data manipulation with pandas\n- Numerical computing with numpy\n- Data visualization with matplotlib\n- Statistical analysis\n\n## Prerequisites\n\nBasic programming knowledge is helpful but not required. We'll start from the ground up.\n\n## Course Modules\n\n1. **Python Basics** - Variables, loops, functions\n2. **Data Structures** - Lists, dictionaries, sets\n3. **Pandas** - Data manipulation and analysis\n4. **Numpy** - Numerical computing\n5. **Matplotlib** - Data visualization\n6. **Real Projects** - Apply your skills",
                Instructor = "David Kim",
                Duration = 10,
                Level = "Intermediate",
                Price = 39.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                IsActive = true
            },
            new Course
            {
                Title = "UI/UX Design Principles",
                Description = "Master the fundamentals of user interface and user experience design. Learn to create beautiful, functional, and user-friendly designs.",
                ContentPath = "/courses/ui-ux-design",
                ContentType = ContentType.Text,
                Content = "# UI/UX Design Principles\n\n## Introduction to Design\n\nLearn the fundamental principles that guide effective user interface and user experience design.\n\n## Design Principles\n\n- **Hierarchy** - Organize information clearly\n- **Consistency** - Maintain design patterns\n- **Accessibility** - Design for everyone\n- **Usability** - Make interfaces intuitive\n\n## Tools You'll Use\n\n- Figma for design\n- Adobe XD for prototyping\n- Sketch for UI design\n- InVision for collaboration\n\n## Course Projects\n\n1. **Wireframing** - Create basic layouts\n2. **Prototyping** - Build interactive mockups\n3. **User Testing** - Validate your designs\n4. **Final Portfolio** - Showcase your work",
                Instructor = "Lisa Wang",
                Duration = 6,
                Level = "Beginner",
                Price = 0.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                IsActive = true
            },
            new Course
            {
                Title = "DevOps and CI/CD",
                Description = "Learn modern DevOps practices, containerization with Docker, and continuous integration/deployment pipelines.",
                ContentPath = "/courses/devops-cicd",
                ContentType = ContentType.Video,
                VideoUrl = "https://www.youtube.com/embed/9UJzfOGblV0",
                Instructor = "Alex Thompson",
                Duration = 18,
                Level = "Advanced",
                Price = 59.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                IsActive = true
            }
        };

        _context.Courses.AddRange(sampleCourses);
        await _context.SaveChangesAsync();
    }
} 