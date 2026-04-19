using System;
using System.Windows;
using UniversitySystem.Utils;

namespace UniversitySystem;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppLogger.Info("=== ПРИЛОЖЕНИЕ ЗАПУЩЕНО ===");

        this.DispatcherUnhandledException += (s, args) =>
        {
            AppLogger.Error("💥 DispatcherUnhandledException", args.Exception);
            MessageBox.Show(
                $"Ошибка UI-потока:\n{args.Exception.Message}\n\nStack:\n{args.Exception.StackTrace}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        try
        {
            AppLogger.Info("Открываю LoginWindow...");
            var loginWindow = new LoginWindow();
            
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            bool? result = loginWindow.ShowDialog();

            AppLogger.Info($"LoginWindow закрыт с результатом: {result}");

            if (result == true || RbacService.IsAuthenticated)
            {
                AppLogger.Info("Вход успешен, создаю MainWindow...");
                var mainWindow = new MainWindow();
                this.MainWindow = mainWindow;
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;
                AppLogger.Info("MainWindow создан, вызываю Show()...");
                mainWindow.Show();
                AppLogger.Info("MainWindow показан");
            }
            else
            {
                AppLogger.Info($"Вход отменён (result={result})");
                this.Shutdown();
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error("💥 Критическая ошибка в OnStartup", ex);
            MessageBox.Show($"Критическая ошибка:\n{ex.Message}\n\n{ex.StackTrace}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Shutdown();
        }
    }
}