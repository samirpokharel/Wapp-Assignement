# SimpleLMS - Learning Management System

A modern, feature-rich Learning Management System (LMS) built with ASP.NET Core 8.0, featuring a beautiful UI with Tailwind CSS and comprehensive course management capabilities.

## 🚀 Features

### Core Features
- **User Authentication & Authorization**: Complete user registration, login, and account management
- **Course Management**: Create, edit, and manage courses with rich content
- **Content Types**: Support for Text (Markdown), Video (YouTube), and PDF content
- **Topic-Based Structure**: Udemy-like course structure with Topics and Content Items
- **Progress Tracking**: Track user progress through courses and individual content items
- **Dashboard**: Personalized dashboard with course statistics and recommendations
- **Admin Panel**: Comprehensive admin interface for course management

### User Experience
- **Modern UI**: Beautiful, responsive design with Tailwind CSS
- **Mobile-Friendly**: Fully responsive design that works on all devices
- **Smooth Animations**: Engaging animations and transitions
- **Intuitive Navigation**: Easy-to-use interface with clear navigation

### Technical Features
- **Entity Framework Core**: Modern data access with SQLite database
- **ASP.NET Core Identity**: Secure user authentication and authorization
- **Razor Pages & MVC**: Hybrid approach for optimal development experience
- **File Upload Support**: PDF upload and management
- **YouTube Integration**: Automatic YouTube URL conversion to embed format
- **Markdown Rendering**: Rich text content with Markdig library

## 📋 Prerequisites

Before running this project, ensure you have the following installed:

- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Git** - For cloning the repository
- **A modern web browser** - Chrome, Firefox, Safari, or Edge

## 🛠️ Installation & Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd SimpleLMS
```

### 2. Install Dependencies
```bash
dotnet restore
```

### 3. Set Up the Database
```bash
# Create and apply database migrations
dotnet ef database update
```

### 4. Run the Application
```bash
dotnet run
```

### 5. Access the Application
Open your browser and navigate to:
- **Development**: https://localhost:7001 or http://localhost:5000
- **Production**: Configure your hosting URL

## 🏗️ Project Structure

```
SimpleLMS/
├── Areas/
│   └── Identity/           # User authentication pages
│       └── Pages/
│           └── Account/    # Login, Register, Account management
├── Controllers/            # MVC Controllers
│   ├── AdminController.cs      # Admin panel functionality
│   ├── ContentItemController.cs # Content item management
│   ├── CoursesController.cs     # Course listing and enrollment
│   ├── DashboardController.cs   # User dashboard
│   ├── HomeController.cs        # Home page
│   └── TopicController.cs       # Topic management
├── Data/
│   ├── ApplicationDbContext.cs  # Entity Framework context
│   └── Migrations/             # Database migrations
├── Models/                 # Data models
│   ├── Course.cs              # Course entity
│   ├── Topic.cs               # Topic entity
│   ├── ContentItem.cs         # Content item entity
│   ├── Enrollment.cs          # User enrollment
│   ├── Progress.cs            # User progress tracking
│   ├── ContentType.cs         # Content type enum
│   └── ViewModels/            # View models
├── Views/                  # Razor views
│   ├── Admin/              # Admin panel views
│   ├── Courses/            # Course-related views
│   ├── Dashboard/          # Dashboard views
│   ├── Home/               # Home page views
│   ├── Topic/              # Topic management views
│   ├── ContentItem/        # Content item views
│   └── Shared/             # Shared layouts and partials
├── wwwroot/               # Static files
│   ├── css/               # Stylesheets
│   ├── js/                # JavaScript files
│   └── lib/               # Library files
├── Program.cs             # Application entry point
├── appsettings.json       # Configuration
└── SimpleLMS.csproj      # Project file
```

## 🎯 Key Features Explained

### Course Management
- **Course Creation**: Admins can create courses with title, description, instructor, duration, level, and price
- **Content Types**: Each course can have different content types:
  - **Text**: Rich Markdown content
  - **Video**: YouTube video integration
  - **PDF**: File upload and display
- **Topic Structure**: Courses are organized into topics, each containing multiple content items
- **Ordering**: Topics and content items can be ordered for proper sequencing

### User Experience
- **Registration & Login**: Secure user authentication with email confirmation
- **Dashboard**: Personalized dashboard showing:
  - Enrolled courses
  - Progress statistics
  - Recent activity
  - Recommended courses
- **Course Enrollment**: Free enrollment system (no payment required)
- **Progress Tracking**: Track completion of individual content items and overall course progress

### Admin Features
- **Course Management**: Full CRUD operations for courses
- **Content Management**: Add, edit, and delete topics and content items
- **File Upload**: PDF upload with automatic file management
- **YouTube Integration**: Automatic conversion of YouTube URLs to embed format

## 🎨 UI/UX Features

### Design System
- **Tailwind CSS**: Modern utility-first CSS framework
- **Glassmorphism**: Beautiful glass-like effects with backdrop blur
- **Gradient Backgrounds**: Eye-catching gradient designs
- **Responsive Design**: Mobile-first approach
- **Smooth Animations**: CSS animations and transitions

### Color Scheme
- **Primary**: Indigo and Purple gradients
- **Success**: Green and Emerald for positive actions
- **Error**: Red for warnings and errors
- **Neutral**: White and Gray for text and backgrounds

## 🔧 Configuration

### Database
The application uses SQLite as the default database:
- **File**: `app.db` (automatically created)
- **Connection String**: Configured in `appsettings.json`

### Authentication
- **Email Confirmation**: Required for new user accounts
- **Password Requirements**: Default ASP.NET Core Identity requirements
- **Session Management**: Standard ASP.NET Core session handling

### File Uploads
- **PDF Storage**: Files stored in `wwwroot/uploads/pdfs/`
- **File Naming**: Unique GUID-based naming to prevent conflicts
- **Size Limits**: Configured for reasonable file sizes

## 🚀 Deployment

### Development
```bash
dotnet run
```

### Production
```bash
dotnet publish -c Release
```

### Docker (Optional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SimpleLMS.csproj", "./"]
RUN dotnet restore "SimpleLMS.csproj"
COPY . .
RUN dotnet build "SimpleLMS.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleLMS.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleLMS.dll"]
```

