using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace hw1
{
    public partial class MainWindow : Window
    {
        // Список студентов будет храниться на уровне окна
        // т.е. во вкладке Дипломы
        private List<Student> _students = new List<Student>();
        private readonly string _studentsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "students.json");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            double? aValue = EvaluateExpression(TextBoxA.Text);
            double? rValue = EvaluateExpression(TextBoxR.Text);

            if (aValue == null)
            {
                MessageBox.Show("Неверное значение A. Введите число или выражение (например '3.5' или '2*Pi').", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (rValue == null)
            {
                MessageBox.Show("Неверное значение R. Введите число или выражение (например '1.5' или 'Pi/2').", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double a = aValue.Value;
            double r = rValue.Value;

            if (a <= 0 || r <= 0)
            {
                MessageBox.Show("A и R должны быть положительными числами.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (r > a / 2.0)
            {
                MessageBox.Show("Радиус R не должен превышать половину ребра куба (a/2).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double volumeOfCube = Math.Pow(a, 3);
            double volumeOfSphere = (4.0 / 3.0) * Math.PI * Math.Pow(r, 3);
            double wastePercentage = 0;
            if (volumeOfCube > 0)
                wastePercentage = ((volumeOfCube - volumeOfSphere) / volumeOfCube) * 100.0;

            ResultTextBlock.Text = $"Объём куба:  {volumeOfCube:F4}\n" +
                                   $"Объём шара: {volumeOfSphere:F4}\n" +
                                   $"Процент отходов: {wastePercentage:F2}%";
        }

        // Парсер выражений, теперь ввод будет поддерживать + - * / и скобки (а также подмену Pi)
        private double? EvaluateExpression(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            string piStr = Math.PI.ToString(CultureInfo.InvariantCulture);
            string expr = input.Trim()
                               .Replace("π", piStr)
                               .Replace("PI", piStr)
                               .Replace("Pi", piStr)
                               .Replace("pi", piStr);

            // Заменим запятую на точку
            expr = expr.Replace(',', '.');

            try
            {
                var table = new DataTable();
                table.Locale = CultureInfo.InvariantCulture;
                object value = table.Compute(expr, string.Empty);
                if (value == null) return null;
                if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                    return result;
            }
            catch
            {
                // При ошибке парсинга возвращается null
                return null;
            }
            return null;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            _students = DataGenerator.GenerateStudents(30);
            StudentsDataGrid.ItemsSource = null;
            StudentsDataGrid.ItemsSource = _students;
            FilterResultTextBlock.Text = $"Сгенерировано {_students.Count} студентов. Файл: {_studentsFilePath}";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataStorage.SaveToJson(_students, _studentsFilePath);
                MessageBox.Show($"Сохранено {_students.Count} студентов в:\n{_studentsFilePath}", "Сохранено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _students = DataStorage.LoadFromJson<Student>(_studentsFilePath);
                StudentsDataGrid.ItemsSource = null;
                StudentsDataGrid.ItemsSource = _students;
                FilterResultTextBlock.Text = $"Загружено {_students.Count} студентов из {_studentsFilePath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            string group = GroupFilterTextBox.Text.Trim();
            string gradeText = GradeFilterTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(group) || string.IsNullOrWhiteSpace(gradeText))
            {
                MessageBox.Show("Укажите и группу, и оценку для фильтра.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!int.TryParse(gradeText, out int grade))
            {
                MessageBox.Show("Оценка должна быть целым числом (напр., 2..5).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var filtered = _students
                .Where(s => s.Group.Equals(group, StringComparison.OrdinalIgnoreCase) && s.Grade == grade)
                .ToList();

            StudentsDataGrid.ItemsSource = null;
            StudentsDataGrid.ItemsSource = filtered;

            if (filtered.Any())
            {
                FilterResultTextBlock.Text = $"Найдено {filtered.Count} студентов (Группа={group}, Оценка={grade}).";
            }
            else
            {
                FilterResultTextBlock.Text = $"Нет студентов в группе '{group}' с оценкой {grade}.";
            }
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            GroupFilterTextBox.Text = "";
            GradeFilterTextBox.Text = "";
            StudentsDataGrid.ItemsSource = null;
            StudentsDataGrid.ItemsSource = _students;
            FilterResultTextBlock.Text = $"Фильтр сброшен. Всего {_students.Count} студентов.";
        }
    }

    public class Student
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Group { get; set; } = "";
        public int Grade { get; set; }
    }

    public static class DataGenerator
    {
        static readonly string[] firstNames = { "Иван", "Александр", "Ольга", "Мария", "Дмитрий", "Алексей", "Елена", "Сергей" };
        static readonly string[] lastNames = { "Иванов", "Петров", "Сидоров", "Кузнецов", "Смирнов", "Попов", "Лебедев" };
        static readonly string[] groups = { "ПИ-241", "ФИТ-241", "ПМИ-241", "МОА-241", "КБ-241" };

        public static List<Student> GenerateStudents(int count)
        {
            var rnd = new Random();
            var list = new List<Student>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new Student
                {
                    FirstName = firstNames[rnd.Next(firstNames.Length)],
                    LastName = lastNames[rnd.Next(lastNames.Length)],
                    Group = groups[rnd.Next(groups.Length)],
                    Grade = rnd.Next(2, 6) // оценки 2..5
                });
            }
            return list;
        }
    }

    public static class DataStorage
    {
        private static readonly JsonSerializerOptions _opts = new JsonSerializerOptions { WriteIndented = true };

        public static void SaveToJson<T>(List<T> list, string path)
        {
            string json = JsonSerializer.Serialize(list, _opts);
            File.WriteAllText(path, json);
        }

        public static List<T> LoadFromJson<T>(string path)
        {
            if (!File.Exists(path)) return new List<T>();
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<T>>(json, _opts) ?? new List<T>();
        }
    }
}
