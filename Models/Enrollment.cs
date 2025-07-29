using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleLMS.Models;

public class Enrollment
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public int CourseId { get; set; }
    
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    
    public bool IsCompleted { get; set; } = false;
    
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    [ForeignKey("UserId")]
    public virtual IdentityUser User { get; set; } = null!;
    
    [ForeignKey("CourseId")]
    public virtual Course Course { get; set; } = null!;
} 