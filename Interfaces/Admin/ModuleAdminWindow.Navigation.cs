
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleAdminWindow
    {
        // Переключение активного пункта меню
        private void SetActiveButton(Button activeButton)
        {
            Button[] buttons =
            {
                DashboardButton,
                GroupsButton,
                StudentsButton,
                InstructorsButton,
                CarsButton,
                LessonsButton,
                PaymentsButton,
                ReportsButton,
                SettingsButton
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

        // Скрытие всех рабочих панелей
        private void HideAllPanels()
        {
            DashboardPanel.Visibility = Visibility.Collapsed;
            SectionPanel.Visibility = Visibility.Collapsed;
            GroupsPanel.Visibility = Visibility.Collapsed;
            CarsPanel.Visibility = Visibility.Collapsed;
            StudentsPanel.Visibility = Visibility.Collapsed;
            PaymentsPanel.Visibility = Visibility.Collapsed;
            InstructorsPanel.Visibility = Visibility.Collapsed;
            LessonsPanel.Visibility = Visibility.Collapsed;
        }

        private void ShowPanel(Grid panel)
        {
            panel.Opacity = 0;
            panel.Visibility = Visibility.Visible;

            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(220),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            panel.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        // Открытие главной страницы
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(DashboardButton);
            HideAllPanels();

            DashboardPanel.Visibility = Visibility.Visible;
        }

        // Открытие обычного раздела
        private void ShowSection(
            string title,
            string description,
            Button activeButton)
        {
            SetActiveButton(activeButton);
            HideAllPanels();

            SectionPanel.Visibility = Visibility.Visible;

            SectionTitle.Text = title;
            SectionDescription.Text = description;
        }

        // Отчёты
        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(
                "Отчёты",
                "Формирование и сохранение Excel-отчётов по работе автошколы.",
                ReportsButton);
        }

        // Настройки
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(
                "Настройки",
                "Настройка приложения и параметров журналирования.",
                SettingsButton);
        }

        // О программе
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Информационная система управления автошколой.\n\n" +
                "Панель администратора.\n" +
                "Версия 1.0",
                "О программе",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Выход из аккаунта
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Вы действительно хотите выйти из учётной записи?",
                "Выход",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                logger.LOG("Администратор вышел из учётной записи.");

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                Close();
            }
        }
    }
}
