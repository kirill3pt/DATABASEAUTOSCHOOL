
using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Windows;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class RegisterInstructorWindow : Window
    {
        private readonly InstructorInvitationService invitationService =
            new InstructorInvitationService();

        public RegisterInstructorWindow()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string invitationCode = InvitationCodeBox.Text.Trim();
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(invitationCode))
            {
                MessageBox.Show("Введите код приглашения.");
                InvitationCodeBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Введите логин.");
                LoginBox.Focus();
                return;
            }

            if (login.Length < 4)
            {
                MessageBox.Show("Логин должен содержать не менее 4 символов.");
                LoginBox.Focus();
                return;
            }

            if (password.Length < 8)
            {
                MessageBox.Show("Пароль должен содержать не менее 8 символов.");
                PasswordBox.Focus();
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Введённые пароли не совпадают.");
                ConfirmPasswordBox.Clear();
                ConfirmPasswordBox.Focus();
                return;
            }

            try
            {
                invitationService.RegisterInstructor(
                    invitationCode,
                    login,
                    password);

                MessageBox.Show(
                    "Учётная запись успешно создана. Теперь можно войти в систему.",
                    "Регистрация завершена",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Регистрация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось выполнить регистрацию.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
