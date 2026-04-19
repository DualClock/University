-- ====================================
-- Скрипт создания базы данных UniversityDB
-- ====================================
-- ВНИМАНИЕ: Запускайте в SQL Server Management Studio
-- или через sqlcmd с поддержкой UTF-8
-- ====================================

-- Удаляем существующую базу данных (если есть)
USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'UniversityDB')
BEGIN
    ALTER DATABASE UniversityDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE UniversityDB;
END
GO

-- Создаем новую базу данных с поддержкой UTF-8
CREATE DATABASE UniversityDB
COLLATE Cyrillic_General_100_CI_AS;
GO

USE UniversityDB;
GO

-- ====================================
-- Таблицы
-- ====================================

-- Факультеты
CREATE TABLE Faculties (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(20) NOT NULL UNIQUE
);
GO

-- Кафедры
CREATE TABLE Departments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    FacultyId INT NOT NULL FOREIGN KEY REFERENCES Faculties(Id) ON DELETE CASCADE
);
GO

-- Группы
CREATE TABLE Groups (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Year INT NOT NULL,
    DepartmentId INT NOT NULL FOREIGN KEY REFERENCES Departments(Id) ON DELETE CASCADE
);
GO

-- Дисциплины
CREATE TABLE Disciplines (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    LectureHours INT NOT NULL DEFAULT 0,
    SeminarHours INT NOT NULL DEFAULT 0,
    LabHours INT NOT NULL DEFAULT 0,
    Credits INT NOT NULL DEFAULT 0
);
GO

-- Пользователи
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Login NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(200) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(100) NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'Student',
    GroupId INT NULL FOREIGN KEY REFERENCES Groups(Id),
    DepartmentId INT NULL FOREIGN KEY REFERENCES Departments(Id)
);
GO

-- Учебный план
CREATE TABLE Curricula (
    Id INT PRIMARY KEY IDENTITY(1,1),
    GroupId INT NOT NULL FOREIGN KEY REFERENCES Groups(Id) ON DELETE CASCADE,
    DisciplineId INT NOT NULL FOREIGN KEY REFERENCES Disciplines(Id) ON DELETE CASCADE,
    Semester INT NOT NULL,
    AcademicYear NVARCHAR(20) NOT NULL,
    CONSTRAINT UQ_Curriculum UNIQUE (GroupId, DisciplineId, Semester, AcademicYear)
);
GO

-- Расписание
CREATE TABLE Schedules (
    Id INT PRIMARY KEY IDENTITY(1,1),
    GroupId INT NOT NULL FOREIGN KEY REFERENCES Groups(Id) ON DELETE CASCADE,
    DisciplineId INT NOT NULL FOREIGN KEY REFERENCES Disciplines(Id) ON DELETE CASCADE,
    TeacherId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    DayOfWeek INT NOT NULL, -- 1=Понедельник, 7=Воскресенье
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Room NVARCHAR(20) NOT NULL,
    Type NVARCHAR(20) NOT NULL -- Lecture, Seminar, Lab
);
GO

-- Оценки
CREATE TABLE Grades (
    Id INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    DisciplineId INT NOT NULL FOREIGN KEY REFERENCES Disciplines(Id) ON DELETE CASCADE,
    Type NVARCHAR(20) NOT NULL, -- Seminar, Lab, Exam
    Value DECIMAL(3,1) NOT NULL,
    Date DATETIME NOT NULL DEFAULT GETUTCDATE(),
    Semester INT NOT NULL,
    AcademicYear NVARCHAR(20) NOT NULL
);
GO

-- Уведомления
CREATE TABLE Notifications (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    Title NVARCHAR(100) NOT NULL,
    Message NVARCHAR(500) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    Type NVARCHAR(20) NOT NULL, -- Grade, Debt, Schedule, System
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE()
);
GO

-- Связь пользователи-группы (многие-ко-многим)
CREATE TABLE UserGroups (
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    GroupId INT NOT NULL FOREIGN KEY REFERENCES Groups(Id) ON DELETE CASCADE,
    CONSTRAINT PK_UserGroups PRIMARY KEY (UserId, GroupId)
);
GO

-- Связь пользователи-кафедры (многие-ко-многим)
CREATE TABLE UserDepartments (
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    DepartmentId INT NOT NULL FOREIGN KEY REFERENCES Departments(Id) ON DELETE CASCADE,
    CONSTRAINT PK_UserDepartments PRIMARY KEY (UserId, DepartmentId)
);
GO

-- ====================================
-- Индексы
-- ====================================
CREATE INDEX IX_Users_Login ON Users(Login);
CREATE INDEX IX_Grades_StudentId ON Grades(StudentId);
CREATE INDEX IX_Grades_DisciplineId ON Grades(DisciplineId);
CREATE INDEX IX_Notifications_UserId_IsRead ON Notifications(UserId, IsRead);
GO

