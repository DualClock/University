# 🎓 UniversitySystem - Университетская информационная система

<div align="center">
  
![.NET](https://img.shields.io/badge/.NET-10.0-blue)
![WPF](https://img.shields.io/badge/WPF-Windows%20Desktop-orange)
![SQLite](https://img.shields.io/badge/Database-SQLite-green)
![License](https://img.shields.io/badge/License-Educational-purple)

**Современная информационная система управления учебным процессом вуза**

[📖 Документация](#-установка-и-настройка) • [🚀 Быстрый старт](#-быстрый-старт) • [📸 Скриншоты](#-скриншоты)

</div>

---

## 📋 Описание

UniversitySystem — это полнофункциональная информационная система для управления учебным процессом в университете. Разработана на WPF (.NET 10) с использованием Entity Framework Core и SQLite.

### ✨ Ключевые возможности

| Роль | Функции |
|------|---------|
| 👨‍💼 **Администратор** | Управление структурой вуза, пользователями, учебным планом |
| 👨‍🏫 **Преподаватель** | Ввод оценок, формирование журнала, экспорт данных |
| 👨‍🎓 **Студент** | Просмотр расписания, оценок, уведомления о задолженностях |

---

## 🛠 Технологии

- **Frontend**: WPF (.NET 10.0), XAML
- **Backend**: C# 13, Entity Framework Core
- **База данных**: SQLite / SQL Server LocalDB
- **Аутентификация**: RBAC (Role-Based Access Control), SHA256 хеширование
- **Экспорт**: ClosedXML (Excel), QuestPDF (PDF)
- **UI**: MaterialDesignThemes

---

## 📥 Установка и настройка

### Требования

| Компонент | Версия | Ссылка |
|-----------|--------|--------|
| .NET SDK | 10.0+ | [Download](https://dotnet.microsoft.com/download) |
| Visual Studio | 2022+ | [Download](https://visualstudio.microsoft.com/) |
| Windows | 10/11 | — |

### Шаг 1: Клонирование репозитория

```bash
git clone https://github.com/DualClock/University.git
cd University/UniversitySystem
```

### Шаг 2: Восстановление зависимостей

```bash
dotnet restore
```

### Шаг 3: Сборка проекта

```bash
dotnet build
```

### Шаг 4: Инициализация базы данных

При первом запуске база данных создастся автоматически. Для создания тестовых данных выполните SQL-скрипт:

```bash
# Откройте SQL Server Management Studio или Azure Data Studio
# Подключитесь к (localdb)\MSSQLLocalDB
# Выполните скрипт из файла DatabaseInit.sql
```

Или создайте пользователей вручную:

```sql
-- Администратор
INSERT INTO Users (Login, Password, FullName, Role, Email) 
VALUES ('admin', 'admin123', 'Системный Администратор', 'Admin', 'admin@university.edu');

-- Преподаватель
INSERT INTO Users (Login, Password, FullName, Role, Email, DepartmentId) 
VALUES ('teacher1', 'teacher123', 'Иванов Иван Иванович', 'Teacher', 'ivanov@university.edu', 1);

-- Студент  
INSERT INTO Users (Login, Password, FullName, Role, Email, GroupId) 
VALUES ('student1', 'student123', 'Петров Петр Петрович', 'Student', 'petrov@student.edu', 1);
```

### Шаг 5: Запуск

```bash
dotnet run
```

---

## 🔐 Тестовые учетные данные

| Роль | Логин | Пароль |
|------|-------|--------|
| Администратор | `admin` | `admin123` |
| Преподаватель | `teacher1` | `teacher123` |
| Студент | `student1` | `student123` |

> ⚠️ **Важно**: Измените пароли перед использованием в production!

---

## 📂 Структура проекта

```
UniversitySystem/
├── Models/                    # Модели данных
│   ├── User.cs               # Пользователь
│   ├── Faculty.cs            # Факультет
│   ├── Department.cs         # Кафедра
│   ├── Group.cs              # Учебная группа
│   ├── Discipline.cs         # Дисциплина
│   ├── Curriculum.cs         # Учебный план
│   ├── Grade.cs              # Оценка
│   └── Schedule.cs           # Расписание
│
├── Data/
│   └── AppDbContext.cs       # Контекст EF Core
│
├── Services/                  # Бизнес-логика
│   ├── AuthService.cs        # Аутентификация
│   ├── RbacService.cs       # Права доступа
│   ├── GradeService.cs       # Оценки
│   ├── ExportService.cs      # Экспорт Excel/PDF
│   └── NotificationService.cs # Уведомления
│
├── Controls/                   # UserControls
│   ├── ExportControl.xaml    # Панель экспорта
│   ├── GradeManagementControl.xaml
│   └── ...
│
└── *.xaml(.cs)               # Окна WPF
```

---

## 📸 Скриншоты

### Окно входа
```
┌─────────────────────────────────┐
│      🎓 UniversitySystem        │
│                                 │
│   ┌─────────────────────────┐   │
│   │     🔐 Вход в систему    │   │
│   └─────────────────────────┘   │
│                                 │
│   Логин:    [____________]      │
│   Пароль:   [____________]      │
│                                 │
│         [Войти]                │
└─────────────────────────────────┘
```

### Панель администратора
- ✅ Управление факультетами, кафедрами, группами
- ✅ Управление дисциплинами и учебным планом
- ✅ Создание и редактирование пользователей
- ✅ Экспорт данных в Excel

### Панель преподавателя
- ✅ Просмотр назначенных групп
- ✅ Ввод и редактирование оценок
- ✅ Формирование журнала
- ✅ Экспорт в Excel

### Панель студента
- ✅ Расписание занятий
- ✅ Успеваемость в реальном времени
- ✅ Уведомления о задолженностях
- ✅ Средний балл

---

## 🔧 Конфигурация

### Строка подключения

Находится в `Data/AppDbContext.cs`:

```csharp
optionsBuilder.UseSqlite("Data Source=UniversityDB.db");
// или для SQL Server:
// optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Database=UniversityDB;");
```

### Логирование

Логи сохраняются в:
```
bin/Debug/net10.0-windows/Logs/log_YYYY-MM-DD.txt
```

---

## 🐛 Возможные проблемы

### ❌ "Cannot find package"
```bash
dotnet restore
```

### ❌ Ошибка подключения к БД
```bash
# Проверьте что LocalDB запущен
sqllocaldb info
# Перезапустите при необходимости
sqllocaldb start MSSQLLocalDB
```

### ❌ Access denied при экспорте
Запустите Visual Studio от имени администратора.

### ❌ Windows Forms/WPF not found
```bash
dotnet workload install wpf
```

---

## 📈 Развитие проекта

- [ ] Добавление Unit-тестов
- [ ] REST API на ASP.NET Core
- [ ] Docker контейнеризация
- [ ] Мобильное приложение (Xamarin/MAUI)
- [ ] Интеграция с 1С

---

## 📄 Лицензия

Образовательный проект. Все права защищены.

---

## 👨‍💻 Автор

Разработано в рамках учебного курса по разработке информационных систем.

---

<div align="center">
  
⭐ Если проект полезен — поставьте звезду!

</div>
