using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UniversitySystem.Models;

namespace UniversitySystem.Services;

public static class ExportService
{
    static ExportService()
    {
        // Настройка QuestPDF для работы без лицензии
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static void ExportGradesToExcel(List<Grade> grades, string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Журнал оценок");

            // Заголовки
            var headers = new[] { "ID", "Студент", "Дисциплина", "Тип", "Оценка", "Дата", "Семестр", "Учебный год" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            for (int row = 0; row < grades.Count; row++)
            {
                var g = grades[row];
                worksheet.Cell(row + 2, 1).Value = g.Id;
                worksheet.Cell(row + 2, 2).Value = g.Student?.FullName ?? $"[Студент #{g.StudentId}]";
                worksheet.Cell(row + 2, 3).Value = g.Discipline?.Name ?? $"[Дисциплина #{g.DisciplineId}]";
                worksheet.Cell(row + 2, 4).Value = g.Type ?? "";
                worksheet.Cell(row + 2, 5).Value = g.Value;
                worksheet.Cell(row + 2, 6).Value = g.Date.ToString("dd.MM.yyyy");
                worksheet.Cell(row + 2, 7).Value = g.Semester;
                worksheet.Cell(row + 2, 8).Value = g.AcademicYear ?? "";
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка экспорта в Excel: {ex.Message}", ex);
        }
    }

    public static async Task ExportGradesToExcelAsync(List<Grade> grades, string filePath)
    {
        await Task.Run(() => ExportGradesToExcel(grades, filePath));
    }

    public static void ExportStudentsToExcel(List<User> students, string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Студенты");

            // Заголовки
            var headers = new[] { "ID", "Логин", "ФИО", "Email", "Роль", "Группа", "Кафедра" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            for (int row = 0; row < students.Count; row++)
            {
                var s = students[row];
                worksheet.Cell(row + 2, 1).Value = s.Id;
                worksheet.Cell(row + 2, 2).Value = s.Login ?? "";
                worksheet.Cell(row + 2, 3).Value = s.FullName ?? "";
                worksheet.Cell(row + 2, 4).Value = s.Email ?? "N/A";
                worksheet.Cell(row + 2, 5).Value = s.Role ?? "";
                worksheet.Cell(row + 2, 6).Value = s.Group?.Name ?? "N/A";
                worksheet.Cell(row + 2, 7).Value = s.Department?.Name ?? "N/A";
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка экспорта в Excel: {ex.Message}", ex);
        }
    }

    public static void ExportStatsToExcel(string groupName, int studentCount, decimal average, int excellent, int good, int satisfactory, int fail, string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Статистика группы");
            
            ws.Cell(1, 1).Value = "Статистика группы";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            ws.Range(1, 1, 1, 2).Merge();

            ws.Cell(3, 1).Value = "Группа:";
            ws.Cell(3, 2).Value = groupName;
            ws.Cell(4, 1).Value = "Количество студентов:";
            ws.Cell(4, 2).Value = studentCount;
            ws.Cell(5, 1).Value = "Средний балл:";
            ws.Cell(5, 2).Value = average.ToString("F2");
            ws.Cell(7, 1).Value = "Отлично (5):";
            ws.Cell(7, 2).Value = excellent;
            ws.Cell(8, 1).Value = "Хорошо (4):";
            ws.Cell(8, 2).Value = good;
            ws.Cell(9, 1).Value = "Удовлетворительно (3):";
            ws.Cell(9, 2).Value = satisfactory;
            ws.Cell(10, 1).Value = "Неудовлетворительно (2):";
            ws.Cell(10, 2).Value = fail;

            workbook.SaveAs(filePath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка экспорта в Excel: {ex.Message}", ex);
        }
    }

    public static void ExportScheduleToExcel(List<Schedule> schedules, string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Расписание");

            // Заголовки
            var headers = new[] { "День", "Время", "Дисциплина", "Группа", "Кафедра", "Аудитория", "Тип" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            for (int row = 0; row < schedules.Count; row++)
            {
                var s = schedules[row];
                worksheet.Cell(row + 2, 1).Value = GetDayName(s.DayOfWeek);
                worksheet.Cell(row + 2, 2).Value = s.StartTime.ToString("HH:mm");
                worksheet.Cell(row + 2, 3).Value = s.Discipline?.Name ?? $"[Дисциплина #{s.DisciplineId}]";
                worksheet.Cell(row + 2, 4).Value = s.Group?.Name ?? $"[Группа #{s.GroupId}]";
                worksheet.Cell(row + 2, 5).Value = s.Group?.Department?.Name ?? "N/A";
                worksheet.Cell(row + 2, 6).Value = s.Room ?? "N/A";
                worksheet.Cell(row + 2, 7).Value = s.Type ?? "Lecture";
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка экспорта в Excel: {ex.Message}", ex);
        }
    }

    public static void ExportExamSheet(int groupId, int disciplineId, string disciplineName, string groupName, List<User> students, List<Grade> grades, string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Ведомость");

            ws.Cell(1, 1).Value = "ЭКЗАМЕНАЦИОННАЯ ВЕДОМОСТЬ";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Range(1, 1, 1, 5).Merge();

            ws.Cell(2, 1).Value = $"Дисциплина: {disciplineName}";
            ws.Cell(3, 1).Value = $"Группа: {groupName}";
            ws.Cell(4, 1).Value = $"Дата: {DateTime.Now:dd.MM.yyyy}";

            int row = 6;
            ws.Cell(row, 1).Value = "№";
            ws.Cell(row, 2).Value = "ФИО студента";
            ws.Cell(row, 3).Value = "Оценка";
            ws.Cell(row, 4).Value = "Подпись";
            ws.Cell(row, 5).Value = "Примечание";

            for (int i = 0; i < 5; i++)
                ws.Cell(row, i + 1).Style.Font.Bold = true;

            row = 7;
            for (int i = 0; i < students.Count; i++)
            {
                var student = students[i];
                var examGrade = grades.FirstOrDefault(g => g.StudentId == student.Id && g.Type == "Exam");
                
                ws.Cell(row, 1).Value = i + 1;
                ws.Cell(row, 2).Value = student.FullName ?? "";
                ws.Cell(row, 3).Value = examGrade?.Value.ToString() ?? "";
                ws.Cell(row, 4).Value = "";
                ws.Cell(row, 5).Value = "";
                row++;
            }

            row++;
            ws.Cell(row, 1).Value = "Подпись преподавателя: ____________________";

            ws.Column(1).Width = 5;
            ws.Column(2).Width = 30;
            ws.Column(3).Width = 10;
            ws.Column(4).Width = 15;
            ws.Column(5).Width = 15;

            workbook.SaveAs(filePath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка экспорта в Excel: {ex.Message}", ex);
        }
    }

    // ==================== PDF ЭКСПОРТЫ ====================

    public static void ExportGradesToPdf(List<Grade> grades, string filePath)
    {
        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("ЖУРНАЛ ОЦЕНОК")
                        .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium)
                        .AlignCenter();

                    page.Content()
                        .PaddingVertical(10)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("#").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Студент").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Дисциплина").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Тип").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Оценка").SemiBold();
                            });

