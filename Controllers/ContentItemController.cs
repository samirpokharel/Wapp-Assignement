using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleLMS.Data;
using SimpleLMS.Models;

namespace SimpleLMS.Controllers;

public class ContentItemController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ContentItemController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: ContentItem/Create
    public async Task<IActionResult> Create(int topicId)
    {
        var topic = await _context.Topics
            .Include(t => t.Course)
            .FirstOrDefaultAsync(t => t.Id == topicId);
            
        if (topic == null)
        {
            return NotFound();
        }

        ViewBag.TopicId = topicId;
        ViewBag.TopicTitle = topic.Title;
        ViewBag.CourseTitle = topic.Course.Title;
        ViewBag.ContentTypes = new SelectList(Enum.GetValues(typeof(ContentType))
            .Cast<ContentType>()
            .Select(ct => new { Value = (int)ct, Text = ct.ToString() }), "Value", "Text");
            
        return View();
    }

    // POST: ContentItem/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int topicId, [Bind("Title,Description,Order,ContentType,Content,VideoUrl,PdfFilePath")] ContentItem contentItem, IFormFile? pdfFile)
    {
        if (ModelState.IsValid)
        {
            contentItem.TopicId = topicId;
            
            // Handle PDF file upload
            if (contentItem.ContentType == ContentType.Pdf && pdfFile != null && pdfFile.Length > 0)
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

                contentItem.PdfFilePath = "/uploads/pdfs/" + uniqueFileName;
            }

            // Handle YouTube video URL
            if (contentItem.ContentType == ContentType.Video && !string.IsNullOrEmpty(contentItem.VideoUrl))
            {
                // Convert YouTube URL to embed format if needed
                if (contentItem.VideoUrl!.Contains("youtube.com/watch?v="))
                {
                    var videoId = contentItem.VideoUrl.Split("v=")[1];
                    if (videoId.Contains("&"))
                    {
                        videoId = videoId.Split("&")[0];
                    }
                    contentItem.VideoUrl = $"https://www.youtube.com/embed/{videoId}";
                }
                else if (contentItem.VideoUrl!.Contains("youtu.be/"))
                {
                    var videoId = contentItem.VideoUrl.Split("youtu.be/")[1];
                    contentItem.VideoUrl = $"https://www.youtube.com/embed/{videoId}";
                }
            }

            contentItem.CreatedAt = DateTime.UtcNow;
            contentItem.IsActive = true;
            
            _context.Add(contentItem);
            await _context.SaveChangesAsync();
            
            var topic = await _context.Topics.FindAsync(topicId);
            return RedirectToAction("Details", "Courses", new { id = topic!.CourseId });
        }
        
        ViewBag.TopicId = topicId;
        ViewBag.ContentTypes = new SelectList(Enum.GetValues(typeof(ContentType))
            .Cast<ContentType>()
            .Select(ct => new { Value = (int)ct, Text = ct.ToString() }), "Value", "Text");
            
        return View(contentItem);
    }

    // GET: ContentItem/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var contentItem = await _context.ContentItems
            .Include(c => c.Topic)
            .ThenInclude(t => t.Course)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (contentItem == null)
        {
            return NotFound();
        }
        
        ViewBag.ContentTypes = new SelectList(Enum.GetValues(typeof(ContentType))
            .Cast<ContentType>()
            .Select(ct => new { Value = (int)ct, Text = ct.ToString() }), "Value", "Text");
            
        return View(contentItem);
    }

    // POST: ContentItem/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,TopicId,Order,ContentType,Content,VideoUrl,PdfFilePath,CreatedAt,IsActive")] ContentItem contentItem, IFormFile? pdfFile)
    {
        if (id != contentItem.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Handle PDF file upload
                if (contentItem.ContentType == ContentType.Pdf && pdfFile != null && pdfFile.Length > 0)
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

                    contentItem.PdfFilePath = "/uploads/pdfs/" + uniqueFileName;
                }

                // Handle YouTube video URL
                if (contentItem.ContentType == ContentType.Video && !string.IsNullOrEmpty(contentItem.VideoUrl))
                {
                    // Convert YouTube URL to embed format if needed
                    if (contentItem.VideoUrl!.Contains("youtube.com/watch?v="))
                    {
                        var videoId = contentItem.VideoUrl.Split("v=")[1];
                        if (videoId.Contains("&"))
                        {
                            videoId = videoId.Split("&")[0];
                        }
                        contentItem.VideoUrl = $"https://www.youtube.com/embed/{videoId}";
                    }
                    else if (contentItem.VideoUrl!.Contains("youtu.be/"))
                    {
                        var videoId = contentItem.VideoUrl.Split("youtu.be/")[1];
                        contentItem.VideoUrl = $"https://www.youtube.com/embed/{videoId}";
                    }
                }

                _context.Update(contentItem);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContentItemExists(contentItem.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            var topic = await _context.Topics.FindAsync(contentItem.TopicId);
            return RedirectToAction("Details", "Courses", new { id = topic!.CourseId });
        }
        
        ViewBag.ContentTypes = new SelectList(Enum.GetValues(typeof(ContentType))
            .Cast<ContentType>()
            .Select(ct => new { Value = (int)ct, Text = ct.ToString() }), "Value", "Text");
            
        return View(contentItem);
    }

    // GET: ContentItem/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var contentItem = await _context.ContentItems
            .Include(c => c.Topic)
            .ThenInclude(t => t.Course)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (contentItem == null)
        {
            return NotFound();
        }

        return View(contentItem);
    }

    // POST: ContentItem/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var contentItem = await _context.ContentItems
            .Include(c => c.Topic)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (contentItem != null)
        {
            var topic = await _context.Topics.FindAsync(contentItem.TopicId);
            _context.ContentItems.Remove(contentItem);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Courses", new { id = topic!.CourseId });
        }
        
        return RedirectToAction("Index", "Courses");
    }

    // GET: ContentItem/Learn/5
    public async Task<IActionResult> Learn(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var contentItem = await _context.ContentItems
            .Include(c => c.Topic)
            .ThenInclude(t => t.Course)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (contentItem == null)
        {
            return NotFound();
        }

        return View(contentItem);
    }

    // POST: ContentItem/Complete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var contentItem = await _context.ContentItems
            .Include(c => c.Topic)
            .ThenInclude(t => t.Course)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (contentItem == null)
        {
            return NotFound();
        }

        // Check if user is enrolled in the course
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == contentItem.Topic.CourseId);
            
        if (enrollment == null)
        {
            return RedirectToAction("Details", "Courses", new { id = contentItem.Topic.CourseId });
        }

        // Update or create progress
        var progress = await _context.Progresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == contentItem.Topic.CourseId && p.ContentItemId == id);
            
        if (progress == null)
        {
            progress = new Progress
            {
                UserId = userId,
                CourseId = contentItem.Topic.CourseId,
                TopicId = contentItem.TopicId,
                ContentItemId = id,
                Status = ProgressStatus.Complete
            };
            _context.Progresses.Add(progress);
        }
        else
        {
            progress.Status = ProgressStatus.Complete;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Learn", new { id = id });
    }

    private bool ContentItemExists(int id)
    {
        return _context.ContentItems.Any(e => e.Id == id);
    }
} 