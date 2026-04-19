using System;

namespace UniversitySystem;

public static class RbacService
{
    private static int _currentUserId;
    private static string _currentUserRole = string.Empty;
    private static string _currentUserLogin = string.Empty;
    private static DateTime? _loginTime;

    public static int CurrentUserId => _currentUserId;
    public static string CurrentUserRole => _currentUserRole;
    public static string CurrentUserLogin => _currentUserLogin;
    public static string CurrentRole => _currentUserRole; // Алиас для совместимости
    public static DateTime? LoginTime => _loginTime;

    public static bool IsAuthenticated => _currentUserId > 0;

    public static bool IsSessionExpired(int timeoutMinutes = 30) =>
        _loginTime.HasValue && (DateTime.UtcNow - _loginTime.Value).TotalMinutes > timeoutMinutes;

    public static void Login(int userId, string role, string login)
    {
        _currentUserId = userId;
        _currentUserRole = role;
        _currentUserLogin = login;
        _loginTime = DateTime.UtcNow;
    }

    public static void Logout()
    {
        _currentUserId = 0;
        _currentUserRole = string.Empty;
        _currentUserLogin = string.Empty;
        _loginTime = null;
    }

    public static bool HasAccess(string requiredRole)
    {
        if (!IsAuthenticated) return false;

        return requiredRole.ToUpper() switch
        {
            "ADMIN" => _currentUserRole.Equals("Admin", StringComparison.OrdinalIgnoreCase),
            "TEACHER" => _currentUserRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                        _currentUserRole.Equals("Teacher", StringComparison.OrdinalIgnoreCase),
            "STUDENT" => true, // Все аутентифицированные пользователи имеют доступ к студенческим функциям
            _ => false
        };
    }

    public static bool IsOwnerOrAdmin(int targetUserId)
    {
        if (!IsAuthenticated) return false;
        return _currentUserRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
               _currentUserId == targetUserId;
    }
}
