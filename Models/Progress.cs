// Models/Progress.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleLMS.Models;

public class Progress
{
    public int Id { get; set; }

    // This property will hold the foreign key to the Users table
    [Required]
    public string UserId { get; set; } = string.Empty;

    // This property will hold the foreign key to the Courses table
    [Required]
    public int CourseId { get; set; }

    // The status of the progress
    public ProgressStatus Status { get; set; } = ProgressStatus.Incomplete;


    // --- Navigation Properties ---
    // These tell EF Core about the relationships between tables.
    // They are not actual columns in the database but are crucial for queries.

    [ForeignKey("UserId")]
    public virtual IdentityUser User { get; set; } = null!;

    [ForeignKey("CourseId")]
    public virtual Course Course { get; set; } = null!;
}