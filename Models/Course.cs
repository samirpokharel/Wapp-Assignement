// Models/Course.cs
using System.ComponentModel.DataAnnotations;

namespace SimpleLMS.Models;

public class Course
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    [Display(Name = "Content Path")]
    public string ContentPath { get; set; } = string.Empty;
    
    // Content type and actual content
    [Display(Name = "Content Type")]
    public ContentType ContentType { get; set; } = ContentType.Text;
    
    [Display(Name = "Course Content")]
    public string Content { get; set; } = string.Empty; // For text content
    
    [StringLength(500)]
    [Display(Name = "Video URL")]
    public string? VideoUrl { get; set; } // For video content
    
    [StringLength(500)]
    [Display(Name = "PDF File Path")]
    public string? PdfFilePath { get; set; } // For PDF content
    
    // Additional properties that might be needed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    [StringLength(50)]
    public string Instructor { get; set; } = string.Empty;
    
    [Range(1, 100)]
    public int Duration { get; set; } = 1; // Duration in hours
    
    [StringLength(20)]
    public string Level { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced
    
    public decimal Price { get; set; } = 0.00m;
    
    [StringLength(200)]
    public string? ImageUrl { get; set; }
    
    // Navigation properties for relationships
    public List<Progress> Progresses { get; set; } = new List<Progress>();
}