                            for (int i = 0; i < grades.Count; i++)
                            {
                                var g = grades[i];
                                var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                table.Cell().Background(bgColor).Padding(5).Text((i + 1).ToString());
                                table.Cell().Background(bgColor).Padding(5).Text(g.Student?.FullName ?? $"[Студент #{g.StudentId}]");
                                table.Cell().Background(bgColor).Padding(5).Text(g.Discipline?.Name ?? $"[Дисциплина #{g.DisciplineId}]");
                                table.Cell().Background(bgColor).Padding(5).Text(g.Type ?? "");
                                table.Cell().Background(bgColor).Padding(5).Text(g.Value.ToString());
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Страница ");
                            text.CurrentPageNumber();
                            text.Span(" из ");
                            text.TotalPages();
                        });
                });
            });

            document.GeneratePdf(filePath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка экспорта в PDF: {ex.Message}", ex);
        }
    }

    public static void ExportScheduleToPdf(List<Schedule> schedules, string filePath)
    {
        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("РАСПИСАНИЕ ЗАНЯТИЙ")
                        .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium)
                        .AlignCenter();

                    page.Content()
                        .PaddingVertical(10)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("День").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Время").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Дисциплина").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Группа").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Аудитория").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Тип").SemiBold();
                            });

                            for (int i = 0; i < schedules.Count; i++)
                            {
                                var s = schedules[i];
                                var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                table.Cell().Background(bgColor).Padding(5).Text(GetDayName(s.DayOfWeek));
                                table.Cell().Background(bgColor).Padding(5).Text(s.StartTime.ToString("HH:mm"));
                                table.Cell().Background(bgColor).Padding(5).Text(s.Discipline?.Name ?? "N/A");
                                table.Cell().Background(bgColor).Padding(5).Text(s.Group?.Name ?? "N/A");
                                table.Cell().Background(bgColor).Padding(5).Text(s.Room ?? "N/A");
                                table.Cell().Background(bgColor).Padding(5).Text(s.Type ?? "Lecture");
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Страница ");
                            text.CurrentPageNumber();
                            text.Span(" из ");
                            text.TotalPages();
                        });
                });
            });

            document.GeneratePdf(filePath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка экспорта в PDF: {ex.Message}", ex);
        }
    }

    public static void ExportStatsToPdf(int facultyCount, int departmentCount, int groupCount, int studentCount, int teacherCount, int disciplineCount, string filePath)
    {
        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("СТАТИСТИКА СИСТЕМЫ")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium)
                        .AlignCenter();

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            column.Item().Text("University System").FontSize(14).SemiBold();
                            column.Item().Text($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10).Italic();

                            column.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                            column.Item().PaddingTop(20).Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Общая статистика").SemiBold().FontSize(14);
                                    col.Item().PaddingTop(10).Text($"Факультеты: {facultyCount}");
                                    col.Item().Text($"Кафедры: {departmentCount}");
                                    col.Item().Text($"Группы: {groupCount}");
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Пользователи").SemiBold().FontSize(14);
                                    col.Item().PaddingTop(10).Text($"Студенты: {studentCount}");
                                    col.Item().Text($"Преподаватели: {teacherCount}");
                                    col.Item().Text($"Всего: {studentCount + teacherCount}");
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Учебный процесс").SemiBold().FontSize(14);
                                    col.Item().PaddingTop(10).Text($"Дисциплины: {disciplineCount}");
                                });
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Страница ");
                            text.CurrentPageNumber();
                            text.Span(" из ");
                            text.TotalPages();
                        });
                });
            });

            document.GeneratePdf(filePath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка экспорта в PDF: {ex.Message}", ex);
        }
    }

    private static string GetDayName(int day)
    {
        return day switch
        {
            1 => "Понедельник",
            2 => "Вторник",
            3 => "Среда",
            4 => "Четверг",
            5 => "Пятница",
            6 => "Суббота",
            _ => "Неизвестно"
        };
    }
}
