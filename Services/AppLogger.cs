using System.IO;

namespace UniversitySystem.Utils;

public static class AppLogger
{
    private static readonly string LogDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

    public static void Info(string message) => Write("INFO", message, null);
    public static void Error(string message, Exception? ex = null) => Write("ERROR", message, ex);

    private static void Write(string level, string message, Exception? ex)
    {
        Directory.CreateDirectory(LogDir);
        var path = Path.Combine(LogDir, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
        var log = $"[{DateTime.Now:HH:mm:ss}] {level}: {message}{(ex != null ? $"\n{ex.Message}\n{ex.StackTrace}" : "")}\n";
        File.AppendAllText(path, log);
    }
}