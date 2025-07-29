using System.ComponentModel.DataAnnotations;

namespace SimpleLMS.Models;

public class Module
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public int CourseId { get; set; }
    
    public int Order { get; set; } = 1; // For ordering modules within a course
    
    [Display(Name = "Content Type")]
    public ContentType ContentType { get; set; } = ContentType.Text;
    
    [Display(Name = "Module Content")]
    public string Content { get; set; } = string.Empty; // For text content
    
    [StringLength(500)]
    [Display(Name = "Video URL")]
    public string? VideoUrl { get; set; } // For video content
    
    [StringLength(500)]
    [Display(Name = "PDF File Path")]
    public string? PdfFilePath { get; set; } // For PDF content
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Course Course { get; set; } = null!;
    public List<Progress> Progresses { get; set; } = new List<Progress>();
} 