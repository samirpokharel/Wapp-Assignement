using System.ComponentModel.DataAnnotations;

namespace SimpleLMS.Models;

public class ContentItem
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public int TopicId { get; set; }
    
    public int Order { get; set; } = 1; // For ordering content items within a topic
    
    [Display(Name = "Content Type")]
    public ContentType ContentType { get; set; } = ContentType.Text;
    
    [Display(Name = "Content")]
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
    public Topic Topic { get; set; } = null!;
    public List<Progress> Progresses { get; set; } = new List<Progress>();
} 