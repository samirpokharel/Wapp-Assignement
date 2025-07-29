using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Required for [Authorize]
using Microsoft.AspNetCore.Identity; // Required for UserManager
using Microsoft.EntityFrameworkCore; // Required for .ToListAsync()
using SimpleLMS.Data;
using SimpleLMS.Models;
using SimpleLMS.Models.ViewModels;
using System.Threading.Tasks;

namespace SimpleLMS.Controllers;

// This attribute ensures that only logged-in users can access any action in this controller.
[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    // We inject the database context and the user manager
    public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // This is the main action for our dashboard page
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        var userName = user?.UserName ?? user?.Email ?? "User";

        // Get user's enrolled courses with progress
        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        // Calculate statistics
        var totalEnrolled = enrollments.Count;
        var completedCourses = enrollments.Count(e => e.IsCompleted);
        var inProgressCourses = enrollments.Count(e => !e.IsCompleted);
        var completionRate = totalEnrolled > 0 ? (double)completedCourses / totalEnrolled * 100 : 0;

        // Get recent activity (last 5 enrollments)
        var recentActivity = enrollments.Take(5).ToList();

        // Get recommended courses (courses not enrolled in)
        var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToList();
        var recommendedCourses = await _context.Courses
            .Where(c => c.IsActive && !enrolledCourseIds.Contains(c.Id))
            .OrderByDescending(c => c.CreatedAt)
            .Take(3)
            .ToListAsync();

        var dashboardViewModel = new DashboardViewModel
        {
            UserName = userName,
            UserInitials = GetUserInitials(userName),
            EnrolledCourses = enrollments,
            TotalEnrolled = totalEnrolled,
            CompletedCourses = completedCourses,
            InProgressCourses = inProgressCourses,
            CompletionRate = completionRate,
            RecentActivity = recentActivity,
            RecommendedCourses = recommendedCourses
        };

        return View(dashboardViewModel);
    }

    private string GetUserInitials(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            return "U";

        var parts = userName.Split('@')[0].Split('.', ' ', '_');
        if (parts.Length >= 2)
        {
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        }
        else if (parts.Length == 1 && parts[0].Length >= 2)
        {
            return parts[0].Substring(0, 2).ToUpper();
        }
        else
        {
            return userName.Substring(0, Math.Min(2, userName.Length)).ToUpper();
        }
    }
}