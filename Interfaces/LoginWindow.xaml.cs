using System;
using System.Windows;
using System.Windows.Controls;
using DATABASEAUTOSCHOOL.Classes;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class LoginWindow : Window
    {
        private readonly AUTH auth = new AUTH();

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
                if (auth.Authenticate(role, login, password))
                {
                    if (role == "Администратор")
                    {
                        ModuleAdminWindow adminWindow = new ModuleAdminWindow();
                        adminWindow.Show();
                    }
                    else if (role == "Инструктор")
                    {
                        ModuleInstructorWindow instructorWindow =
                            new ModuleInstructorWindow();

                        instructorWindow.Show();
                    }

                    this.Close();
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
            MessageBox.Show("Регистрация инструктора будет подключена позже.");
        }
    }
}
