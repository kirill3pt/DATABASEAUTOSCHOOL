
using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class AddGroupWindow : Window
    {
        private readonly GroupService groupService = new GroupService();

        private DateTime? selectedStartDate = null;
        private DateTime? selectedEndDate = null;

        private DateTime displayedStartMonth = DateTime.Today;
        private DateTime displayedEndMonth = DateTime.Today;

        public AddGroupWindow()
        {
            InitializeComponent();

            UpdateStartCalendar();
            UpdateEndCalendar();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string groupName = GroupNameBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(groupName))
                {
                    MessageBox.Show(
                        "Введите название группы.",
                        "Проверка данных",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    GroupNameBox.Focus();
                    return;
                }

                if (selectedStartDate.HasValue &&
                    selectedEndDate.HasValue &&
                    selectedEndDate.Value < selectedStartDate.Value)
                {
                    MessageBox.Show(
                        "Дата окончания не может быть раньше даты начала.",
                        "Проверка данных",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                groupService.AddGroup(
                    groupName,
                    selectedStartDate,
                    selectedEndDate);

                MessageBox.Show(
                    "Группа успешно добавлена.",
                    "Добавление группы",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось добавить группу.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void UpdateCalendar(
            UniformGrid daysPanel,
            TextBlock monthText,
            DateTime displayedMonth,
            DateTime? selectedDate,
            RoutedEventHandler dayClick)
        {
            monthText.Text = displayedMonth.ToString(
                "MMMM yyyy",
                new CultureInfo("ru-RU"));

            daysPanel.Children.Clear();

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
                daysPanel.Children.Add(new Border
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
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(41, 51, 48))
                };

                if (selectedDate.HasValue &&
                    date.Date == selectedDate.Value.Date)
                {
                    dayButton.Background = new SolidColorBrush(
                        Color.FromRgb(168, 181, 165));

                    dayButton.FontWeight = FontWeights.Bold;
                }

                dayButton.Click += dayClick;

                daysPanel.Children.Add(dayButton);
            }
        }

        private void UpdateStartCalendar()
        {
            UpdateCalendar(
                StartCalendarDaysPanel,
                StartCalendarMonthText,
                displayedStartMonth,
                selectedStartDate,
                StartCalendarDay_Click);
        }

        private void UpdateEndCalendar()
        {
            UpdateCalendar(
                EndCalendarDaysPanel,
                EndCalendarMonthText,
                displayedEndMonth,
                selectedEndDate,
                EndCalendarDay_Click);
        }

        private void StartCalendarToggleButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            StartCalendarPopup.IsOpen = !StartCalendarPopup.IsOpen;
        }

        private void EndCalendarToggleButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            EndCalendarPopup.IsOpen = !EndCalendarPopup.IsOpen;
        }

        private void StartPreviousMonth_Click(
            object sender,
            RoutedEventArgs e)
        {
            displayedStartMonth = displayedStartMonth.AddMonths(-1);
            UpdateStartCalendar();
        }

        private void StartNextMonth_Click(
            object sender,
            RoutedEventArgs e)
        {
            displayedStartMonth = displayedStartMonth.AddMonths(1);
            UpdateStartCalendar();
        }

        private void EndPreviousMonth_Click(
            object sender,
            RoutedEventArgs e)
        {
            displayedEndMonth = displayedEndMonth.AddMonths(-1);
            UpdateEndCalendar();
        }

        private void EndNextMonth_Click(
            object sender,
            RoutedEventArgs e)
        {
            displayedEndMonth = displayedEndMonth.AddMonths(1);
            UpdateEndCalendar();
        }

        private void StartCalendarDay_Click(
            object sender,
            RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
                return;

            selectedStartDate = (DateTime)button.Tag;

            StartDateText.Text = selectedStartDate.Value.ToString("dd.MM.yyyy");
            StartDateText.Foreground = new SolidColorBrush(
                Color.FromRgb(41, 51, 48));

            UpdateStartCalendar();

            StartCalendarPopup.IsOpen = false;
        }

        private void EndCalendarDay_Click(
            object sender,
            RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
                return;

            selectedEndDate = (DateTime)button.Tag;

            EndDateText.Text = selectedEndDate.Value.ToString("dd.MM.yyyy");
            EndDateText.Foreground = new SolidColorBrush(
                Color.FromRgb(41, 51, 48));

            UpdateEndCalendar();

            EndCalendarPopup.IsOpen = false;
        }
    }
}
