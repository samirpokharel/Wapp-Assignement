using System.ComponentModel.DataAnnotations;

namespace SimpleLMS.Models;

public class Quiz
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public int ContentItemId { get; set; }
    
    public int TimeLimitMinutes { get; set; } = 30; // Default 30 minutes
    
    public int PassingScore { get; set; } = 70; // Default 70%
    
    public int MaxAttempts { get; set; } = 3; // Default 3 attempts
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ContentItem ContentItem { get; set; } = null!;
    public List<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    public List<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
} 