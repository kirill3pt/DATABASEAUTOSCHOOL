
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DATABASEAUTOSCHOOL.Classes;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class AddLessonWindow : Window
    {
        private readonly LessonService lessonService = new LessonService();

        private DateTime selectedLessonDate = DateTime.Today;
        private DateTime displayedMonth = DateTime.Today;

        public AddLessonWindow()
        {
            InitializeComponent();

            LoadComboBoxes();
            LoadTimeValues();

            selectedLessonDate = DateTime.Today;
            displayedMonth = DateTime.Today;

            UpdateCalendar();
            UpdateSelectedDateText();
        }

        private void LoadComboBoxes()
        {
            try
            {
                StudentBox.ItemsSource = lessonService.GetStudents().DefaultView;
                InstructorBox.ItemsSource = lessonService.GetInstructors().DefaultView;
                CarBox.ItemsSource = lessonService.GetCars().DefaultView;

                if (StudentBox.Items.Count > 0)
                    StudentBox.SelectedIndex = 0;

                if (InstructorBox.Items.Count > 0)
                    InstructorBox.SelectedIndex = 0;

                if (CarBox.Items.Count > 0)
                    CarBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить списки для занятия.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
        }

        private void LoadTimeValues()
        {
            for (int hour = 0; hour < 24; hour++)
                HourBox.Items.Add(hour.ToString("00"));

            for (int minute = 0; minute < 60; minute += 5)
                MinuteBox.Items.Add(minute.ToString("00"));

            HourBox.SelectedItem = "10";
            MinuteBox.SelectedItem = "00";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudentBox.SelectedValue == null ||
                InstructorBox.SelectedValue == null ||
                CarBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Необходимо выбрать ученика, инструктора и автомобиль.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (HourBox.SelectedItem == null ||
                MinuteBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите время занятия.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            ComboBoxItem selectedStatus = StatusBox.SelectedItem as ComboBoxItem;

            if (selectedStatus == null)
            {
                MessageBox.Show(
                    "Выберите статус занятия.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            int hour = Convert.ToInt32(HourBox.SelectedItem);
            int minute = Convert.ToInt32(MinuteBox.SelectedItem);

            TimeSpan lessonTime = new TimeSpan(hour, minute, 0);
            string status = selectedStatus.Content.ToString();

            try
            {
                lessonService.AddLesson(
                    Convert.ToInt32(StudentBox.SelectedValue),
                    Convert.ToInt32(InstructorBox.SelectedValue),
                    Convert.ToInt32(CarBox.SelectedValue),
                    selectedLessonDate,
                    lessonTime,
                    status);

                MessageBox.Show(
                    "Занятие успешно добавлено.",
                    "Занятия",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Не удалось добавить занятие",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void UpdateSelectedDateText()
        {
            SelectedDateText.Text = selectedLessonDate.ToString("dd.MM.yyyy");
        }


        private void UpdateCalendar()
        {
            CalendarMonthText.Text = displayedMonth.ToString(
                "MMMM yyyy",
                new System.Globalization.CultureInfo("ru-RU"));

            CalendarDaysPanel.Children.Clear();

            DateTime firstDay = new DateTime(
                displayedMonth.Year,
                displayedMonth.Month,
                1);

            int offset = ((int)firstDay.DayOfWeek + 6) % 7;

            int daysInMonth = DateTime.DaysInMonth(
                displayedMonth.Year,
                displayedMonth.Month);

            for (int i = 0; i < offset; i++)
            {
                CalendarDaysPanel.Children.Add(new Border
                {
                    Height = 36
                });
            }

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(
                    displayedMonth.Year,
                    displayedMonth.Month,
                    day);

                Button dayButton = new Button
                {
                    Content = day.ToString(),
                    Tag = date,
                    Height = 36,
                    Margin = new Thickness(2),
                    FontSize = 13,
                    FontWeight = FontWeights.Normal,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Style = (Style)FindResource("CalendarButtonStyle"),
                    Background = Brushes.Transparent,
                    Foreground = new SolidColorBrush(Color.FromRgb(41, 51, 48))
                };

                if (date.Date == selectedLessonDate.Date)
                {
                    dayButton.Background = new SolidColorBrush(
                        Color.FromRgb(168, 181, 165));

                    dayButton.FontWeight = FontWeights.Bold;
                }

                dayButton.Click += CalendarDay_Click;

                CalendarDaysPanel.Children.Add(dayButton);
            }
        }


        private void CalendarToggleButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LessonCalendarPopup.IsOpen =
                !LessonCalendarPopup.IsOpen;
        }

        private void PreviousMonth_Click(
            object sender,
            RoutedEventArgs e)
        {
            displayedMonth = displayedMonth.AddMonths(-1);
            UpdateCalendar();
        }

        private void NextMonth_Click(
            object sender,
            RoutedEventArgs e)
        {
            displayedMonth = displayedMonth.AddMonths(1);
            UpdateCalendar();
        }

        private void CalendarDay_Click(
            object sender,
            RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
                return;

            selectedLessonDate = (DateTime)button.Tag;

            UpdateSelectedDateText();
            UpdateCalendar();

            LessonCalendarPopup.IsOpen = false;
        }
    }
}
