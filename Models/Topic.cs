using System.ComponentModel.DataAnnotations;

namespace SimpleLMS.Models;

public class Topic
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public int CourseId { get; set; }
    
    public int Order { get; set; } = 1; // For ordering topics within a course
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Course Course { get; set; } = null!;
    public List<ContentItem> ContentItems { get; set; } = new List<ContentItem>();
    public List<Progress> Progresses { get; set; } = new List<Progress>();
} 