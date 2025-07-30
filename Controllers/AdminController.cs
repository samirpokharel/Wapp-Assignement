using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SimpleLMS.Data;
using SimpleLMS.Models;

namespace SimpleLMS.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Admin
    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
            
        return View(courses);
    }
    
    // GET: Admin/RoleRequests
    public async Task<IActionResult> RoleRequests()
    {
        var roleRequests = await _context.RoleRequests
            .Include(rr => rr.User)
            .OrderByDescending(rr => rr.RequestedAt)
            .ToListAsync();
            
        return View(roleRequests);
    }
    
    // POST: Admin/SeedData
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SeedData()
    {
        try
        {
            await SimpleLMS.Data.SeedData.SeedDatabase(HttpContext.RequestServices);
            TempData["SuccessMessage"] = "Database seeded successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error seeding database: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Create
    public IActionResult Create()
    {
        ViewBag.ContentTypes = new SelectList(Enum.GetValues(typeof(ContentType))
            .Cast<ContentType>()
            .Select(ct => new { Value = (int)ct, Text = ct.ToString() }), "Value", "Text");
            
        return View();
    }

    // POST: Admin/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,ContentPath,ContentType,Content,VideoUrl,PdfFilePath,Instructor,Duration,Level,Price,ImageUrl")] Course course, IFormFile? pdfFile)
    {
        if (ModelState.IsValid)
        {
            // Handle PDF file upload
            if (course.ContentType == ContentType.Pdf && pdfFile != null && pdfFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "pdfs");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + (pdfFile.FileName ?? "unknown.pdf");
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await pdfFile.CopyToAsync(fileStream);
                }

                course.PdfFilePath = "/uploads/pdfs/" + uniqueFileName;
            }

            // Handle YouTube video URL
            if (course.ContentType == ContentType.Video && !string.IsNullOrEmpty(course.VideoUrl))
            {
                // Convert YouTube URL to embed format if needed
                if (course.VideoUrl!.Contains("youtube.com/watch?v="))
                {
                    var videoId = course.VideoUrl.Split("v=")[1];
                    if (videoId.Contains("&"))
                    {
                        videoId = videoId.Split("&")[0];
                    }
                    course.VideoUrl = $"https://www.youtube.com/embed/{videoId}";
                }
                else if (course.VideoUrl!.Contains("youtu.be/"))
                {
                    var videoId = course.VideoUrl.Split("youtu.be/")[1];
                    course.VideoUrl = $"https://www.youtube.com/embed/{videoId}";
                }
            }

            course.CreatedAt = DateTime.UtcNow;
            course.IsActive = true;
            
            // Generate a content path if not provided
            if (string.IsNullOrEmpty(course.ContentPath))
            {
                course.ContentPath = $"/courses/{course.Title.ToLower().Replace(" ", "-")}";
            }
            
            _context.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        ViewBag.ContentTypes = new SelectList(Enum.GetValues(typeof(ContentType))
            .Cast<ContentType>()
            .Select(ct => new { Value = (int)ct, Text = ct.ToString() }), "Value", "Text");
            
        return View(course);
    }

    // GET: Admin/Edit/5
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
        
        ViewBag.ContentTypes = new SelectList(Enum.GetValues(typeof(ContentType))
            .Cast<ContentType>()
            .Select(ct => new { Value = (int)ct, Text = ct.ToString() }), "Value", "Text");
            
        return View(course);
    }

    // POST: Admin/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,ContentPath,ContentType,Content,VideoUrl,PdfFilePath,CreatedAt,IsActive,Instructor,Duration,Level,Price,ImageUrl")] Course course, IFormFile? pdfFile)
    {
        if (id != course.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Handle PDF file upload
                if (course.ContentType == ContentType.Pdf && pdfFile != null && pdfFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "pdfs");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + (pdfFile.FileName ?? "unknown.pdf");
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await pdfFile.CopyToAsync(fileStream);
                    }

                    course.PdfFilePath = "/uploads/pdfs/" + uniqueFileName;
                }

                // Handle YouTube video URL
                if (course.ContentType == ContentType.Video && !string.IsNullOrEmpty(course.VideoUrl))
                {
                    // Convert YouTube URL to embed format if needed
                    if (course.VideoUrl!.Contains("youtube.com/watch?v="))
                    {
                        var videoId = course.VideoUrl.Split("v=")[1];
                        if (videoId.Contains("&"))
                        {
                            videoId = videoId.Split("&")[0];
                        }
                        course.VideoUrl = $"https://www.youtube.com/embed/{videoId}";
                    }
                    else if (course.VideoUrl!.Contains("youtu.be/"))
                    {
                        var videoId = course.VideoUrl.Split("youtu.be/")[1];
                        course.VideoUrl = $"https://www.youtube.com/embed/{videoId}";
                    }
                }

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
        
        ViewBag.ContentTypes = new SelectList(Enum.GetValues(typeof(ContentType))
            .Cast<ContentType>()
            .Select(ct => new { Value = (int)ct, Text = ct.ToString() }), "Value", "Text");
            
        return View(course);
    }

    // GET: Admin/Delete/5
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

    // POST: Admin/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }
        
        return RedirectToAction(nameof(Index));
    }

    private bool CourseExists(int id)
    {
        return _context.Courses.Any(e => e.Id == id);
    }
}