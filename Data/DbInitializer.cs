using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleLMS.Models;

namespace SimpleLMS.Data;

public static class DbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

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
        if (context.Courses.Any())
            return;

        var courses = new List<Course>
        {
            new Course
            {
                Title = "Email Basics",
                Description = "Learn the fundamentals of email communication, including composing, sending, and managing emails effectively.",
                ContentPath = "/courses/email-basics",
                ContentType = ContentType.Text,
                Content = "# Email Basics\n\nMaster the essentials of email communication for personal and professional use.",
                Instructor = "Digital Skills Team",
                Duration = 2,
                Level = "Beginner",
                Price = 0.00m,
                ImageUrl = "https://images.unsplash.com/photo-1551434678-e076c223a692?w=800",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Topics = new List<Topic>
                {
                    new Topic
                    {
                        Title = "Understanding Email",
                        Description = "Learn what email is and how it works in the digital world.",
                        Order = 1,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        ContentItems = new List<ContentItem>
                        {
                            new ContentItem
                            {
                                Title = "What is Email?",
                                Description = "Introduction to email and its importance in modern communication.",
                                Order = 1,
                                ContentType = ContentType.Text,
                                Content = @"# What is Email?

Email (Electronic Mail) is a method of exchanging digital messages between people using electronic devices.

## Why Email is Important
- **Fast Communication**: Send messages instantly across the world
- **Professional Tool**: Essential for business and work communication
- **Document Sharing**: Attach files, photos, and documents
- **Record Keeping**: Maintain a history of conversations
- **Cost Effective**: Free to use with internet connection

## How Email Works
1. **Compose**: Write your message
2. **Address**: Enter recipient's email address
3. **Send**: Click send button
4. **Delivery**: Email travels through internet
5. **Receive**: Recipient gets the message

## Email Address Structure
```
username@domain.com
```
- **username**: Your unique identifier
- **@**: Separator symbol
- **domain.com**: The email service provider

## Popular Email Services
- Gmail (Google)
- Outlook (Microsoft)
- Yahoo Mail
- Apple Mail

## Next Steps
In the next topic, we'll learn how to create and manage your email account!",
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            }
                        }
                    }
                }
            },
            new Course
            {
                Title = "Microsoft Word for Beginners",
                Description = "Master the basics of Microsoft Word to create professional documents, letters, and reports.",
                ContentPath = "/courses/microsoft-word",
                ContentType = ContentType.Text,
                Content = "# Microsoft Word for Beginners\n\nLearn to create and format documents using Microsoft Word.",
                Instructor = "Office Skills Team",
                Duration = 3,
                Level = "Beginner",
                Price = 0.00m,
                ImageUrl = "https://images.unsplash.com/photo-1586281380349-632531db7ed4?w=800",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Topics = new List<Topic>
                {
                    new Topic
                    {
                        Title = "Getting Started with Word",
                        Description = "Learn the basic interface and tools of Microsoft Word.",
                        Order = 1,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        ContentItems = new List<ContentItem>
                        {
                            new ContentItem
                            {
                                Title = "Word Interface Overview",
                                Description = "Understanding the Microsoft Word interface and basic tools.",
                                Order = 1,
                                ContentType = ContentType.Text,
                                Content = @"# Microsoft Word Interface Overview

Microsoft Word is a powerful word processing application for creating documents.

## Main Interface Elements

### 1. Title Bar
- Shows document name and application name
- Minimize, maximize, and close buttons

### 2. Ribbon
- Contains tabs with different tools
- **Home**: Basic formatting tools
- **Insert**: Add pictures, tables, headers
- **Layout**: Page setup and formatting
- **Review**: Spelling, grammar, comments

### 3. Document Area
- White space where you type your content
- Shows page boundaries and margins

### 4. Status Bar
- Shows page number, word count, language
- Zoom controls and view options

## Basic Tools

### Text Formatting
- **Font**: Change text style and size
- **Bold**: Make text thicker
- **Italic**: Make text slanted
- **Underline**: Add line under text
- **Color**: Change text color

### Paragraph Formatting
- **Alignment**: Left, center, right, justify
- **Bullets**: Create bulleted lists
- **Numbering**: Create numbered lists
- **Indentation**: Adjust paragraph spacing

## Practice Exercise
Create a simple document with:
1. A title in large, bold text
2. A paragraph with some text
3. A bulleted list
4. Save the document

## Next Steps
In the next topic, we'll learn about document formatting and styles!",
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            }
                        }
                    }
                }
            },
            new Course
            {
                Title = "Digital Literacy Assessment Quiz",
                Description = "Test your digital skills with this comprehensive quiz covering basic computer and internet knowledge.",
                ContentPath = "/courses/digital-literacy-quiz",
                ContentType = ContentType.Quiz,
                Content = "# Digital Literacy Assessment Quiz\n\nTest your knowledge of basic digital skills and computer literacy.",
                Instructor = "Assessment Team",
                Duration = 1,
                Level = "Beginner",
                Price = 0.00m,
                ImageUrl = "https://images.unsplash.com/photo-1503676260728-1c00da094a0b?w=800",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Topics = new List<Topic>
                {
                    new Topic
                    {
                        Title = "Digital Skills Assessment",
                        Description = "A comprehensive quiz to test your digital literacy and computer skills.",
                        Order = 1,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        ContentItems = new List<ContentItem>
                        {
                            new ContentItem
                            {
                                Title = "Digital Literacy Quiz",
                                Description = "Test your knowledge of basic digital skills and computer operations.",
                                Order = 1,
                                ContentType = ContentType.Quiz,
                                Content = "This quiz will test your understanding of basic digital skills including computer operations, internet safety, and common software applications.",
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true,
                                Quiz = new Quiz
                                {
                                    Title = "Digital Literacy Assessment",
                                    Description = "Test your knowledge of basic digital skills and computer literacy.",
                                    TimeLimitMinutes = 30,
                                    PassingScore = 70,
                                    MaxAttempts = 3,
                                    CreatedAt = DateTime.UtcNow,
                                    Questions = new List<QuizQuestion>
                                    {
                                        new QuizQuestion
                                        {
                                            QuestionText = "What is the primary function of a web browser?",
                                            QuestionType = QuestionType.MultipleChoice,
                                            Points = 10,
                                            Order = 1,
                                            IsRequired = true,
                                            CreatedAt = DateTime.UtcNow,
                                            Options = new List<QuizQuestionOption>
                                            {
                                                new QuizQuestionOption
                                                {
                                                    OptionText = "To create documents",
                                                    IsCorrect = false,
                                                    Order = 1,
                                                    CreatedAt = DateTime.UtcNow
                                                },
                                                new QuizQuestionOption
                                                {
                                                    OptionText = "To access websites and web applications",
                                                    IsCorrect = true,
                                                    Order = 2,
                                                    CreatedAt = DateTime.UtcNow
                                                },
                                                new QuizQuestionOption
                                                {
                                                    OptionText = "To send emails",
                                                    IsCorrect = false,
                                                    Order = 3,
                                                    CreatedAt = DateTime.UtcNow
                                                },
                                                new QuizQuestionOption
                                                {
                                                    OptionText = "To play games",
                                                    IsCorrect = false,
                                                    Order = 4,
                                                    CreatedAt = DateTime.UtcNow
                                                }
                                            }
                                        },
                                        new QuizQuestion
                                        {
                                            QuestionText = "Which of the following is a strong password?",
                                            QuestionType = QuestionType.MultipleChoice,
                                            Points = 10,
                                            Order = 2,
                                            IsRequired = true,
                                            CreatedAt = DateTime.UtcNow,
                                            Options = new List<QuizQuestionOption>
                                            {
                                                new QuizQuestionOption
                                                {
                                                    OptionText = "password123",
                                                    IsCorrect = false,
                                                    Order = 1,
                                                    CreatedAt = DateTime.UtcNow
                                                },
                                                new QuizQuestionOption
                                                {
                                                    OptionText = "MyName2024!",
                                                    IsCorrect = true,
                                                    Order = 2,
                                                    CreatedAt = DateTime.UtcNow
                                                },
                                                new QuizQuestionOption
                                                {
                                                    OptionText = "123456",
                                                    IsCorrect = false,
                                                    Order = 3,
                                                    CreatedAt = DateTime.UtcNow
                                                },
                                                new QuizQuestionOption
                                                {
                                                    OptionText = "qwerty",
                                                    IsCorrect = false,
                                                    Order = 4,
                                                    CreatedAt = DateTime.UtcNow
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();
    }
} 