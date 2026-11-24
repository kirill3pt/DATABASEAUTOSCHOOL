using System;
using System.Windows;
using System.Windows.Controls;
using DATABASEAUTOSCHOOL.Classes;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class LoginWindow : Window
    {
        private readonly AUTH auth = new AUTH();
        private readonly LOGGER logger = new LOGGER();
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string role = (roleBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string login = loginBox.Text.Trim();
            string password = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(role) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrEmpty(password))
            {
                MessageBox.Show(
                    "Заполните все поля.",
                    "Авторизация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                int instructorId;

                if (auth.Authenticate(role, login, password, out instructorId))
                {
                    if (role == "Администратор")
                    {
                        ModuleAdminWindow adminWindow = new ModuleAdminWindow();
                        adminWindow.Show();

                        logger.LOG($"Авторизация. Администратор {login} вошёл в систему.");
                    }
                    else if (role == "Инструктор")
                    {
                        ModuleInstructorWindow instructorWindow =
                            new ModuleInstructorWindow(instructorId);

                        instructorWindow.Show();

                        logger.LOG($"Авторизация. Инструктор {login} вошёл в систему.");
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        "Неверный логин или пароль.",
                        "Авторизация",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось выполнить авторизацию.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterInstructorWindow window = new RegisterInstructorWindow();
            window.Owner = this;
            window.ShowDialog();
        }
    }
}
