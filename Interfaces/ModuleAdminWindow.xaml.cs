using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleAdminWindow : Window
    {

        private readonly CarService carService = new CarService();
        private readonly LOGGER logger = new LOGGER();

        public ModuleAdminWindow()
        {
            InitializeComponent();
            logger.LOG("Открыта панель администратора.");
            Dashboard_Click(null, null);
        }

        // Переключение активного пункта меню
        private void SetActiveButton(Button activeButton)
        {
            Button[] buttons =
            {
                DashboardButton,
                StudentsButton,
                InstructorsButton,
                CarsButton,
                LessonsButton,
                ReportsButton
            };

            foreach (Button button in buttons)
            {
                button.Background =
                    System.Windows.Media.Brushes.Transparent;

                button.Foreground =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(197, 208, 227));
            }

            if (activeButton != null)
            {
                activeButton.Background =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(65, 105, 225));

                activeButton.Foreground =
                    System.Windows.Media.Brushes.White;
            }
        }

        // Открытие главной страницы
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(DashboardButton);

            DashboardPanel.Visibility = Visibility.Visible;
            SectionPanel.Visibility = Visibility.Collapsed;
            CarsPanel.Visibility = Visibility.Collapsed;
        }

        // Универсальное открытие раздела
        private void ShowSection(
            string title,
            string description,
            Button activeButton)
        {
            SetActiveButton(activeButton);

            DashboardPanel.Visibility = Visibility.Collapsed;
            SectionPanel.Visibility = Visibility.Visible;
            CarsPanel.Visibility = Visibility.Collapsed;

            SectionTitle.Text = title;
            SectionDescription.Text = description;
        }

        // Ученики
        private void Students_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(
                "Ученики",
                "Просмотр, добавление, изменение и удаление данных учеников.",
                StudentsButton);
        }

        // Инструкторы
        private void Instructors_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(
                "Инструкторы",
                "Управление инструкторами, их контактными данными и регистрацией.",
                InstructorsButton);
        }

        // Открытие окна добавления автомобиля
        private void AddCar_Click(object sender, RoutedEventArgs e)
        {
            logger.LOG("Администратор открыл окно добавления автомобиля.");

            AddCarWindow window = new AddCarWindow
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                logger.LOG("Автомобиль успешно добавлен.");
                LoadCars();
            }
            else
            {
                logger.LOG("Добавление автомобиля отменено.");
            }
        }


        // Удаление выбранных автомобилей
        private void DeleteCars_Click(object sender, RoutedEventArgs e)
        {
            if (CarsDataGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Выберите один или несколько автомобилей для удаления.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                logger.LOG("Попытка удаления автомобилей без выбора записей.");
                return;
            }

            List<int> ids = new List<int>();

            foreach (DataRowView row in CarsDataGrid.SelectedItems)
            {
                ids.Add(Convert.ToInt32(row["ID"]));
            }

            MessageBoxResult result = MessageBox.Show(
                $"Вы действительно хотите удалить выбранные автомобили ({ids.Count})?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                logger.LOG("Удаление автомобилей отменено пользователем.");
                return;
            }

            try
            {
                carService.DeleteCars(ids);

                logger.LOG(
                    $"Удалено автомобилей: {ids.Count}. ID: {string.Join(", ", ids)}");

                LoadCars();

                MessageBox.Show(
                    $"Успешно удалено автомобилей: {ids.Count}.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка удаления автомобилей: " + ex);

                MessageBox.Show(
                    "Не удалось удалить автомобили:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // Обновление таблицы
        private void RefreshCars_Click(object sender, RoutedEventArgs e)
        {
            logger.LOG("Администратор обновил список автомобилей.");
            LoadCars();
        }

        private void LoadCars()
        {
            try
            {
                CarsDataGrid.ItemsSource = carService.GetCars().DefaultView;

                logger.LOG("Список автомобилей успешно загружен.");
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка загрузки автомобилей: " + ex);

                MessageBox.Show(
                    "Не удалось загрузить автомобили:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Автомобили
        private void Cars_Click(object sender, RoutedEventArgs e)
        {
            logger.LOG("Открыт раздел автомобилей.");
            SetActiveButton(CarsButton);

            DashboardPanel.Visibility = Visibility.Collapsed;
            SectionPanel.Visibility = Visibility.Collapsed;
            CarsPanel.Visibility = Visibility.Visible;

            LoadCars();
        }

        // Расписание
        private void Lessons_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(
                "Расписание",
                "Просмотр занятий, управление расписанием и изменение статусов занятий.",
                LessonsButton);
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
                null);
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
