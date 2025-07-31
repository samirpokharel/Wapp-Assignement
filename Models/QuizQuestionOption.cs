using System.ComponentModel.DataAnnotations;

namespace SimpleLMS.Models;

public class QuizQuestionOption
{
    public int Id { get; set; }
    
    [Required]
    public int QuizQuestionId { get; set; }
    
    [Required]
    [StringLength(500)]
    public string OptionText { get; set; } = string.Empty;
    
    public bool IsCorrect { get; set; } = false;
    
    public int Order { get; set; } = 1;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public QuizQuestion QuizQuestion { get; set; } = null!;
} 