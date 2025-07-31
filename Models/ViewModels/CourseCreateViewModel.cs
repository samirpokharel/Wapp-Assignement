using System.ComponentModel.DataAnnotations;

namespace SimpleLMS.Models.ViewModels;

public class CourseCreateViewModel
{
    // Course properties
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public ContentType ContentType { get; set; }
    
    public string Content { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public string? PdfFilePath { get; set; }
    
    [Required]
    public string Instructor { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    // Quiz-specific properties
    public bool IsQuizCourse { get; set; } = false;
    
    // Topic creation for quiz courses
    public List<TopicCreateViewModel> Topics { get; set; } = new List<TopicCreateViewModel>();
}

public class TopicCreateViewModel
{
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public int Order { get; set; } = 1;
    
    // Quiz content for this topic
    public QuizCreateViewModel? Quiz { get; set; }
}

 