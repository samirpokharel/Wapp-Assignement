# Quick Start Guide - SimpleLMS

Get SimpleLMS up and running in under 5 minutes!

## ⚡ Quick Setup

### 1. Prerequisites Check
```bash
# Check if .NET 8.0 is installed
dotnet --version
# Should show 8.0.x
```

### 2. Clone & Setup
```bash
# Clone the repository
git clone <repository-url>
cd SimpleLMS

# Restore packages
dotnet restore

# Setup database
dotnet ef database update

# Run the application
dotnet run
```

### 3. Access the Application
- **URL**: https://localhost:7001 or http://localhost:5000
- **Default Admin**: Register a new account and use it for admin features

## 🎯 First Steps

### 1. Create Your Account
1. Navigate to the application
2. Click "Register" in the top navigation
3. Fill in your details and confirm your email
4. Log in with your credentials

### 2. Explore as a User
1. **Browse Courses**: Visit the Courses page to see available courses
2. **Enroll in a Course**: Click "Enroll" on any course
3. **Access Dashboard**: View your enrolled courses and progress
4. **Learn Content**: Navigate through topics and content items

### 3. Try Admin Features
1. **Create a Course**: Go to Admin → Create Course
2. **Add Topics**: Add topics to your course
3. **Add Content**: Add text, video, or PDF content to topics
4. **Manage Content**: Edit and organize your course content

## 🧪 Sample Data

The application includes sample courses with:
- **Web Development Bootcamp**: Complete course with multiple topics
- **Data Science Fundamentals**: Another comprehensive course
- **Sample Content**: Text, video, and PDF examples

## 🔧 Development Commands

```bash
# Run in development mode
dotnet run

# Build the project
dotnet build

# Run tests (if available)
dotnet test

# Create a new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## 🎨 Customization

### Styling
- **Tailwind CSS**: Modify `wwwroot/css/site.css`
- **Layout**: Edit `Views/Shared/_Layout.cshtml`
- **Components**: Update individual view files

### Database
- **Connection**: Modify `appsettings.json`
- **Models**: Update files in `Models/` directory
- **Migrations**: Use Entity Framework commands

### Features
- **Controllers**: Add new controllers in `Controllers/`
- **Views**: Create new views in `Views/`
- **Models**: Extend data models as needed

## 🚨 Common Issues

### Database Issues
```bash
# Reset database
dotnet ef database drop
dotnet ef database update
```

### Port Issues
```bash
# Check what's using the port
lsof -i :5000
# Kill the process or change port in Properties/launchSettings.json
```

### Build Issues
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

## 📱 Mobile Testing

The application is fully responsive. Test on:
- **Desktop**: Chrome, Firefox, Safari, Edge
- **Mobile**: iOS Safari, Android Chrome
- **Tablet**: iPad, Android tablets

## 🔍 Debugging

### Console Logs
- Check the terminal where you ran `dotnet run`
- Look for error messages and stack traces

### Browser DevTools
- **F12**: Open developer tools
- **Console**: Check for JavaScript errors
- **Network**: Monitor API calls
- **Elements**: Inspect HTML structure

### Database Inspection
```bash
# Install SQLite browser or use command line
sqlite3 app.db
.tables
SELECT * FROM Courses;
```

## 🎯 Next Steps

1. **Explore the Codebase**: Familiarize yourself with the project structure
2. **Add Features**: Implement new functionality
3. **Customize UI**: Modify the design to match your needs
4. **Deploy**: Set up production deployment
5. **Test**: Ensure everything works as expected

## 📞 Need Help?

- **Documentation**: Check the main README.md
- **Issues**: Create an issue in the repository
- **Community**: Reach out to the development team

---

**Happy Coding! 🚀** 