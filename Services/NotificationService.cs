using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;

namespace UniversitySystem.Services;

public static class NotificationService
{
    public static async Task<List<Notification>> GetUnreadAsync(int userId)
    {
        await using var db = new AppDbContext();
        return await db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public static async Task CreateAsync(int userId, string title, string message, string type)
    {
        await using var db = new AppDbContext();
        db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type
        });
        await db.SaveChangesAsync();
    }

    public static async Task MarkAllReadAsync(int userId)
    {
        await using var db = new AppDbContext();
        var notifs = await db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in notifs) n.IsRead = true;
        await db.SaveChangesAsync();
    }
}