// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity;
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
    
    // Add DbSet for CourseRating
    public DbSet<CourseRating> CourseRatings { get; set; }
    
    // Add DbSets for Quiz system
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizQuestionOption> QuizQuestionOptions { get; set; }
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }
    
    // Add DbSet for RoleRequest table
    public DbSet<RoleRequest> RoleRequests { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Add Role column to AspNetUsers table
        builder.Entity<IdentityUser>()
            .Property<string>("Role")
            .HasMaxLength(50);
    }
}