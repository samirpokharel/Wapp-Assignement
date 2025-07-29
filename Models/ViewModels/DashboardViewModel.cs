// Models/ViewModels/DashboardViewModel.cs
using SimpleLMS.Models; // This is the line you just added

namespace SimpleLMS.Models.ViewModels;

public class DashboardViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = string.Empty;
    public List<Course> Courses { get; set; } = new List<Course>(); // This line will now work
    public int TotalCourses => Courses.Count;
}