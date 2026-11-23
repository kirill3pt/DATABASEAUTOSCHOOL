using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleInstructorWindow : Window
    {
        public ModuleInstructorWindow()
        {
            InitializeComponent();
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
                    Color.FromRgb(197, 208, 227));
            }

            if (activeButton != null)
            {
                activeButton.Background = new SolidColorBrush(
                    Color.FromRgb(65, 105, 225));

                activeButton.Foreground = Brushes.White;
            }
        }

        // Главная
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(DashboardButton);

            DashboardPanel.Visibility = Visibility.Visible;
            SectionPanel.Visibility = Visibility.Collapsed;
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
    }
}
