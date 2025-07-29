using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Required for [Authorize]
using Microsoft.AspNetCore.Identity; // Required for UserManager
using Microsoft.EntityFrameworkCore; // Required for .ToListAsync()
using SimpleLMS.Data;
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
        // Get the currently logged-in user
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            // This case is unlikely due to [Authorize], but good practice
            return Challenge(); 
        }

        // Fetch all courses from the database
        var allCourses = await _context.Courses.OrderBy(c => c.Title).ToListAsync();

        // Prepare the view model with all the necessary data
        var viewModel = new DashboardViewModel
        {
            UserName = user.UserName ?? "User",
            UserInitials = (user.UserName?.Length >= 2) ? user.UserName.Substring(0, 2).ToUpper() : "U",
            Courses = allCourses
        };
        
        // Pass the populated view model to the view
        return View(viewModel);
    }
}