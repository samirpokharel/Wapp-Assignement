using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SimpleLMS.Models;

public class QuizAttempt
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public int QuizId { get; set; }
    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }
    
    public int Score { get; set; } = 0;
    
    public int TotalPoints { get; set; } = 0;
    
    public double PercentageScore => TotalPoints > 0 ? (double)Score / TotalPoints * 100 : 0;
    
    public bool IsPassed => PercentageScore >= Quiz.PassingScore;
    
    public int AttemptNumber { get; set; } = 1;
    
    public bool IsCompleted { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public IdentityUser User { get; set; } = null!;
    public Quiz Quiz { get; set; } = null!;
    public List<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
} 