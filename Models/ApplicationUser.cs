using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SimpleLMS.Models;

public class ApplicationUser : IdentityUser
{
    [StringLength(50)]
    public string? Role { get; set; }
} 