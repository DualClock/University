using System;
using System.Windows;
using System.Windows.Input;
using UniversitySystem.Services;

namespace UniversitySystem;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        
        // Обработка нажатия Enter
        KeyDown += (s, e) =>
        {
            if (e.Key == Key.Return)
            {
                LoginButton_Click(this, new RoutedEventArgs());
            }
        };
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string login = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login))
            {
                ErrorText.Text = "Введите логин";
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "Введите пароль";
                return;
            }

            LoginButton.IsEnabled = false;
            LoginButton.Content = "Вход...";
            ErrorText.Text = "";

            // Используем AuthService для аутентификации
            var result = await AuthService.LoginAsync(login, password);

            if (result.Success)
            {
                Close();
            }
            else
            {
                ErrorText.Text = result.Error ?? "Неверный логин или пароль";
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Войти";
            }
        }
        catch (Exception ex)
        {
            ErrorText.Text = $"Ошибка: {ex.Message}";
            LoginButton.IsEnabled = true;
            LoginButton.Content = "Войти";
        }
    }
}