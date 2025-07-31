using System.ComponentModel.DataAnnotations;

namespace SimpleLMS.Models;

public class QuizQuestion
{
    public int Id { get; set; }
    
    [Required]
    public int QuizId { get; set; }
    
    [Required]
    [StringLength(500)]
    public string QuestionText { get; set; } = string.Empty;
    
    public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;
    
    public int Order { get; set; } = 1;
    
    public int Points { get; set; } = 1; // Points for this question
    
    public bool IsRequired { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Quiz Quiz { get; set; } = null!;
    public List<QuizQuestionOption> Options { get; set; } = new List<QuizQuestionOption>();
} 