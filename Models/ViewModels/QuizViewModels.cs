using System.ComponentModel.DataAnnotations;

namespace SimpleLMS.Models.ViewModels;

public class QuizCreateViewModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public int ContentItemId { get; set; }
    
    [Range(1, 180, ErrorMessage = "Time limit must be between 1 and 180 minutes")]
    public int TimeLimitMinutes { get; set; } = 30;
    
    [Range(1, 100, ErrorMessage = "Passing score must be between 1 and 100")]
    public int PassingScore { get; set; } = 70;
    
    [Range(1, 10, ErrorMessage = "Max attempts must be between 1 and 10")]
    public int MaxAttempts { get; set; } = 3;
    
    // For course creation with topics
    public List<QuizQuestionViewModel> Questions { get; set; } = new List<QuizQuestionViewModel>();
}

public class QuizQuestionViewModel
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(500)]
    public string QuestionText { get; set; } = string.Empty;
    
    public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;
    
    public int Order { get; set; } = 1;
    
    [Range(1, 10, ErrorMessage = "Points must be between 1 and 10")]
    public int Points { get; set; } = 1;
    
    public bool IsRequired { get; set; } = true;
    
    public List<QuizQuestionOptionViewModel> Options { get; set; } = new List<QuizQuestionOptionViewModel>();
}

public class QuizQuestionOptionViewModel
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(500)]
    public string OptionText { get; set; } = string.Empty;
    
    public bool IsCorrect { get; set; } = false;
    
    public int Order { get; set; } = 1;
}

public class QuizAttemptViewModel
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int Score { get; set; }
    public int TotalPoints { get; set; }
    public double PercentageScore { get; set; }
    public bool IsPassed { get; set; }
    public int AttemptNumber { get; set; }
    public bool IsCompleted { get; set; }
    public int TimeRemaining { get; set; }
    public List<QuizQuestionViewModel> Questions { get; set; } = new List<QuizQuestionViewModel>();
}

public class QuizResultsViewModel
{
    public Quiz Quiz { get; set; } = null!;
    public List<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
    public QuizAttempt? BestAttempt { get; set; }
    public double AverageScore { get; set; }
    public int TotalAttempts { get; set; }
    public int PassedAttempts { get; set; }
} 