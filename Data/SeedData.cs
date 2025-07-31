using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleLMS.Models;

namespace SimpleLMS.Data;

public static class SeedData
{
    /// <summary>
    /// Manually seed the database with roles, users, and sample data
    /// This can be called from a controller or command line if needed
    /// </summary>
    public static async Task SeedDatabase(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        Console.WriteLine("Starting database seeding...");

        // Seed roles
        await SeedRoles(roleManager);
        Console.WriteLine("✓ Roles seeded successfully");

        // Seed users
        await SeedUsers(userManager);
        Console.WriteLine("✓ Users seeded successfully");

        // Seed courses
        await SeedCourses(context);
        Console.WriteLine("✓ Courses seeded successfully");

        await context.SaveChangesAsync();
        Console.WriteLine("✓ Database seeding completed successfully!");
    }

    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "Instructor", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (result.Succeeded)
                {
                    Console.WriteLine($"  - Created role: {role}");
                }
                else
                {
                    Console.WriteLine($"  - Failed to create role: {role}");
                }
            }
            else
            {
                Console.WriteLine($"  - Role already exists: {role}");
            }
        }
    }

    private static async Task SeedUsers(UserManager<IdentityUser> userManager)
    {
        var users = new[]
        {
            new { Email = "admin@simplelms.com", Password = "Admin123!", Role = "Admin" },
            new { Email = "instructor@simplelms.com", Password = "Instructor123!", Role = "Instructor" }
        };

        foreach (var userInfo in users)
        {
            var user = await userManager.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = userInfo.Email,
                    Email = userInfo.Email,
                    EmailConfirmed = true,
                    NormalizedEmail = userInfo.Email.ToUpper(),
                    NormalizedUserName = userInfo.Email.ToUpper()
                };

                var result = await userManager.CreateAsync(user, userInfo.Password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, userInfo.Role);
                    Console.WriteLine($"  - Created user: {userInfo.Email} with role: {userInfo.Role}");
                }
                else
                {
                    Console.WriteLine($"  - Failed to create user: {userInfo.Email}");
                }
            }
            else
            {
                // Ensure role is assigned
                if (!await userManager.IsInRoleAsync(user, userInfo.Role))
                {
                    await userManager.AddToRoleAsync(user, userInfo.Role);
                    Console.WriteLine($"  - Added role {userInfo.Role} to existing user: {userInfo.Email}");
                }
                else
                {
                    Console.WriteLine($"  - User already exists with role: {userInfo.Email} ({userInfo.Role})");
                }
            }
        }
    }

    private static async Task SeedCourses(ApplicationDbContext context)
    {
        if (await context.Courses.AnyAsync())
        {
            Console.WriteLine("  - Courses already exist, skipping course seeding");
            return;
        }

        var courses = new List<Course>
        {
            new Course
            {
                Title = "Introduction to Web Development",
                Description = "Learn the fundamentals of web development including HTML, CSS, and JavaScript. Perfect for beginners who want to start their journey in web development.",
                ContentType = ContentType.Text,
                Content = "# Introduction to Web Development\n\nWelcome to the world of web development! This course will teach you the basics of creating websites.\n\n## What you'll learn:\n- HTML structure and elements\n- CSS styling and layout\n- JavaScript basics\n- Responsive design principles",
                Instructor = "John Doe",
                Duration = 8,
                Level = "Beginner",
                Price = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Course
            {
                Title = "Advanced JavaScript Programming",
                Description = "Master advanced JavaScript concepts including ES6+, async programming, and modern frameworks. Build real-world applications with confidence.",
                ContentType = ContentType.Text,
                Content = "# Advanced JavaScript Programming\n\nTake your JavaScript skills to the next level with this comprehensive course.\n\n## Topics covered:\n- ES6+ features\n- Async/await patterns\n- Functional programming\n- Design patterns\n- Testing and debugging",
                Instructor = "Jane Smith",
                Duration = 12,
                Level = "Advanced",
                Price = 49.99m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Course
            {
                Title = "Python for Data Science",
                Description = "Learn Python programming specifically for data science applications. Work with pandas, numpy, and matplotlib to analyze and visualize data.",
                ContentType = ContentType.Text,
                Content = "# Python for Data Science\n\nDiscover how to use Python for data analysis and visualization.\n\n## Course content:\n- Python basics for data science\n- Pandas for data manipulation\n- NumPy for numerical computing\n- Matplotlib for visualization\n- Real-world data projects",
                Instructor = "Dr. Sarah Johnson",
                Duration = 10,
                Level = "Intermediate",
                Price = 29.99m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Courses.AddRange(courses);
        Console.WriteLine($"  - Created {courses.Count} sample courses");
    }
} 