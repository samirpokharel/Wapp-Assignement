using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SimpleLMS.Models;

public static class UserExtensions
{
    public static async Task<string?> GetRoleAsync(this IdentityUser user, UserManager<IdentityUser> userManager)
    {
        var roles = await userManager.GetRolesAsync(user);
        return roles.FirstOrDefault();
    }

    public static async Task SetRoleAsync(this IdentityUser user, UserManager<IdentityUser> userManager, string role)
    {
        // Remove existing roles
        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }
        
        // Add new role
        await userManager.AddToRoleAsync(user, role);
    }

    public static async Task<bool> IsInRoleAsync(this IdentityUser user, UserManager<IdentityUser> userManager, string role)
    {
        return await userManager.IsInRoleAsync(user, role);
    }
} 