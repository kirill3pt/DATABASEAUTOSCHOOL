
using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleInstructorWindow : Window
    {
        private readonly int instructorId;

        private readonly InstructorDashboardService dashboardService = new InstructorDashboardService();

        private readonly LessonService lessonService = new LessonService();
        private readonly InstructorStudentsService studentsService = new InstructorStudentsService();
        private readonly InstructorProfileService profileService = new InstructorProfileService();

        private DateTime currentCalendarMonth = DateTime.Today;
        private DateTime? selectedCalendarDate;

        private DataTable instructorLessons = new DataTable();

        public ModuleInstructorWindow(int instructorId)
        {
            InitializeComponent();

            this.instructorId = instructorId;

            Dashboard_Click(null, null);
        }

        // Выделение активного пункта меню
        private void SetActiveButton(Button activeButton)
        {
            Button[] buttons =
            {
                DashboardButton,
                LessonsButton,
                StudentsButton,
                ProfileButton
            };

            foreach (Button button in buttons)
            {
                button.Background = Brushes.Transparent;
                button.Foreground = new SolidColorBrush(
                    Color.FromRgb(216, 222, 216));
            }

            if (activeButton != null)
            {
                activeButton.Background = new SolidColorBrush(
                    Color.FromRgb(168, 181, 165));

                activeButton.Foreground = new SolidColorBrush(
                    Color.FromRgb(41, 51, 48));
            }
        }

        // Главная
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(DashboardButton);

            DashboardPanel.Visibility = Visibility.Visible;
            SectionPanel.Visibility = Visibility.Collapsed;

            LoadDashboardStats();
        }

        // Переключение содержимого разделов
        private void ShowSection(string title, string description, Button activeButton, UIElement contentPanel)
        {
            SetActiveButton(activeButton);

            DashboardPanel.Visibility = Visibility.Collapsed;
            SectionPanel.Visibility = Visibility.Visible;

            SectionTitle.Text = title;
            SectionDescription.Text = description;

            CalendarContentPanel.Visibility = Visibility.Collapsed;
            StudentsContentPanel.Visibility = Visibility.Collapsed;
            ProfileContentPanel.Visibility = Visibility.Collapsed;

            contentPanel.Visibility = Visibility.Visible;
        }

        // Расписание
        // Расписание
        private void Lessons_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(
                "Моё расписание",
                "Просмотр запланированных занятий и информации о них.",
                LessonsButton,
                CalendarContentPanel);

            LoadInstructorCalendar();
        }

        // Ученики
        private void Students_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(
                "Мои ученики",
                "Просмотр информации об учениках, закреплённых за вами.",
                StudentsButton,
                StudentsContentPanel);

            LoadInstructorStudents();
        }

        private void InstructorStudentSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(InstructorStudentsGrid.ItemsSource is DataView view))
                return;

            string search = InstructorStudentSearchBox.Text.Trim()
                .Replace("'", "''")
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("*", "[*]");

            if (string.IsNullOrWhiteSpace(search))
            {
                view.RowFilter = "";
                return;
            }

            view.RowFilter =
                $"Convert([ID], 'System.String') LIKE '%{search}%' OR " +
                $"[ФИО] LIKE '%{search}%' OR " +
                $"[Телефон] LIKE '%{search}%' OR " +
                $"[Группа] LIKE '%{search}%'";
        }
        private void RefreshInstructorStudents_Click(object sender, RoutedEventArgs e)
        {
            LoadInstructorStudents();
        }

        // Профиль


        private void LoadInstructorProfile()
        {
            try
            {
                DataTable profile = profileService.GetProfile(instructorId);

                if (profile.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "Не удалось найти данные профиля.",
                        "Профиль",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                DataRow row = profile.Rows[0];

                InstructorProfileNameText.Text =
                    row["ФИО"].ToString();

                InstructorProfilePhoneText.Text =
                    row["Телефон"] == DBNull.Value
                        ? "Не указан"
                        : row["Телефон"].ToString();

                InstructorProfileLoginText.Text =
                    row["Логин"].ToString();

                bool isActive = Convert.ToBoolean(row["Активен"]);

                InstructorProfileStatusText.Text =
                    isActive ? "Активна" : "Заблокирована";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить профиль.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(
                "Мой профиль",
                "Просмотр личной информации инструктора.",
                ProfileButton,
                ProfileContentPanel);

            LoadInstructorProfile();
        }

        private void RefreshInstructorProfile_Click(object sender, RoutedEventArgs e)
        {
            LoadInstructorProfile();
        }

        // Выход
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Вы действительно хотите выйти из аккаунта?",
                "Выход из системы",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                Close();
            }
        }

        // Статистика главной страницы
        private void LoadDashboardStats()
        {
            try
            {
                DataTable stats =
                    dashboardService.GetDashboardStats(instructorId);

                if (stats.Rows.Count > 0)
                {
                    StudentsCountText.Text =
                        stats.Rows[0]["StudentCount"].ToString();

                    LessonsCountText.Text =
                        stats.Rows[0]["LessonCount"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить статистику.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Загрузка занятий за выбранный месяц
        private void LoadInstructorCalendar()
        {
            try
            {
                instructorLessons =
                    lessonService.GetInstructorLessonsByMonth(
                        instructorId,
                        currentCalendarMonth);

                CalendarMonthText.Text =
                    currentCalendarMonth.ToString(
                        "MMMM yyyy",
                        new CultureInfo("ru-RU"));

                BuildCalendar();

                if (selectedCalendarDate.HasValue &&
                    selectedCalendarDate.Value.Month ==
                        currentCalendarMonth.Month &&
                    selectedCalendarDate.Value.Year ==
                        currentCalendarMonth.Year)
                {
                    ShowSelectedDay(selectedCalendarDate.Value);
                }
                else
                {
                    selectedCalendarDate = null;

                    SelectedDateText.Text = "Выберите дату";
                    SelectedDayCountText.Text = "Занятий: 0";

                    SelectedDayLessonsPanel.Children.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить календарь.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Построение календаря
        private void BuildCalendar()
        {
            CalendarDaysGrid.Children.Clear();

            DateTime firstDay = new DateTime(
                currentCalendarMonth.Year,
                currentCalendarMonth.Month,
                1);

            int offset = ((int)firstDay.DayOfWeek + 6) % 7;

            DateTime calendarStart = firstDay.AddDays(-offset);

            for (int i = 0; i < 42; i++)
            {
                DateTime date = calendarStart.AddDays(i);

                Border dayBorder = new Border
                {
                    BorderBrush = (Brush)FindResource("BorderColor"),
                    BorderThickness = new Thickness(0.5),
                    Background = (Brush)FindResource("SurfaceColor"),
                    Padding = new Thickness(5),
                    Margin = new Thickness(1),
                    Tag = date,
                    Cursor = Cursors.Hand
                };

                StackPanel dayContent = new StackPanel();

                TextBlock dayNumber = new TextBlock
                {
                    Text = date.Day.ToString(),
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,

                    Foreground = date.Month == currentCalendarMonth.Month
                        ? (Brush)FindResource("TextColor")
                        : (Brush)FindResource("SecondaryTextColor")
                };

                dayContent.Children.Add(dayNumber);

                int lessonCount = 0;

                foreach (DataRow row in instructorLessons.Rows)
                {
                    DateTime lessonDate =
                        Convert.ToDateTime(row["Дата"]);

                    if (lessonDate.Date != date.Date)
                        continue;

                    lessonCount++;

                    if (lessonCount <= 3)
                    {
                        string status = row["Статус"].ToString();

                        TextBlock lessonText = new TextBlock
                        {
                            Text = row["Время"] + " " + row["Ученик"],
                            FontSize = 9,
                            TextTrimming =
                                TextTrimming.CharacterEllipsis,
                            Margin = new Thickness(0, 3, 0, 0),
                            Foreground = GetLessonForeground(status),

                            ToolTip = row["Время"] + " — " +
                                      row["Ученик"] + "\n" +
                                      row["Автомобиль"] + "\n" +
                                      status
                        };

                        dayContent.Children.Add(lessonText);
                    }
                }

                if (lessonCount > 3)
                {
                    dayContent.Children.Add(new TextBlock
                    {
                        Text = "+" + (lessonCount - 3) + " ещё",
                        FontSize = 9,
                        Foreground =
                            (Brush)FindResource("SecondaryTextColor"),
                        Margin = new Thickness(0, 2, 0, 0)
                    });
                }

                dayBorder.Child = dayContent;

                if (date.Date == DateTime.Today)
                {
                    dayBorder.BorderBrush =
                        (Brush)FindResource("AccentColor");

                    dayBorder.BorderThickness =
                        new Thickness(1.5);
                }

                if (selectedCalendarDate.HasValue &&
                    date.Date == selectedCalendarDate.Value.Date)
                {
                    dayBorder.Background =
                        (Brush)FindResource("MutedSurfaceColor");
                }

                dayBorder.MouseLeftButtonUp += CalendarDay_Click;

                CalendarDaysGrid.Children.Add(dayBorder);
            }
        }

        // Цвет занятия в зависимости от статуса
        private Brush GetLessonForeground(string status)
        {
            if (status == "проведено")
                return new SolidColorBrush(
                    Color.FromRgb(105, 139, 111));

            if (status == "отменено")
                return new SolidColorBrush(
                    Color.FromRgb(169, 109, 96));

            return new SolidColorBrush(
                Color.FromRgb(73, 112, 91));
        }

        // Выбор дня календаря
        private void CalendarDay_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            Border dayBorder = sender as Border;

            if (dayBorder == null)
                return;

            DateTime date = (DateTime)dayBorder.Tag;

            if (date.Month != currentCalendarMonth.Month ||
                date.Year != currentCalendarMonth.Year)
                return;

            ShowSelectedDay(date);
            BuildCalendar();
        }

        // Отображение занятий выбранного дня
        private void ShowSelectedDay(DateTime date)
        {
            selectedCalendarDate = date.Date;

            SelectedDateText.Text = date.ToString(
                "d MMMM yyyy, dddd",
                new CultureInfo("ru-RU"));

            SelectedDayLessonsPanel.Children.Clear();

            int count = 0;

            foreach (DataRow row in instructorLessons.Rows)
            {
                DateTime lessonDate = Convert.ToDateTime(row["Дата"]);

                if (lessonDate.Date != date.Date)
                    continue;

                count++;

                int lessonId = Convert.ToInt32(row["ID"]);
                string status = row["Статус"].ToString();

                TimeSpan lessonTime =
                    TimeSpan.Parse(row["Время"].ToString());

                DateTime lessonStart = lessonDate.Date.Add(lessonTime);

                Border lessonBorder = new Border
                {
                    Background = (Brush)FindResource("SurfaceColor"),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 8),
                    BorderBrush = (Brush)FindResource("BorderColor"),
                    BorderThickness = new Thickness(1)
                };

                StackPanel content = new StackPanel();

                content.Children.Add(new TextBlock
                {
                    Text = row["Время"].ToString(),
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (Brush)FindResource("TextColor")
                });

                content.Children.Add(new TextBlock
                {
                    Text = row["Ученик"].ToString(),
                    FontSize = 12,
                    Foreground = (Brush)FindResource("TextColor"),
                    Margin = new Thickness(0, 5, 0, 0)
                });

                content.Children.Add(new TextBlock
                {
                    Text = row["Автомобиль"] + " (" +
                           row["Госномер"] + ")",

                    FontSize = 11,
                    Foreground = (Brush)FindResource("SecondaryTextColor"),
                    Margin = new Thickness(0, 3, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                });

                content.Children.Add(new TextBlock
                {
                    Text = status,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = GetLessonForeground(status),
                    Margin = new Thickness(0, 6, 0, 0)
                });

                if (status == "запланировано")
                {
                    WrapPanel buttonsPanel = new WrapPanel
                    {
                        Margin = new Thickness(0, 10, 0, 0)
                    };

                    if (lessonStart <= DateTime.Now)
                    {
                        buttonsPanel.Children.Add(
                            CreateLessonStatusButton(
                                "Проведено",
                                lessonId,
                                "проведено",
                                "AdminActionButtonStyle"));
                    }

                    buttonsPanel.Children.Add(
                        CreateLessonStatusButton(
                            "Отменить",
                            lessonId,
                            "отменено",
                            "LightActionButtonStyle"));

                    content.Children.Add(buttonsPanel);
                }
                else if (status == "отменено")
                {
                    WrapPanel buttonsPanel = new WrapPanel
                    {
                        Margin = new Thickness(0, 10, 0, 0)
                    };

                    buttonsPanel.Children.Add(
                        CreateLessonStatusButton(
                            "Вернуть в расписание",
                            lessonId,
                            "запланировано",
                            "LightActionButtonStyle"));

                    content.Children.Add(buttonsPanel);
                }

                lessonBorder.Child = content;

                SelectedDayLessonsPanel.Children.Add(lessonBorder);
            }

            SelectedDayCountText.Text = "Занятий: " + count;

            if (count == 0)
            {
                SelectedDayLessonsPanel.Children.Add(new TextBlock
                {
                    Text = "На этот день занятий нет.",
                    FontSize = 12,
                    Foreground = (Brush)FindResource("SecondaryTextColor"),
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }
        private Button CreateLessonStatusButton(string buttonText, int lessonId,string newStatus, string styleName)
        {
            Button button = new Button
            {
                Content = buttonText,
                Style = (Style)FindResource(styleName),
                Margin = new Thickness(0, 0, 7, 0),
                Padding = new Thickness(12, 6, 12, 6),
                FontSize = 11,
                FontWeight = FontWeights.Medium,
                MinWidth = 0,
                Cursor = Cursors.Hand
            };

            button.Click += (sender, e) =>
            {
                ChangeLessonStatus(lessonId, newStatus);
            };

            return button;
        }
        private void ChangeLessonStatus(int lessonId, string newStatus)
        {
            try
            {
                lessonService.UpdateInstructorLessonStatus(
                    lessonId,
                    instructorId,
                    newStatus);

                // Обновляем занятия текущего месяца
                instructorLessons =
                    lessonService.GetInstructorLessonsByMonth(
                        instructorId,
                        currentCalendarMonth);

                // Обновляем выбранный день и календарь
                if (selectedCalendarDate.HasValue)
                {
                    ShowSelectedDay(selectedCalendarDate.Value);
                }

                BuildCalendar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Изменение статуса занятия",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        // Предыдущий месяц
        private void PreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            currentCalendarMonth =
                currentCalendarMonth.AddMonths(-1);

            LoadInstructorCalendar();
        }

        // Следующий месяц
        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            currentCalendarMonth =
                currentCalendarMonth.AddMonths(1);

            LoadInstructorCalendar();
        }
        // Загрузка списка учеников
        private void LoadInstructorStudents()
        {
            try
            {
                DataTable students = studentsService.GetInstructorStudents(instructorId);

                InstructorStudentsGrid.ItemsSource = students.DefaultView;
                InstructorStudentSearchBox.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить список учеников.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
