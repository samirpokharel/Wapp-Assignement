# Seeded Accounts and Roles

This document lists the default accounts and roles that are automatically created when the database is initialized.

## 🔐 **Default Roles**

The following roles are automatically created in the `AspNetRoles` table:

1. **Admin** - Full administrative access to the system
2. **Instructor** - Can create and manage courses
3. **User** - Regular user with access to enroll in courses

## 👤 **Default Accounts**

### **Admin Account**
- **Email**: `admin@simplelms.com`
- **Password**: `Admin123!`
- **Role**: Admin
- **Status**: Email confirmed, ready to use

### **Instructor Account**
- **Email**: `instructor@simplelms.com`
- **Password**: `Instructor123!`
- **Role**: Instructor
- **Status**: Email confirmed, ready to use

## 📚 **Sample Courses**

The following sample courses are automatically created:

1. **Introduction to Web Development**
   - Instructor: John Doe
   - Duration: 8 hours
   - Level: Beginner
   - Price: Free

2. **Advanced JavaScript Programming**
   - Instructor: Jane Smith
   - Duration: 12 hours
   - Level: Advanced
   - Price: $49.99

3. **Python for Data Science**
   - Instructor: Dr. Sarah Johnson
   - Duration: 10 hours
   - Level: Intermediate
   - Price: $29.99

## 🚀 **How to Use**

### **For Testing:**
1. Run the application: `dotnet run`
2. Navigate to the login page
3. Use any of the seeded accounts above to log in

### **For Development:**
- The admin account can access the admin panel at `/Admin`
- The instructor account can test instructor-specific features
- Regular users can register new accounts or use the seeded accounts

### **For Production:**
- **IMPORTANT**: Change the default passwords before deploying to production
- Consider removing or modifying the seeded accounts based on your requirements
- The seeding only runs if the database is empty or when explicitly called

## 🔧 **Customization**

To modify the seeded data, edit the `DbInitializer.cs` file in the `Data` folder:

- Change default passwords in the seeding methods
- Add or remove sample courses
- Modify role names (ensure they match your authorization requirements)
- Add additional sample users

## 📝 **Database Tables**

The seeding process creates/updates the following tables:

- `AspNetRoles` - Contains the three default roles
- `AspNetUsers` - Contains the admin and instructor accounts
- `AspNetUserRoles` - Links users to their respective roles
- `Courses` - Contains the sample courses

## ⚠️ **Security Notes**

- Default passwords are for development/testing only
- Always change passwords in production environments
- Consider implementing password policies for production use
- The seeding process is idempotent - it won't create duplicate entries 