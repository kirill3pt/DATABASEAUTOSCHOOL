
using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleAdminWindow
    {
        private DateTime examCalendarMonth = DateTime.Today;
        private DateTime? selectedExamDate = DateTime.Today;

        private readonly CultureInfo russianCulture =
            new CultureInfo("ru-RU");

        // Открытие календаря
        private void ExamCalendarToggleButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!selectedExamDate.HasValue)
                selectedExamDate = DateTime.Today;

            examCalendarMonth = selectedExamDate.Value;

            DrawExamCalendar();
            ExamCalendarPopup.IsOpen = !ExamCalendarPopup.IsOpen;
        }

        // Предыдущий месяц
        private void ExamPreviousMonth_Click(
            object sender,
            RoutedEventArgs e)
        {
            examCalendarMonth = examCalendarMonth.AddMonths(-1);
            DrawExamCalendar();
        }

        // Следующий месяц
        private void ExamNextMonth_Click(
            object sender,
            RoutedEventArgs e)
        {
            examCalendarMonth = examCalendarMonth.AddMonths(1);
            DrawExamCalendar();
        }

        // Отрисовка календаря
        private void DrawExamCalendar()
        {
            ExamCalendarMonthText.Text =
                examCalendarMonth.ToString("MMMM yyyy", russianCulture);

            ExamCalendarDaysPanel.Children.Clear();

            DateTime firstDay = new DateTime(
                examCalendarMonth.Year,
                examCalendarMonth.Month,
                1);

            int offset = ((int)firstDay.DayOfWeek + 6) % 7;

            DateTime calendarStart = firstDay.AddDays(-offset);

            for (int i = 0; i < 42; i++)
            {
                DateTime date = calendarStart.AddDays(i);

                Button dayButton = new Button
                {
                    Content = date.Day.ToString(),
                    Tag = date,
                    Height = 36,
                    Margin = new Thickness(2),
                    FontSize = 13,
                    FontWeight = FontWeights.Normal,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Style = (Style)FindResource("CalendarButtonStyle"),
                    Background = Brushes.Transparent,
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(41, 51, 48))
                };

                if (selectedExamDate.HasValue &&
                    selectedExamDate.Value.Date == date.Date)
                {
                    dayButton.Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#A8B5A5"));

                    dayButton.FontWeight = FontWeights.SemiBold;
                }

                dayButton.Click += ExamCalendarDay_Click;

                ExamCalendarDaysPanel.Children.Add(dayButton);
            }
        }

        // Выбор даты
        private void ExamCalendarDay_Click(
            object sender,
            RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
                return;

            selectedExamDate = (DateTime)button.Tag;

            ExamDateText.Text =
                selectedExamDate.Value.ToString("dd.MM.yyyy");

            ExamDateText.Foreground =
                new SolidColorBrush(Color.FromRgb(41, 51, 48));

            examCalendarMonth = selectedExamDate.Value;

            ExamCalendarPopup.IsOpen = false;
        }

        // Загрузка групп для экзаменационного списка
        private void LoadExamGroups()
        {
            try
            {
                object selectedGroupId = ExamGroupComboBox.SelectedValue;

                DataTable groups = groupService.GetGroups();

                ExamGroupComboBox.ItemsSource = groups.DefaultView;

                if (selectedGroupId != null)
                    ExamGroupComboBox.SelectedValue = selectedGroupId;
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка загрузки групп для экзамена: " + ex.Message);

                MessageBox.Show(
                    "Не удалось загрузить список групп.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Переключение на аналитику
        private void ReportsAnalytics_Click(
            object sender,
            RoutedEventArgs e)
        {
            ReportsAnalyticsPanel.Visibility = Visibility.Visible;
            ReportsExamPanel.Visibility = Visibility.Collapsed;

            ReportsAnalyticsButton.Style =
                (Style)FindResource("AdminActionButtonStyle");

            ReportsExamButton.Style =
                (Style)FindResource("LightActionButtonStyle");
        }

        // Переключение на экзаменационный список
        private void ReportsExam_Click(
            object sender,
            RoutedEventArgs e)
        {
            ReportsAnalyticsPanel.Visibility = Visibility.Collapsed;
            ReportsExamPanel.Visibility = Visibility.Visible;

            ReportsAnalyticsButton.Style =
                (Style)FindResource("LightActionButtonStyle");

            ReportsExamButton.Style =
                (Style)FindResource("AdminActionButtonStyle");

            if (!selectedExamDate.HasValue)
                selectedExamDate = DateTime.Today;

            ExamDateText.Text =
                selectedExamDate.Value.ToString("dd.MM.yyyy");

            examCalendarMonth = selectedExamDate.Value;

            LoadExamGroups();
        }

        // Формирование списка учеников выбранной группы
        private void GenerateExamList_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ExamGroupComboBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Сначала выберите учебную группу.",
                    "Экзаменационный список",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            if (!selectedExamDate.HasValue)
            {
                MessageBox.Show(
                    "Укажите дату экзамена.",
                    "Экзаменационный список",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            try
            {
                int groupId =
                    Convert.ToInt32(ExamGroupComboBox.SelectedValue);

                DataRowView selectedGroup =
                    ExamGroupComboBox.SelectedItem as DataRowView;

                string groupName = selectedGroup != null
                    ? Convert.ToString(selectedGroup["Название группы"])
                    : "Учебная группа";

                DataTable students = new DataTable();

                students.Columns.Add("№", typeof(int));
                students.Columns.Add("ФИО", typeof(string));
                students.Columns.Add("Подпись", typeof(string));

                using (SqlConnection connection = DB.GetConnection())
                {
                    string query = @"
                        SELECT
                            LastName,
                            FirstName,
                            MiddleName
                        FROM dbo.Students
                        WHERE GroupID = @GroupID
                        ORDER BY LastName, FirstName, MiddleName";

                    using (SqlCommand command =
                        new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@GroupID", SqlDbType.Int)
                            .Value = groupId;

                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            int number = 1;

                            while (reader.Read())
                            {
                                string lastName =
                                    Convert.ToString(reader["LastName"]);

                                string firstName =
                                    Convert.ToString(reader["FirstName"]);

                                string middleName =
                                    reader["MiddleName"] == DBNull.Value
                                        ? ""
                                        : Convert.ToString(reader["MiddleName"]);

                                string fio =
                                    (lastName + " " + firstName + " " + middleName)
                                    .Trim();

                                students.Rows.Add(number, fio, "");

                                number++;
                            }
                        }
                    }
                }

                if (students.Rows.Count == 0)
                {
                    ExamStudentsGrid.ItemsSource = null;
                    ExamStudentsGrid.Visibility = Visibility.Collapsed;
                    ExamEmptyState.Visibility = Visibility.Visible;
                    PrintExamListButton.Visibility = Visibility.Collapsed;
                    ExamStudentsCountText.Text = "0 учеников";

                    MessageBox.Show(
                        "В выбранной группе пока нет учеников.",
                        "Экзаменационный список",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                ExamStudentsGrid.ItemsSource = students.DefaultView;

                ExamListTitle.Text =
                    "Экзаменационный список — " + groupName;

                ExamListDate.Text =
                    "Дата экзамена: " +
                    selectedExamDate.Value.ToString("dd.MM.yyyy");

                ExamStudentsCountText.Text =
                    students.Rows.Count + " учеников";

                ExamEmptyState.Visibility = Visibility.Collapsed;
                ExamStudentsGrid.Visibility = Visibility.Visible;
                PrintExamListButton.Visibility = Visibility.Visible;

                logger.LOG(
                    "Сформирован экзаменационный список группы " +
                    groupName + ". Количество учеников: " +
                    students.Rows.Count);
            }
            catch (Exception ex)
            {
                logger.LOG(
                    "Ошибка формирования экзаменационного списка: " +
                    ex.Message);

                MessageBox.Show(
                    "Не удалось сформировать экзаменационный список.\n\n" +
                    ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        // Печать экзаменационного списка
        private void PrintExamList_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ExamStudentsGrid.ItemsSource == null ||
                ExamStudentsGrid.Items.Count == 0)
            {
                MessageBox.Show(
                    "Сначала сформируйте экзаменационный список.",
                    "Печать",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            try
            {
                DataView dataView = ExamStudentsGrid.ItemsSource as DataView;

                if (dataView == null || dataView.Count == 0)
                {
                    MessageBox.Show(
                        "Нет данных для печати.",
                        "Печать",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                PrintDialog printDialog = new PrintDialog();

                if (printDialog.ShowDialog() != true)
                    return;

                double pageWidth = printDialog.PrintableAreaWidth;
                double pageHeight = printDialog.PrintableAreaHeight;

                Thickness pagePadding = new Thickness(55, 35, 65, 35);

                FlowDocument document = new FlowDocument
                {
                    PageWidth = pageWidth,
                    PageHeight = pageHeight,
                    PagePadding = pagePadding,
                    FontFamily = new FontFamily("Times New Roman"),
                    FontSize = 12,
                    ColumnWidth = double.PositiveInfinity,
                    TextAlignment = TextAlignment.Left
                };

                // Заголовок
                Paragraph title = new Paragraph(
                    new Run("ЭКЗАМЕНАЦИОННАЯ ВЕДОМОСТЬ"))
                {
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 18)
                };

                document.Blocks.Add(title);

                // Название группы
                Paragraph group = new Paragraph(
                    new Run(ExamListTitle.Text))
                {
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 6)
                };

                document.Blocks.Add(group);

                // Дата экзамена
                Paragraph date = new Paragraph(
                    new Run(ExamListDate.Text))
                {
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 20)
                };

                document.Blocks.Add(date);

                // Таблица
                double tableWidth =
                    pageWidth - pagePadding.Left - pagePadding.Right - 10;

                double numberColumnWidth = 45;
                double nameColumnWidth = tableWidth * 0.55;
                double signatureColumnWidth =
                    tableWidth - numberColumnWidth - nameColumnWidth;

                Table table = new Table
                {
                    CellSpacing = 0,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.5)
                };

                table.Columns.Add(new TableColumn
                {
                    Width = new GridLength(numberColumnWidth)
                });

                table.Columns.Add(new TableColumn
                {
                    Width = new GridLength(nameColumnWidth)
                });

                table.Columns.Add(new TableColumn
                {
                    Width = new GridLength(signatureColumnWidth)
                });

                TableRowGroup rows = new TableRowGroup();

                // Заголовок таблицы
                TableRow header = new TableRow();

                AddExamPrintCell(
                    header,
                    "№",
                    true,
                    TextAlignment.Center);

                AddExamPrintCell(
                    header,
                    "ФИО ученика",
                    true,
                    TextAlignment.Center);

                AddExamPrintCell(
                    header,
                    "Подпись",
                    true,
                    TextAlignment.Center);

                rows.Rows.Add(header);

                // Ученики
                foreach (DataRowView student in dataView)
                {
                    TableRow row = new TableRow();

                    AddExamPrintCell(
                        row,
                        Convert.ToString(student["№"]),
                        false,
                        TextAlignment.Center);

                    AddExamPrintCell(
                        row,
                        Convert.ToString(student["ФИО"]),
                        false,
                        TextAlignment.Left);

                    AddExamPrintCell(
                        row,
                        "",
                        false,
                        TextAlignment.Left);

                    rows.Rows.Add(row);
                }

                table.RowGroups.Add(rows);
                document.Blocks.Add(table);

                // Подпись экзаменатора
                Paragraph signature = new Paragraph(
                    new Run("Подпись экзаменатора: __________________________"))
                {
                    FontSize = 12,
                    Margin = new Thickness(0, 30, 0, 0)
                };

                document.Blocks.Add(signature);

                IDocumentPaginatorSource paginator = document;

                paginator.DocumentPaginator.PageSize =
                    new Size(pageWidth, pageHeight);

                printDialog.PrintDocument(
                    paginator.DocumentPaginator,
                    "Экзаменационная ведомость");

                logger.LOG(
                    "Отправлен на печать экзаменационный список.");
            }
            catch (Exception ex)
            {
                logger.LOG(
                    "Ошибка печати экзаменационного списка: " +
                    ex.Message);

                MessageBox.Show(
                    "Не удалось напечатать список.\n\n" + ex.Message,
                    "Ошибка печати",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        // Добавление ячейки в печатную таблицу
        private void AddExamPrintCell(
            TableRow row,
            string value,
            bool isHeader = false,
            TextAlignment alignment = TextAlignment.Left)
        {
            Paragraph paragraph = new Paragraph(new Run(value))
            {
                Margin = new Thickness(0),
                FontSize = 12,
                FontWeight = isHeader
                    ? FontWeights.Bold
                    : FontWeights.Normal,
                TextAlignment = alignment
            };

            TableCell cell = new TableCell(paragraph)
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5),
                Padding = new Thickness(8, 10, 8, 10),
                TextAlignment = alignment
            };

            if (isHeader)
            {
                cell.Background = new SolidColorBrush(
                    Color.FromRgb(230, 230, 230));
            }

            row.Cells.Add(cell);
        }
    }
}
