using System.ComponentModel.DataAnnotations;

namespace SimpleLMS.Models;

public class QuizAttemptAnswer
{
    public int Id { get; set; }
    
    [Required]
    public int QuizAttemptId { get; set; }
    
    [Required]
    public int QuizQuestionId { get; set; }
    
    [StringLength(2000)]
    public string? AnswerText { get; set; } // For text-based answers
    
    public int? SelectedOptionId { get; set; } // For multiple choice/true-false
    
    public bool? BooleanAnswer { get; set; } // For true/false questions
    
    public int PointsEarned { get; set; } = 0;
    
    public bool IsCorrect { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public QuizAttempt QuizAttempt { get; set; } = null!;
    public QuizQuestion QuizQuestion { get; set; } = null!;
    public QuizQuestionOption? SelectedOption { get; set; }
} 