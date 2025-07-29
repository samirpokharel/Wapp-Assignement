// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleLMS.Models; // Add this line

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // This is our existing DbSet for Courses
    public DbSet<Course> Courses { get; set; }

    // Add this new DbSet for the Progress table
    public DbSet<Progress> Progresses { get; set; }
    
    // Add this new DbSet for the Enrollment table
    public DbSet<Enrollment> Enrollments { get; set; }
    
    // Add these new DbSets for the Topic and ContentItem tables
    public DbSet<Topic> Topics { get; set; }
    public DbSet<ContentItem> ContentItems { get; set; }
}