## 📊 Database Schema

### Core Entities
- **Users**: ASP.NET Core Identity users
- **Courses**: Main course information
- **Topics**: Course chapters/sections
- **ContentItems**: Individual content pieces within topics
- **Enrollments**: User course enrollments
- **Progress**: User progress tracking

### Relationships
- Course → Topics (One-to-Many)
- Topic → ContentItems (One-to-Many)
- User → Enrollments (One-to-Many)
- User → Progress (One-to-Many)
- Course → Enrollments (One-to-Many)
- Course → Progress (One-to-Many)

## 🔒 Security Features

- **HTTPS**: Enforced in production
- **CSRF Protection**: Built-in ASP.NET Core protection
- **Input Validation**: Server-side and client-side validation
- **File Upload Security**: Restricted file types and sizes
- **Authentication**: Secure user authentication with email confirmation

## 🧪 Testing

### Manual Testing
1. **User Registration**: Test the complete registration flow
2. **Course Creation**: Create courses with different content types
3. **Content Management**: Add topics and content items
4. **User Enrollment**: Test course enrollment and progress tracking
5. **Admin Features**: Test all admin functionality

### Automated Testing
```bash
# Run tests (if test project exists)
dotnet test
```

## 🐛 Troubleshooting

### Common Issues

1. **Database Connection Error**
   ```bash
   dotnet ef database update
   ```

2. **Package Restore Issues**
   ```bash
   dotnet restore
   ```

3. **Port Already in Use**
   - Change the port in `Properties/launchSettings.json`
   - Or kill the process using the port

4. **File Upload Issues**
   - Ensure `wwwroot/uploads/` directory exists
   - Check file permissions

### Logs
- **Development**: Logs are displayed in the console
- **Production**: Configure logging in `appsettings.json`

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- **ASP.NET Core**: Microsoft's web framework
- **Tailwind CSS**: Utility-first CSS framework
- **Entity Framework Core**: Microsoft's ORM
- **Markdig**: Markdown processing library

## 📞 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation

---

**SimpleLMS** - Empowering education through modern technology. 