-- ====================================
-- Тестовые данные
-- ====================================

-- Факультеты
INSERT INTO Faculties (Name, Code) VALUES
(N'Факультет информационных технологий', N'FIT'),
(N'Инженерный факультет', N'ENG'),
(N'Экономический факультет', N'ECO');
GO

-- Кафедры
INSERT INTO Departments (Name, Code, FacultyId) VALUES
(N'Кафедра программирования', N'PROG', 1),
(N'Кафедра информационных систем', N'IS', 1),
(N'Кафедра автоматизации', N'AUTO', 2),
(N'Кафедра экономики', N'ECON', 3);
GO

-- Группы
INSERT INTO Groups (Name, Year, DepartmentId) VALUES
(N'ИВТ-21', 1, 1),
(N'ИВТ-22', 2, 1),
(N'ИС-21', 1, 2),
(N'АВТ-21', 1, 3),
(N'ЭК-21', 1, 4);
GO

-- Дисциплины
INSERT INTO Disciplines (Name, Code, LectureHours, SeminarHours, LabHours, Credits) VALUES
(N'Программирование на C#', N'CSHARP', 36, 18, 36, 5),
(N'Базы данных', N'DATABASE', 36, 18, 18, 4),
(N'Веб-разработка', N'WEB', 18, 18, 36, 4),
(N'Алгоритмы и структуры данных', N'ALGO', 36, 18, 18, 5),
(N'Экономика предприятия', N'ECONOMICS', 36, 18, 0, 4);
GO

-- Пользователи
-- Администратор
INSERT INTO Users (Login, Password, FullName, Email, Role) VALUES
(N'admin', N'admin123', N'Администратор Системы', N'admin@university.edu', N'Admin');

-- Преподаватели
INSERT INTO Users (Login, Password, FullName, Email, Role, DepartmentId) VALUES
(N'ivanov', N'ivanov123', N'Иванов Иван Иванович', N'ivanov@university.edu', N'Teacher', 1),
(N'petrov', N'petrov123', N'Петров Петр Петрович', N'petrov@university.edu', N'Teacher', 2);

-- Студенты
INSERT INTO Users (Login, Password, FullName, Email, Role, GroupId) VALUES
(N'student1', N'student123', N'Сидоров Алексей Владимирович', N'sidorov@student.edu', N'Student', 1),
(N'student2', N'student123', N'Кузнецова Мария Ивановна', N'kuznetsova@student.edu', N'Student', 1),
(N'student3', N'student123', N'Смирнов Дмитрий Александрович', N'smirnov@student.edu', N'Student', 2),
(N'student4', N'student123', N'Васильева Анна Сергеевна', N'vasilyeva@student.edu', N'Student', 3);
GO

-- Учебный план
INSERT INTO Curricula (GroupId, DisciplineId, Semester, AcademicYear) VALUES
(1, 1, 1, N'2024/2025'), -- ИВТ-21: Программирование на C#
(1, 2, 1, N'2024/2025'), -- ИВТ-21: Базы данных
(1, 4, 1, N'2024/2025'), -- ИВТ-21: Алгоритмы
(2, 1, 2, N'2024/2025'), -- ИВТ-22: Программирование на C#
(2, 3, 2, N'2024/2025'), -- ИВТ-22: Веб-разработка
(3, 2, 1, N'2024/2025'), -- ИС-21: Базы данных
(3, 4, 1, N'2024/2025'); -- ИС-21: Алгоритмы
GO

-- Оценки (пример)
INSERT INTO Grades (StudentId, DisciplineId, Type, Value, Date, Semester, AcademicYear) VALUES
(4, 1, N'Seminar', 4.5, GETUTCDATE(), 1, N'2024/2025'), -- Сидоров: Программирование
(4, 2, N'Lab', 5.0, GETUTCDATE(), 1, N'2024/2025'),      -- Сидоров: Базы данных
(5, 1, N'Seminar', 4.0, GETUTCDATE(), 1, N'2024/2025'), -- Кузнецова: Программирование
(5, 2, N'Lab', 3.5, GETUTCDATE(), 1, N'2024/2025');     -- Кузнецова: Базы данных
GO

-- ====================================
-- Готово!
-- ====================================
PRINT 'База данных UniversityDB успешно создана и заполнена тестовыми данными!';
PRINT '';
PRINT 'Тестовые учетные записи:';
PRINT 'Администратор: admin / admin123';
PRINT 'Преподаватель: ivanov / ivanov123';
PRINT 'Студент: student1 / student123';
GO