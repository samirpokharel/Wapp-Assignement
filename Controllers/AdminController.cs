using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleLMS.Data;
using SimpleLMS.Models;

namespace SimpleLMS.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin
    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
            
        return View(courses);
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
    public async Task<IActionResult> Create([Bind("Title,Description,ContentPath,ContentType,Content,VideoUrl,PdfFilePath,Instructor,Duration,Level,Price,ImageUrl")] Course course)
    {
        if (ModelState.IsValid)
        {
            course.CreatedAt = DateTime.UtcNow;
            course.IsActive = true;
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
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,ContentPath,ContentType,Content,VideoUrl,PdfFilePath,CreatedAt,IsActive,Instructor,Duration,Level,Price,ImageUrl")] Course course)
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
            course.IsActive = false;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool CourseExists(int id)
    {
        return _context.Courses.Any(e => e.Id == id);
    }
} 