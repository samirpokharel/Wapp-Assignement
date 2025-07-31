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

        // Check user roles and role requests
        var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");
        var isInstructor = await _userManager.IsInRoleAsync(user!, "Instructor");
        
        // Get current user's role request
        var currentRoleRequest = await _context.RoleRequests
            .Where(rr => rr.UserId == userId)
            .OrderByDescending(rr => rr.RequestedAt)
            .FirstOrDefaultAsync();
            
        var hasPendingRoleRequest = currentRoleRequest?.Status == RoleRequestStatus.Pending;
        
        // Get pending role requests for admins
        var pendingRoleRequests = new List<RoleRequest>();
        if (isAdmin)
        {
            pendingRoleRequests = await _context.RoleRequests
                .Include(rr => rr.User)
                .Where(rr => rr.Status == RoleRequestStatus.Pending)
                .OrderBy(rr => rr.RequestedAt)
                .ToListAsync();
        }

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
            RecommendedCourses = recommendedCourses,
            HasPendingRoleRequest = hasPendingRoleRequest,
            CurrentRoleRequest = currentRoleRequest,
            PendingRoleRequests = pendingRoleRequests,
            IsAdmin = isAdmin,
            IsInstructor = isInstructor
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
    
    // POST: Dashboard/RequestInstructorRole
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestInstructorRole(string reason)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }
        
        // Check if user already has a pending request
        var existingRequest = await _context.RoleRequests
            .Where(rr => rr.UserId == userId && rr.Status == RoleRequestStatus.Pending)
            .FirstOrDefaultAsync();
            
        if (existingRequest != null)
        {
            TempData["ErrorMessage"] = "You already have a pending instructor role request.";
            return RedirectToAction(nameof(Index));
        }
        
        // Check if user is already an instructor
        var user = await _userManager.FindByIdAsync(userId);
        if (await _userManager.IsInRoleAsync(user!, "Instructor"))
        {
            TempData["ErrorMessage"] = "You are already an instructor.";
            return RedirectToAction(nameof(Index));
        }
        
        // Create new role request
        var roleRequest = new RoleRequest
        {
            UserId = userId,
            RequestedRole = "Instructor",
            Reason = reason,
            RequestedAt = DateTime.UtcNow,
            Status = RoleRequestStatus.Pending
        };
        
        _context.RoleRequests.Add(roleRequest);
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "Your instructor role request has been submitted successfully. An admin will review it shortly.";
        return RedirectToAction(nameof(Index));
    }
    
    // POST: Dashboard/ApproveRoleRequest
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveRoleRequest(int requestId)
    {
        var roleRequest = await _context.RoleRequests
            .Include(rr => rr.User)
            .FirstOrDefaultAsync(rr => rr.Id == requestId);
            
        if (roleRequest == null)
        {
            return NotFound();
        }
        
        if (roleRequest.Status != RoleRequestStatus.Pending)
        {
            TempData["ErrorMessage"] = "This request has already been processed.";
            return RedirectToAction(nameof(Index));
        }
        
        // Add user to instructor role
        await _userManager.AddToRoleAsync(roleRequest.User, "Instructor");
        
        // Update role request status
        roleRequest.Status = RoleRequestStatus.Approved;
        roleRequest.ProcessedAt = DateTime.UtcNow;
        roleRequest.ProcessedBy = _userManager.GetUserId(User);
        
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = $"Role request for {roleRequest.User.UserName} has been approved.";
        return RedirectToAction(nameof(Index));
    }
    
    // POST: Dashboard/RejectRoleRequest
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectRoleRequest(int requestId, string adminNotes)
    {
        var roleRequest = await _context.RoleRequests
            .Include(rr => rr.User)
            .FirstOrDefaultAsync(rr => rr.Id == requestId);
            
        if (roleRequest == null)
        {
            return NotFound();
        }
        
        if (roleRequest.Status != RoleRequestStatus.Pending)
        {
            TempData["ErrorMessage"] = "This request has already been processed.";
            return RedirectToAction(nameof(Index));
        }
        
        // Update role request status
        roleRequest.Status = RoleRequestStatus.Rejected;
        roleRequest.ProcessedAt = DateTime.UtcNow;
        roleRequest.ProcessedBy = _userManager.GetUserId(User);
        roleRequest.AdminNotes = adminNotes;
        
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = $"Role request for {roleRequest.User.UserName} has been rejected.";
        return RedirectToAction(nameof(Index));
    }
}