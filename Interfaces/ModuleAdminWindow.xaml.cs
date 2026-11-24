using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleAdminWindow : Window
    {

        private readonly CarService carService = new CarService();
        private readonly StudentService studentService = new StudentService();
        private readonly GroupService groupService = new GroupService();
        private readonly PaymentService paymentService = new PaymentService();
        private readonly LOGGER logger = new LOGGER();

        public ModuleAdminWindow()
        {
            InitializeComponent();
            logger.LOG("Открыта панель администратора.");

            LoadStudentsCount();

            Dashboard_Click(null, null);
        }

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

        // Открытие главной страницы
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(DashboardButton);

            DashboardPanel.Visibility = Visibility.Visible;
            SectionPanel.Visibility = Visibility.Collapsed;
            GroupsPanel.Visibility = Visibility.Collapsed;
            CarsPanel.Visibility = Visibility.Collapsed;
            StudentsPanel.Visibility = Visibility.Collapsed;
            PaymentsPanel.Visibility = Visibility.Collapsed;
        }

        private void ShowSection(string title, string description, Button activeButton)
        {
            SetActiveButton(activeButton);
            DashboardPanel.Visibility = Visibility.Collapsed;
            SectionPanel.Visibility = Visibility.Visible;
            GroupsPanel.Visibility = Visibility.Collapsed;
            CarsPanel.Visibility = Visibility.Collapsed;
            StudentsPanel.Visibility = Visibility.Collapsed;
            PaymentsPanel.Visibility = Visibility.Collapsed;

            SectionTitle.Text = title;
            SectionDescription.Text = description;
        }

        private void LoadGroups(string searchText = "")
        {
            try
            {
                GroupsDataGrid.ItemsSource = groupService.GetGroups(searchText).DefaultView;
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка загрузки групп: " + ex.Message);

                MessageBox.Show(
                    "Не удалось загрузить список групп.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Открытие раздела групп
        private void Groups_Click(object sender, RoutedEventArgs e)
        {
            logger.LOG("Открыт раздел учебных групп.");

            SetActiveButton(GroupsButton);

            DashboardPanel.Visibility = Visibility.Collapsed;
            SectionPanel.Visibility = Visibility.Collapsed;
            CarsPanel.Visibility = Visibility.Collapsed;
            StudentsPanel.Visibility = Visibility.Collapsed;
            PaymentsPanel.Visibility = Visibility.Collapsed;
            GroupsPanel.Visibility = Visibility.Visible;

            LoadGroups();
        }

        // Поиск групп
        private void GroupSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadGroups(GroupSearchBox.Text);
        }

        // Обновление списка групп
        private void RefreshGroups_Click(object sender, RoutedEventArgs e)
        {
            GroupSearchBox.Clear();
            LoadGroups();
        }

        // Удаление выбранных групп
        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsDataGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Выберите группу для удаления.",
                    "Удаление группы",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            List<int> groupIds = new List<int>();

            foreach (var item in GroupsDataGrid.SelectedItems)
            {
                if (item is DataRowView row)
                    groupIds.Add(Convert.ToInt32(row["ID"]));
            }

            MessageBoxResult result = MessageBox.Show(
                $"Удалить выбранные группы ({groupIds.Count})?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                groupService.DeleteGroups(groupIds);
                LoadGroups(GroupSearchBox.Text);

                logger.LOG("Удалены группы: " + string.Join(", ", groupIds));

                MessageBox.Show(
                    "Выбранные группы удалены.",
                    "Удаление групп",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка удаления групп: " + ex.Message);

                MessageBox.Show(
                    ex.Message,
                    "Не удалось удалить группы",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void LoadStudents(string searchText = "")
        {
            try
            {
                StudentsDataGrid.ItemsSource =
                    studentService.GetStudents(searchText).DefaultView;
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка загрузки учеников: " + ex.Message);

                MessageBox.Show(
                    "Не удалось загрузить список учеников.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadStudentsCount()
        {
            try
            {
                StudentsCountText.Text =
                    studentService.GetStudentsCount().ToString();
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка загрузки количества учеников: " + ex.Message);
            }
        }
        private void StudentSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsInitialized)
                LoadStudents(StudentSearchBox.Text.Trim());
        }

        private void RefreshStudents_Click(object sender, RoutedEventArgs e)
        {
            StudentSearchBox.Clear();
            LoadStudents("");

            logger.LOG("Список учеников обновлён.");
        }

        private void DeleteStudents_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsDataGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Выберите одного или нескольких учеников для удаления.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            List<int> ids = new List<int>();

            foreach (DataRowView row in StudentsDataGrid.SelectedItems)
                ids.Add(Convert.ToInt32(row["ID"]));

            MessageBoxResult result = MessageBox.Show(
                $"Вы действительно хотите удалить выбранных учеников ({ids.Count})?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                studentService.DeleteStudents(ids);

                logger.LOG($"Удалено учеников: {ids.Count}");

                LoadStudents();

                MessageBox.Show(
                    $"Успешно удалено учеников: {ids.Count}.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка удаления учеников: " + ex);

                MessageBox.Show(
                    "Не удалось удалить учеников:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            LoadStudentsCount();
        }

        // Временные обработчики. Позже подключим отдельные окна.
        private void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            AddStudentWindow window = new AddStudentWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadStudents(StudentSearchBox.Text);
                logger.LOG("Добавлен новый ученик.");
            }
            LoadStudentsCount();
        }

        private void EditStudent_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Окно изменения данных ученика будет подключено следующим шагом.");
        }


        // Ученики
        private void Students_Click(object sender, RoutedEventArgs e)
        {
            logger.LOG("Открыт раздел учеников.");

            SetActiveButton(StudentsButton);

            DashboardPanel.Visibility = Visibility.Collapsed;
            SectionPanel.Visibility = Visibility.Collapsed;
            GroupsPanel.Visibility = Visibility.Collapsed;
            CarsPanel.Visibility = Visibility.Collapsed;
            PaymentsPanel.Visibility = Visibility.Collapsed;
            StudentsPanel.Visibility = Visibility.Visible;

            LoadStudents();
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

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            AddGroupWindow window = new AddGroupWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadGroups(GroupSearchBox.Text);
                logger.LOG("Добавлена новая учебная группа.");
            }
        }

        private void EditGroup_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Форма редактирования группы будет подключена следующим шагом.");
        }

        // Автомобили
        private void Cars_Click(object sender, RoutedEventArgs e)
        {
            logger.LOG("Открыт раздел автомобилей.");

            SetActiveButton(CarsButton);
            DashboardPanel.Visibility = Visibility.Collapsed;
            SectionPanel.Visibility = Visibility.Collapsed;
            GroupsPanel.Visibility = Visibility.Collapsed;
            PaymentsPanel.Visibility = Visibility.Collapsed;
            CarsPanel.Visibility = Visibility.Visible;
            StudentsPanel.Visibility = Visibility.Collapsed;

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

        // Платежи
        private void Payments_Click(object sender, RoutedEventArgs e)
        {
            logger.LOG("Открыт раздел платежей.");

            SetActiveButton(PaymentsButton);

            DashboardPanel.Visibility = Visibility.Collapsed;
            SectionPanel.Visibility = Visibility.Collapsed;
            GroupsPanel.Visibility = Visibility.Collapsed;
            CarsPanel.Visibility = Visibility.Collapsed;
            StudentsPanel.Visibility = Visibility.Collapsed;
            PaymentsPanel.Visibility = Visibility.Visible;
        }
        // Поиск платежей

        // Загрузка платежей
        private void LoadPayments(string searchText = "")
        {
            try
            {
                PaymentsDataGrid.ItemsSource =
                    paymentService.GetPayments(searchText).DefaultView;

                logger.LOG("Список платежей успешно загружен.");
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка загрузки платежей: " + ex);

                MessageBox.Show(
                    "Не удалось загрузить список платежей:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // Поиск платежей
        private void PaymentSearchBox_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            if (IsInitialized)
                LoadPayments(PaymentSearchBox.Text);
        }

        // Обновление списка платежей
        private void RefreshPayments_Click(object sender, RoutedEventArgs e)
        {
            PaymentSearchBox.Clear();
            LoadPayments();

            logger.LOG("Список платежей обновлён.");
        }

        // Удаление выбранных платежей
        private void DeletePayments_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsDataGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Выберите один или несколько платежей для удаления.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            List<int> ids = new List<int>();

            foreach (DataRowView row in PaymentsDataGrid.SelectedItems)
                ids.Add(Convert.ToInt32(row["ID"]));

            MessageBoxResult result = MessageBox.Show(
                $"Вы действительно хотите удалить выбранные платежи ({ids.Count})?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                paymentService.DeletePayments(ids);

                logger.LOG(
                    "Удалены платежи. ID: " + string.Join(", ", ids));

                LoadPayments(PaymentSearchBox.Text);

                MessageBox.Show(
                    $"Успешно удалено платежей: {ids.Count}.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка удаления платежей: " + ex);

                MessageBox.Show(
                    "Не удалось удалить платежи:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Добавление платежа
        private void AddPayment_Click(object sender, RoutedEventArgs e)
        {
            AddPaymentWindow window = new AddPaymentWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadPayments(PaymentSearchBox.Text);
                logger.LOG("Добавлен новый платёж.");
            }
        }

        private void StudentPayments_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = StudentsDataGrid.SelectedItem as DataRowView;

            if (row == null)
            {
                MessageBox.Show(
                    "Сначала выберите ученика.",
                    "История платежей",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            int studentId = Convert.ToInt32(row["ID"]);

            string lastName = row["Фамилия"].ToString();
            string firstName = row["Имя"].ToString();

            string middleName = row["Отчество"] == DBNull.Value
                ? ""
                : row["Отчество"].ToString();

            string studentFio = (lastName + " " + firstName + " " + middleName).Trim();

            StudentPaymentsWindow window =
                new StudentPaymentsWindow(studentId, studentFio);

            window.Owner = this;
            window.ShowDialog();
        }


        // Изменение платежа
        private void EditPayment_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Форма изменения платежа будет подключена следующим шагом.");
        }

    }
}
