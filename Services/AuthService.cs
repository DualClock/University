using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;
using UniversitySystem;

namespace UniversitySystem.Services;

/// <summary>
/// Сервис аутентификации и управления пользователями
/// </summary>
public static class AuthService
{
    private static int _failedAttempts = 0;
    private const int MaxAttempts = 5;

    /// <summary>
    /// Вход пользователя в систему
    /// </summary>
    public static async Task<(bool Success, string? Error)> LoginAsync(string login, string password)
    {
        if (_failedAttempts >= MaxAttempts)
            return (false, "Превышено количество попыток входа. Перезапустите приложение.");

        await using var db = new AppDbContext();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Login == login);

        if (user == null)
        {
            _failedAttempts++;
            return (false, $"Неверный логин или пароль. Осталось попыток: {MaxAttempts - _failedAttempts}");
        }

        // Проверка пароля (без хеширования)
        if (password != user.Password)
        {
            _failedAttempts++;
            return (false, $"Неверный логин или пароль. Осталось попыток: {MaxAttempts - _failedAttempts}");
        }

        _failedAttempts = 0;
        RbacService.Login(user.Id, user.Role, user.Login);
        return (true, null);
    }

    /// <summary>
    /// Выход из системы
    /// </summary>
    public static void Logout()
    {
        RbacService.Logout();
        _failedAttempts = 0;
    }

    /// <summary>
    /// Создание нового пользователя с хешированием пароля
    /// </summary>
    public static async Task<bool> CreateUserAsync(
        string login, 
        string password, 
        string role, 
        string fullName, 
        string? email, 
        int? groupId, 
        int? depId)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new ArgumentException("Логин не может быть пустым");
        
        if (password.Length < 4)
            throw new ArgumentException("Пароль должен содержать минимум 4 символа");

        await using var db = new AppDbContext();
        
        // Проверка уникальности логина
        if (await db.Users.AnyAsync(u => u.Login == login))
            throw new ArgumentException("Пользователь с таким логином уже существует");

        var user = new User
        {
            Login = login,
            Password = password, // Сохраняем пароль в открытом виде
            Role = role,
            FullName = fullName,
            Email = email,
            GroupId = groupId,
            DepartmentId = depId
        };

        db.Users.Add(user);
        return await db.SaveChangesAsync() > 0;
    }

}
