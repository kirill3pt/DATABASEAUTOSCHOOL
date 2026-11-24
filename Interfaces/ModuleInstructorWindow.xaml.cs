using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleInstructorWindow : Window
    {
        private readonly int instructorId;
        private readonly InstructorDashboardService dashboardService = new InstructorDashboardService();
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

        // Переключение разделов
        private void ShowSection(
            string title,
            string description,
            Button activeButton)
        {
            SetActiveButton(activeButton);

            DashboardPanel.Visibility = Visibility.Collapsed;
            SectionPanel.Visibility = Visibility.Visible;

            SectionTitle.Text = title;
            SectionDescription.Text = description;
        }

        // Расписание
        private void Lessons_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(
                "Моё расписание",
                "Просмотр запланированных занятий и информации о них.",
                LessonsButton);
        }

        // Ученики
        private void Students_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(
                "Мои ученики",
                "Просмотр информации об учениках, закреплённых за вами.",
                StudentsButton);
        }

        // Профиль
        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(
                "Мой профиль",
                "Просмотр личной информации инструктора.",
                ProfileButton);
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
        private void LoadDashboardStats()
        {
            try
            {
                DataTable stats = dashboardService.GetDashboardStats(instructorId);

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
    }
}
