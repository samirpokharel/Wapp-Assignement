using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleLMS.Data;
using SimpleLMS.Models;

namespace SimpleLMS.Controllers;

public class CoursesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CoursesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Courses
    public async Task<IActionResult> Index()
    {
        // Seed sample courses if none exist
        if (!await _context.Courses.AnyAsync())
        {
            await SeedSampleCourses();
        }

        var courses = await _context.Courses
            .Where(c => c.IsActive)
            .OrderBy(c => c.Title)
            .ToListAsync();
            
        return View(courses);
    }

    // GET: Courses/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .Include(c => c.Topics.OrderBy(t => t.Order))
                .ThenInclude(t => t.ContentItems.OrderBy(ci => ci.Order))
            .Include(c => c.Ratings.OrderByDescending(r => r.CreatedAt))
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (course == null)
        {
            return NotFound();
        }

        // Check if user is enrolled
        var userId = _userManager.GetUserId(User);
        var isEnrolled = false;
        var userRating = (CourseRating?)null;
        if (!string.IsNullOrEmpty(userId))
        {
            isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == course.Id);
            
            userRating = await _context.CourseRatings
                .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == course.Id);
        }

        ViewBag.IsEnrolled = isEnrolled;
        ViewBag.UserRating = userRating;
        return View(course);
    }

    // POST: Courses/Enroll/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Account", "Login", new { area = "Identity" });
        }

        // Check if already enrolled
        var existingEnrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == id);

        if (existingEnrollment == null)
        {
            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = id,
                EnrolledAt = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Courses/Learn/5
    public async Task<IActionResult> Learn(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var course = await _context.Courses
            .Include(c => c.Topics.OrderBy(t => t.Order))
                .ThenInclude(t => t.ContentItems.OrderBy(ci => ci.Order))
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (course == null)
        {
            return NotFound();
        }

        // Check if user is enrolled
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == id);

        if (enrollment == null)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(course);
    }

    // POST: Courses/Complete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == id);

        if (enrollment != null)
        {
            enrollment.IsCompleted = true;
            enrollment.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Courses/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Courses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,ContentPath,Instructor,Duration,Level,Price,ImageUrl")] Course course)
    {
        if (ModelState.IsValid)
        {
            course.CreatedAt = DateTime.UtcNow;
            course.IsActive = true;
            _context.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(course);
    }

    // GET: Courses/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    // POST: Courses/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,ContentPath,CreatedAt,IsActive,Instructor,Duration,Level,Price,ImageUrl")] Course course)
    {
        if (id != course.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(course);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(course.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(course);
    }

    // GET: Courses/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(m => m.Id == id);
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    // POST: Courses/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            course.IsActive = false;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool CourseExists(int id)
    {
        return _context.Courses.Any(e => e.Id == id);
    }

    // GET: Courses/Rate/5
    public async Task<IActionResult> Rate(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        // Check if user has already rated this course
        var existingRating = await _context.CourseRatings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == id);

        if (existingRating != null)
        {
            // User has already rated, redirect to edit
            return RedirectToAction(nameof(EditRating), new { id });
        }

        ViewBag.Course = course;
        return View();
    }

    // POST: Courses/Rate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int id, [Bind("Rating,Feedback")] CourseRating courseRating)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        if (ModelState.IsValid)
        {
            courseRating.UserId = userId;
            courseRating.CourseId = id;
            courseRating.CreatedAt = DateTime.UtcNow;

            _context.CourseRatings.Add(courseRating);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you for your rating and feedback!";
            return RedirectToAction(nameof(Details), new { id });
        }

        var course = await _context.Courses.FindAsync(id);
        ViewBag.Course = course;
        return View(courseRating);
    }

    // GET: Courses/EditRating/5
    public async Task<IActionResult> EditRating(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var rating = await _context.CourseRatings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == id);

        if (rating == null)
        {
            return RedirectToAction(nameof(Rate), new { id });
        }

        var course = await _context.Courses.FindAsync(id);
        ViewBag.Course = course;
        return View(rating);
    }

    // POST: Courses/EditRating/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRating(int id, [Bind("Id,Rating,Feedback")] CourseRating courseRating)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var existingRating = await _context.CourseRatings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == id);

        if (existingRating == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            existingRating.Rating = courseRating.Rating;
            existingRating.Feedback = courseRating.Feedback;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your rating has been updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        var course = await _context.Courses.FindAsync(id);
        ViewBag.Course = course;
        return View(courseRating);
    }

    private async Task SeedSampleCourses()
    {
        if (_context.Courses.Any())
            return;

        var courses = new List<Course>
        {
            new Course
            {
                Title = "Complete Web Development Bootcamp",
                Description = "Learn web development from scratch with HTML, CSS, JavaScript, and modern frameworks. Build real-world projects and become a full-stack developer.",
                ContentPath = "/courses/web-development-bootcamp",
                ContentType = ContentType.Text,
                Content = "# Welcome to Web Development Bootcamp\n\nThis comprehensive course will take you from beginner to advanced web developer.",
                Instructor = "Sarah Johnson",
                Duration = 40,
                Level = "Beginner",
                Price = 0.00m,
                ImageUrl = "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?w=800",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Topics = new List<Topic>
                {
                    new Topic
                    {
                        Title = "Introduction to Web Development",
                        Description = "Get started with the fundamentals of web development and understand the basics of HTML, CSS, and JavaScript.",
                        Order = 1,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        ContentItems = new List<ContentItem>
                        {
                            new ContentItem
                            {
                                Title = "What is Web Development?",
                                Description = "An overview of web development and what you'll learn in this course.",
                                Order = 1,
                                ContentType = ContentType.Text,
                                Content = @"# What is Web Development?

Web development is the process of creating websites and web applications. It involves several key technologies:

## Frontend Development
- **HTML**: Structure and content
- **CSS**: Styling and layout
- **JavaScript**: Interactivity and functionality

## Backend Development
- **Server-side languages**: PHP, Python, Node.js
- **Databases**: MySQL, PostgreSQL, MongoDB
- **APIs**: RESTful services and data exchange

## Full-Stack Development
Combines both frontend and backend development to create complete web applications.

## Why Learn Web Development?
- High demand for web developers
- Creative and rewarding career
- Ability to build your own projects
- Remote work opportunities
- Continuous learning and growth",
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            },
                            new ContentItem
                            {
                                Title = "Setting Up Your Development Environment",
                                Description = "Learn how to set up your computer for web development.",
                                Order = 2,
                                ContentType = ContentType.Text,
                                Content = @"# Setting Up Your Development Environment

Before we start coding, you need to set up your development environment properly.

## Required Tools

### 1. Code Editor
Choose one of these popular code editors:
- **Visual Studio Code** (Recommended)
- **Sublime Text**
- **Atom**
- **WebStorm**

### 2. Web Browser
Install multiple browsers for testing:
- **Chrome** (with DevTools)
- **Firefox** (with Developer Tools)
- **Safari** (for Mac users)
- **Edge** (Windows)

### 3. Version Control
- **Git** for version control
- **GitHub** account for hosting projects

## Installation Steps

### Step 1: Install Visual Studio Code
1. Go to https://code.visualstudio.com/
2. Download for your operating system
3. Install and launch
4. Install useful extensions:
   - Live Server
   - HTML CSS Support
   - JavaScript (ES6) code snippets

### Step 2: Install Git
1. Go to https://git-scm.com/
2. Download and install
3. Configure with your name and email

### Step 3: Create Project Folders
```bash
mkdir web-development-projects
cd web-development-projects
mkdir html-css-basics
mkdir javascript-fundamentals
mkdir responsive-design
```

## Next Steps
Once your environment is set up, we'll start with HTML basics in the next topic!",
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            }
                        }
                    },
                    new Topic
                    {
                        Title = "HTML Fundamentals",
                        Description = "Learn the basics of HTML, the markup language that structures web content.",
                        Order = 2,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        ContentItems = new List<ContentItem>
                        {
                            new ContentItem
                            {
                                Title = "HTML Structure and Elements",
                                Description = "Understanding HTML document structure and basic elements.",
                                Order = 1,
                                ContentType = ContentType.Text,
                                Content = @"# HTML Structure and Elements

HTML (HyperText Markup Language) is the standard markup language for creating web pages.

## Basic HTML Document Structure

```html
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>My First Web Page</title>
</head>
<body>
    <h1>Hello, World!</h1>
    <p>This is my first HTML page.</p>
</body>
</html>
```

## Key HTML Elements

### 1. Document Structure
- `<!DOCTYPE html>`: Declares HTML5 document
- `<html>`: Root element
- `<head>`: Contains metadata
- `<body>`: Contains visible content

### 2. Text Elements
- `<h1>` to `<h6>`: Headings (h1 is most important)
- `<p>`: Paragraphs
- `<strong>`: Bold text
- `<em>`: Italic text
- `<br>`: Line break
- `<hr>`: Horizontal rule

### 3. Lists
```html
<!-- Unordered List -->
<ul>
    <li>Item 1</li>
    <li>Item 2</li>
    <li>Item 3</li>
</ul>

<!-- Ordered List -->
<ol>
    <li>First step</li>
    <li>Second step</li>
    <li>Third step</li>
</ol>
```

### 4. Links and Images
```html
<!-- Links -->
<a href=""https://www.example.com"">Visit Example</a>

<!-- Images -->
<img src=""image.jpg"" alt=""Description of image"">
```

## Best Practices
1. Always use semantic HTML
2. Include alt text for images
3. Use proper heading hierarchy
4. Validate your HTML
5. Keep code clean and readable

## Practice Exercise
Create a simple HTML page with:
- A main heading
- Two paragraphs
- A list of your favorite websites
- An image (use a placeholder service like https://via.placeholder.com/300x200)",
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            },
                            new ContentItem
                            {
                                Title = "HTML Forms and Input",
                                Description = "Learn how to create interactive forms with HTML.",
                                Order = 2,
                                ContentType = ContentType.Text,
                                Content = @"# HTML Forms and Input

Forms are essential for collecting user input on websites.

## Basic Form Structure

```html
<form action=""/submit"" method=""post"">
    <!-- Form elements go here -->
</form>
```

## Common Form Elements

### 1. Text Input
```html
<label for=""username"">Username:</label>
<input type=""text"" id=""username"" name=""username"" required>
```

### 2. Email Input
```html
<label for=""email"">Email:</label>
<input type=""email"" id=""email"" name=""email"" required>
```

### 3. Password Input
```html
<label for=""password"">Password:</label>
<input type=""password"" id=""password"" name=""password"" required>
```

### 4. Textarea
```html
<label for=""message"">Message:</label>
<textarea id=""message"" name=""message"" rows=""4"" cols=""50""></textarea>
```

### 5. Select Dropdown
```html
<label for=""country"">Country:</label>
<select id=""country"" name=""country"">
    <option value="""">Choose a country</option>
    <option value=""us"">United States</option>
    <option value=""ca"">Canada</option>
    <option value=""uk"">United Kingdom</option>
</select>
```

### 6. Radio Buttons
```html
<fieldset>
    <legend>Gender:</legend>
    <input type=""radio"" id=""male"" name=""gender"" value=""male"">
    <label for=""male"">Male</label>
    
    <input type=""radio"" id=""female"" name=""gender"" value=""female"">
    <label for=""female"">Female</label>
</fieldset>
```

### 7. Checkboxes
```html
<fieldset>
    <legend>Interests:</legend>
    <input type=""checkbox"" id=""coding"" name=""interests"" value=""coding"">
    <label for=""coding"">Coding</label>
    
    <input type=""checkbox"" id=""design"" name=""interests"" value=""design"">
    <label for=""design"">Design</label>
    
    <input type=""checkbox"" id=""marketing"" name=""interests"" value=""marketing"">
    <label for=""marketing"">Marketing</label>
</fieldset>
```

### 8. Submit Button
```html
<button type=""submit"">Submit Form</button>
```

## Complete Form Example

```html
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Contact Form</title>
</head>
<body>
    <h1>Contact Us</h1>
    
    <form action=""/submit"" method=""post"">
        <div>
            <label for=""name"">Name:</label>
            <input type=""text"" id=""name"" name=""name"" required>
        </div>
        
        <div>
            <label for=""email"">Email:</label>
            <input type=""email"" id=""email"" name=""email"" required>
        </div>
        
        <div>
            <label for=""message"">Message:</label>
            <textarea id=""message"" name=""message"" rows=""4"" required></textarea>
        </div>
        
        <button type=""submit"">Send Message</button>
    </form>
</body>
</html>
```

## Form Validation
- Use `required` attribute for mandatory fields
- Use appropriate `type` attributes for validation
- Consider using HTML5 validation patterns
- Always validate on the server side too

## Practice Exercise
Create a registration form with:
- Name, email, and password fields
- Date of birth (date input)
- Gender selection (radio buttons)
- Interests (checkboxes)
- A submit button",
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            }
                        }
                    },
                    new Topic
                    {
                        Title = "CSS Styling",
                        Description = "Learn CSS to style and layout your HTML content beautifully.",
                        Order = 3,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        ContentItems = new List<ContentItem>
                        {
                            new ContentItem
                            {
                                Title = "CSS Basics and Selectors",
                                Description = "Introduction to CSS and how to select elements for styling.",
                                Order = 1,
                                ContentType = ContentType.Text,
                                Content = @"# CSS Basics and Selectors

CSS (Cascading Style Sheets) is used to style and layout HTML elements.

## What is CSS?

CSS describes how HTML elements should be displayed. It can control:
- Colors and backgrounds
- Typography and text formatting
- Layout and positioning
- Animations and transitions
- Responsive design

## CSS Syntax

```css
selector {
    property: value;
    property: value;
}
```

## Ways to Include CSS

### 1. Inline CSS
```html
<h1 style=""color: blue; font-size: 24px;"">Hello World</h1>
```

### 2. Internal CSS
```html
<head>
    <style>
        h1 {
            color: blue;
            font-size: 24px;
        }
    </style>
</head>
```

### 3. External CSS (Recommended)
```html
<head>
    <link rel=""stylesheet"" href=""styles.css"">
</head>
```

## CSS Selectors

### 1. Element Selector
```css
h1 {
    color: red;
}
```

### 2. Class Selector
```css
.highlight {
    background-color: yellow;
}
```

### 3. ID Selector
```css
#header {
    background-color: black;
    color: white;
}
```

### 4. Descendant Selector
```css
div p {
    margin: 10px;
}
```

### 5. Child Selector
```css
div > p {
    border: 1px solid black;
}
```

### 6. Pseudo-classes
```css
a:hover {
    color: red;
}

button:active {
    background-color: gray;
}
```

## Common CSS Properties

### Text Properties
```css
h1 {
    color: #333333;
    font-family: Arial, sans-serif;
    font-size: 24px;
    font-weight: bold;
    text-align: center;
    text-decoration: underline;
    line-height: 1.5;
}
```

### Box Model Properties
```css
.box {
    width: 200px;
    height: 100px;
    padding: 20px;
    margin: 10px;
    border: 2px solid black;
    border-radius: 5px;
}
```

### Background Properties
```css
.header {
    background-color: #f0f0f0;
    background-image: url('image.jpg');
    background-repeat: no-repeat;
    background-position: center;
    background-size: cover;
}
```

## CSS Units

### Absolute Units
- `px` (pixels)
- `pt` (points)
- `in` (inches)
- `cm` (centimeters)

### Relative Units
- `%` (percentage)
- `em` (relative to font-size)
- `rem` (relative to root font-size)
- `vw` (viewport width)
- `vh` (viewport height)

## Practice Exercise
Create a CSS file that styles:
1. All headings with different colors
2. A class called `.container` with padding and border
3. Links that change color on hover
4. A button with background color and rounded corners

## Best Practices
1. Use external CSS files
2. Use meaningful class names
3. Keep CSS organized and commented
4. Use relative units when possible
5. Test across different browsers",
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            }
                        }
                    }
                }
            },
            new Course
            {
                Title = "JavaScript Fundamentals",
                Description = "Master JavaScript programming with hands-on projects. Learn variables, functions, objects, and modern ES6+ features.",
                ContentPath = "/courses/javascript-fundamentals",
                ContentType = ContentType.Text,
                Content = "# JavaScript Fundamentals\n\nLearn the core concepts of JavaScript programming language.",
                Instructor = "Michael Chen",
                Duration = 25,
                Level = "Intermediate",
                Price = 0.00m,
                ImageUrl = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=800",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Topics = new List<Topic>
                {
                    new Topic
                    {
                        Title = "JavaScript Basics",
                        Description = "Learn the fundamentals of JavaScript programming including variables, data types, and operators.",
                        Order = 1,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        ContentItems = new List<ContentItem>
                        {
                            new ContentItem
                            {
                                Title = "Variables and Data Types",
                                Description = "Understanding JavaScript variables and the different data types available.",
                                Order = 1,
                                ContentType = ContentType.Text,
                                Content = @"# Variables and Data Types

JavaScript is a dynamically typed language, meaning you don't need to declare the type of a variable.

## Declaring Variables

### Using `let` (Recommended)
```javascript
let name = ""John"";
let age = 25;
let isStudent = true;
```

### Using `const` (For constants)
```javascript
const PI = 3.14159;
const API_URL = ""https://api.example.com"";
```

### Using `var` (Legacy - avoid in modern code)
```javascript
var oldWay = ""not recommended"";
```

## Data Types

### 1. String
```javascript
let message = ""Hello, World!"";
let singleQuotes = 'This is also a string';
let templateLiteral = `Hello ${name}!`;
```

### 2. Number
```javascript
let integer = 42;
let decimal = 3.14;
let negative = -10;
let scientific = 1.5e6; // 1,500,000
```

### 3. Boolean
```javascript
let isTrue = true;
let isFalse = false;
```

### 4. Undefined
```javascript
let notDefined;
console.log(notDefined); // undefined
```

### 5. Null
```javascript
let emptyValue = null;
```

### 6. Object
```javascript
let person = {
    name: ""John"",
    age: 30,
    city: ""New York""
};
```

### 7. Array
```javascript
let colors = [""red"", ""green"", ""blue""];
let numbers = [1, 2, 3, 4, 5];
let mixed = [""hello"", 42, true, null];
```

## Type Checking

```javascript
console.log(typeof ""hello""); // ""string""
console.log(typeof 42); // ""number""
console.log(typeof true); // ""boolean""
console.log(typeof undefined); // ""undefined""
console.log(typeof null); // ""object"" (this is a known bug)
console.log(typeof {}); // ""object""
console.log(typeof []); // ""object""
```

## Type Conversion

### String to Number
```javascript
let num1 = parseInt(""42""); // 42
let num2 = parseFloat(""3.14""); // 3.14
let num3 = Number(""42""); // 42
let num4 = +""42""; // 42 (unary plus)
```

### Number to String
```javascript
let str1 = 42.toString(); // ""42""
let str2 = String(42); // ""42""
let str3 = 42 + """"; // ""42""
```

## Template Literals

```javascript
let name = ""John"";
let age = 30;
let message = `Hello, my name is ${name} and I am ${age} years old.`;
console.log(message); // ""Hello, my name is John and I am 30 years old.""
```

## Practice Exercises

### Exercise 1: Variable Declaration
Create variables for:
- Your name (string)
- Your age (number)
- Whether you're a student (boolean)
- Your favorite colors (array)
- Your information (object)

### Exercise 2: Type Conversion
Convert these values:
- ""123"" to a number
- 456 to a string
- ""true"" to a boolean
- An array to a string

### Exercise 3: Template Literals
Create a sentence using template literals with:
- Your name
- Your profession
- Your location

## Best Practices
1. Use `let` for variables that will change
2. Use `const` for values that won't change
3. Use meaningful variable names
4. Initialize variables when declaring them
5. Use camelCase for variable names",
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            },
                            new ContentItem
                            {
                                Title = "Functions and Scope",
                                Description = "Learn how to create and use functions in JavaScript.",
                                Order = 2,
                                ContentType = ContentType.Text,
                                Content = @"# Functions and Scope

Functions are reusable blocks of code that perform specific tasks.

## Function Declaration

### Basic Function
```javascript
function greet(name) {
    return `Hello, ${name}!`;
}

let message = greet(""John"");
console.log(message); // ""Hello, John!""
```

### Function Expression
```javascript
let greet = function(name) {
    return `Hello, ${name}!`;
};
```

### Arrow Function (ES6+)
```javascript
let greet = (name) => {
    return `Hello, ${name}!`;
};

// Shorter arrow function
let greet = name => `Hello, ${name}!`;
```

## Function Parameters

### Default Parameters
```javascript
function greet(name = ""Guest"") {
    return `Hello, ${name}!`;
}

console.log(greet()); // ""Hello, Guest!""
console.log(greet(""John"")); // ""Hello, John!""
```

### Rest Parameters
```javascript
function sum(...numbers) {
    return numbers.reduce((total, num) => total + num, 0);
}

console.log(sum(1, 2, 3, 4, 5)); // 15
```

## Return Values

```javascript
function add(a, b) {
    return a + b;
}

function multiply(a, b) {
    return a * b;
}

let result = add(5, 3); // 8
let product = multiply(4, 6); // 24
```

## Scope

### Global Scope
```javascript
let globalVar = ""I'm global"";

function testFunction() {
    console.log(globalVar); // Can access global variable
}
```

### Local Scope
```javascript
function testFunction() {
    let localVar = ""I'm local"";
    console.log(localVar); // Can access local variable
}

// console.log(localVar); // Error: localVar is not defined
```

### Block Scope (let and const)
```javascript
if (true) {
    let blockVar = ""I'm in a block"";
    console.log(blockVar); // Works
}

// console.log(blockVar); // Error: blockVar is not defined
```

## Higher-Order Functions

### Functions as Parameters
```javascript
function processArray(arr, callback) {
    return arr.map(callback);
}

let numbers = [1, 2, 3, 4, 5];
let doubled = processArray(numbers, x => x * 2);
console.log(doubled); // [2, 4, 6, 8, 10]
```

### Functions Returning Functions
```javascript
function multiply(x) {
    return function(y) {
        return x * y;
    };
}

let multiplyByTwo = multiply(2);
console.log(multiplyByTwo(5)); // 10
```

## Callback Functions

```javascript
function fetchData(callback) {
    setTimeout(() => {
        let data = ""Some data"";
        callback(data);
    }, 1000);
}

fetchData(function(result) {
    console.log(""Data received:"", result);
});
```

## Practice Exercises

### Exercise 1: Basic Functions
Create functions for:
- Calculating the area of a circle
- Converting Celsius to Fahrenheit
- Checking if a number is even

### Exercise 2: Array Functions
Create functions that:
- Find the maximum number in an array
- Calculate the average of numbers in an array
- Filter even numbers from an array

### Exercise 3: Callback Functions
Create a function that takes an array and a callback, then applies the callback to each element.

## Common Built-in Array Methods

```javascript
let numbers = [1, 2, 3, 4, 5];

// map - transform each element
let doubled = numbers.map(x => x * 2);

// filter - keep elements that pass a test
let evens = numbers.filter(x => x % 2 === 0);

// reduce - combine all elements
let sum = numbers.reduce((total, num) => total + num, 0);

// forEach - execute for each element
numbers.forEach(num => console.log(num));
```

## Best Practices
1. Use descriptive function names
2. Keep functions small and focused
3. Use default parameters when appropriate
4. Return early to avoid deep nesting
5. Use arrow functions for short, simple functions
6. Avoid global variables when possible",
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            }
                        }
                    }
                }
            },
            new Course
            {
                Title = "React.js Complete Guide",
                Description = "Build modern web applications with React.js. Learn component-based architecture, hooks, and state management.",
                ContentPath = "/courses/react-complete-guide",
                ContentType = ContentType.Text,
                Content = "# React.js Complete Guide\n\nLearn to build modern web applications with React.js.",
                Instructor = "Emily Rodriguez",
                Duration = 35,
                Level = "Advanced",
                Price = 0.00m,
                ImageUrl = "https://images.unsplash.com/photo-1633356122544-f134324a6cee?w=800",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Topics = new List<Topic>
                {
                    new Topic
                    {
                        Title = "React Fundamentals",
                        Description = "Learn the core concepts of React including components, JSX, and props.",
                        Order = 1,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        ContentItems = new List<ContentItem>
                        {
                            new ContentItem
                            {
                                Title = "Introduction to React",
                                Description = "Understanding what React is and why it's popular for building user interfaces.",
                                Order = 1,
                                ContentType = ContentType.Text,
                                Content = @"# Introduction to React

React is a JavaScript library for building user interfaces, particularly single-page applications.

## What is React?

React was developed by Facebook and is used to build interactive user interfaces. It's based on the concept of components and uses a virtual DOM for efficient rendering.

## Key Features

### 1. Component-Based Architecture
React applications are built using reusable components:
```javascript
function Welcome(props) {
    return <h1>Hello, {props.name}!</h1>;
}
```

### 2. Virtual DOM
React uses a virtual representation of the DOM for efficient updates:
- Changes are made to the virtual DOM first
- React calculates the minimal changes needed
- Only the necessary parts of the real DOM are updated

### 3. JSX
JSX allows you to write HTML-like code in JavaScript:
```javascript
const element = <h1>Hello, World!</h1>;
```

### 4. Unidirectional Data Flow
Data flows down from parent to child components through props.

## Setting Up React

### Using Create React App
```bash
npx create-react-app my-app
cd my-app
npm start
```

### Manual Setup
```bash
npm init -y
npm install react react-dom
```

## Basic React Component

```javascript
import React from 'react';

function App() {
    return (
        <div className=""App"">
            <header className=""App-header"">
                <h1>Welcome to React</h1>
                <p>Start editing to see some magic happen!</p>
            </header>
        </div>
    );
}

export default App;
```

## JSX Rules

### 1. Must Return Single Element
```javascript
// Correct
function Component() {
    return (
        <div>
            <h1>Title</h1>
            <p>Content</p>
        </div>
    );
}

// Also correct (React Fragment)
function Component() {
    return (
        <>
            <h1>Title</h1>
            <p>Content</p>
        </>
    );
}
```

### 2. Use camelCase for Attributes
```javascript
// HTML
<div class=""container""></div>

// JSX
<div className=""container""></div>
```

### 3. Use JavaScript Expressions
```javascript
const name = ""John"";
const element = <h1>Hello, {name}!</h1>;
```

### 4. Conditional Rendering
```javascript
function Greeting({ isLoggedIn }) {
    return (
        <div>
            {isLoggedIn ? (
                <h1>Welcome back!</h1>
            ) : (
                <h1>Please sign in.</h1>
            )}
        </div>
    );
}
```

## Components and Props

### Functional Components
```javascript
function Welcome(props) {
    return <h1>Hello, {props.name}!</h1>;
}

// Usage
<Welcome name=""John"" />
```

### Class Components
```javascript
class Welcome extends React.Component {
    render() {
        return <h1>Hello, {this.props.name}!</h1>;
    }
}
```

## Props

Props are read-only and passed from parent to child:
```javascript
function UserCard({ name, email, avatar }) {
    return (
        <div className=""user-card"">
            <img src={avatar} alt={name} />
            <h2>{name}</h2>
            <p>{email}</p>
        </div>
    );
}

// Usage
<UserCard 
    name=""John Doe""
    email=""john@example.com""
    avatar=""https://example.com/avatar.jpg""
/>
```

## Practice Exercise

Create a simple React component that:
1. Displays a user's name and age
2. Shows a button that changes the displayed age
3. Uses conditional rendering to show different messages based on age

## Next Steps
In the next topic, we'll learn about state management and hooks!",
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            }
                        }
                    }
                }
            }
        };

        _context.Courses.AddRange(courses);
        await _context.SaveChangesAsync();
    }
} 