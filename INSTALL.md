# Инструкция по установке и запуску UniversitySystem

## Требования
- .NET 10.0 SDK
- SQL Server LocalDB (обычно устанавливается с Visual Studio)
- Windows 10/11

## Быстрый старт

### 1. Проверка базы данных
Убедитесь, что LocalDB запущена:
```bash
sqllocaldb info
```

Если база данных `UniversityDB` не существует, она будет создана автоматически при первом запуске.

### 2. Запуск приложения
```bash
dotnet run
```

### 3. Вход в систему
В окне входа введите:
- **Логин**: существующий логин из базы данных
- **Пароль**: соответствующий пароль (хранится в открытом виде)

Если в базе данных еще нет пользователей, создайте их через SQL Server Management Studio или другие инструменты.

## Создание тестовых данных (опционально)

Вы можете создать тестовые данные через SQL Server Management Studio:

```sql
-- Создать администратора
INSERT INTO Users (Login, Password, FullName, Role, Email) 
VALUES ('admin', 'admin123', 'Администратор', 'Admin', 'admin@university.edu');

-- Создать преподавателя
INSERT INTO Users (Login, Password, FullName, Role, Email, DepartmentId) 
VALUES ('teacher1', 'teacher123', 'Иванов Иван Иванович', 'Teacher', 'ivanov@university.edu', 1);

-- Создать студента
INSERT INTO Users (Login, Password, FullName, Role, Email, GroupId) 
VALUES ('student1', 'student123', 'Петров Петр Петрович', 'Student', 'petrov@student.edu', 1);
```

## Возможные проблемы и решения

### Приложение закрывается сразу после входа
- Убедитесь, что введены правильные логин и пароль
- Проверьте логи в папке `bin/Debug/net10.0-windows/Logs/`

### Ошибка подключения к базе данных
- Убедитесь, что LocalDB запущена
- Проверьте строку подключения в `Data/AppDbContext.cs`

### Не отображаются панели
- Убедитесь, что у пользователя установлена корректная роль (Admin, Teacher, Student)
- Проверьте, что пользователь авторизован (RbacService.IsAuthenticated)

## Логи
Логи приложения сохраняются в папке:
```
bin/Debug/net10.0-windows/Logs/log_YYYY-MM-DD.txt
```

При возникновении проблем проверьте логи для диагностики.