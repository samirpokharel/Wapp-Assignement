using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleLMS.Models;

namespace SimpleLMS.Data;

public static class DbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed roles
        await SeedRoles(roleManager);

        // Seed admin user
        await SeedAdminUser(userManager);

        // Seed sample instructor user
        await SeedInstructorUser(userManager);

        // Seed sample courses
        await SeedSampleCourses(context);

        // Save changes
        await context.SaveChangesAsync();
    }

    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "Instructor", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminUser(UserManager<IdentityUser> userManager)
    {
        var adminEmail = "admin@simplelms.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                NormalizedEmail = adminEmail.ToUpper(),
                NormalizedUserName = adminEmail.ToUpper()
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");

            if (result.Succeeded)
            {
                // Add admin role to the user
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            // Ensure admin role is assigned
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    private static async Task SeedInstructorUser(UserManager<IdentityUser> userManager)
    {
        var instructorEmail = "instructor@simplelms.com";
        var instructorUser = await userManager.FindByEmailAsync(instructorEmail);

        if (instructorUser == null)
        {
            instructorUser = new IdentityUser
            {
                UserName = instructorEmail,
                Email = instructorEmail,
                EmailConfirmed = true,
                NormalizedEmail = instructorEmail.ToUpper(),
                NormalizedUserName = instructorEmail.ToUpper()
            };

            var result = await userManager.CreateAsync(instructorUser, "Instructor123!");

            if (result.Succeeded)
            {
                // Add instructor role to the user
                await userManager.AddToRoleAsync(instructorUser, "Instructor");
            }
        }
        else
        {
            // Ensure instructor role is assigned
            if (!await userManager.IsInRoleAsync(instructorUser, "Instructor"))
            {
                await userManager.AddToRoleAsync(instructorUser, "Instructor");
            }
        }
    }

    private static async Task SeedSampleCourses(ApplicationDbContext context)
    {
        // Only seed if no courses exist
        if (await context.Courses.AnyAsync())
            return;

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
    }
} 