// Models/ViewModels/DashboardViewModel.cs
using SimpleLMS.Models;

namespace SimpleLMS.Models.ViewModels;

public class DashboardViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = string.Empty;
    
    // Enrolled courses and progress
    public List<Enrollment> EnrolledCourses { get; set; } = new List<Enrollment>();
    public List<Enrollment> RecentActivity { get; set; } = new List<Enrollment>();
    public List<Course> RecommendedCourses { get; set; } = new List<Course>();
    
    // Statistics
    public int TotalEnrolled { get; set; }
    public int CompletedCourses { get; set; }
    public int InProgressCourses { get; set; }
    public double CompletionRate { get; set; }
    
    // Legacy property for backward compatibility
    public List<Course> Courses => EnrolledCourses.Select(e => e.Course).ToList();
}