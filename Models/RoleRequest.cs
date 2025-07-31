using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SimpleLMS.Models;

public class RoleRequest
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string RequestedRole { get; set; } = "Instructor";
    
    public string? Reason { get; set; }
    
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    
    public RoleRequestStatus Status { get; set; } = RoleRequestStatus.Pending;
    
    public DateTime? ProcessedAt { get; set; }
    
    public string? ProcessedBy { get; set; }
    
    public string? AdminNotes { get; set; }
    
    // Navigation property
    public virtual IdentityUser User { get; set; } = null!;
}

public enum RoleRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
} 