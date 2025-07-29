using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleLMS.Data;
using SimpleLMS.Models;

namespace SimpleLMS.Controllers;

public class TopicController : Controller
{
    private readonly ApplicationDbContext _context;

    public TopicController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Topic/Create
    public async Task<IActionResult> Create(int courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null)
        {
            return NotFound();
        }

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = course.Title;
            
        return View();
    }

    // POST: Topic/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int courseId, [Bind("Title,Description,Order")] Topic topic)
    {
        if (ModelState.IsValid)
        {
            topic.CourseId = courseId;
            topic.CreatedAt = DateTime.UtcNow;
            topic.IsActive = true;
            
            _context.Add(topic);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }
        
        ViewBag.CourseId = courseId;
        return View(topic);
    }

    // GET: Topic/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var topic = await _context.Topics
            .Include(t => t.Course)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (topic == null)
        {
            return NotFound();
        }
            
        return View(topic);
    }

    // POST: Topic/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,CourseId,Order,CreatedAt,IsActive")] Topic topic)
    {
        if (id != topic.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(topic);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TopicExists(topic.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Details", "Courses", new { id = topic.CourseId });
        }
            
        return View(topic);
    }

    // GET: Topic/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var topic = await _context.Topics
            .Include(t => t.Course)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (topic == null)
        {
            return NotFound();
        }

        return View(topic);
    }

    // POST: Topic/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var topic = await _context.Topics.FindAsync(id);
        if (topic != null)
        {
            var courseId = topic.CourseId;
            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }
        
        return RedirectToAction("Index", "Courses");
    }

    private bool TopicExists(int id)
    {
        return _context.Topics.Any(e => e.Id == id);
    }
} 