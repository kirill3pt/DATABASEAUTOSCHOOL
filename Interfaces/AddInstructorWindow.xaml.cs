
using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class AddInstructorWindow : Window
    {
        private readonly InstructorService instructorService = new InstructorService();
        private bool isFormattingPhone;

        public AddInstructorWindow()
        {
            InitializeComponent();

            PhoneBox.Text = "+7 (";
            PhoneBox.CaretIndex = PhoneBox.Text.Length;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string lastName = LastNameBox.Text.Trim();
            string firstName = FirstNameBox.Text.Trim();
            string middleName = MiddleNameBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(firstName))
            {
                MessageBox.Show(
                    "Заполните фамилию и имя инструктора.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(lastName, @"^[А-Яа-яЁё-]+$") ||
                !Regex.IsMatch(firstName, @"^[А-Яа-яЁё-]+$") ||
                (!string.IsNullOrWhiteSpace(middleName) &&
                 !Regex.IsMatch(middleName, @"^[А-Яа-яЁё-]+$")))
            {
                MessageBox.Show(
                    "ФИО должно содержать только русские буквы и дефис.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            string digits = Regex.Replace(phone, @"\D", "");

            if (digits.Length != 11 || !digits.StartsWith("7"))
            {
                MessageBox.Show(
                    "Введите корректный номер телефона.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                instructorService.AddInstructor(
                    lastName,
                    firstName,
                    string.IsNullOrWhiteSpace(middleName) ? null : middleName,
                    phone);

                MessageBox.Show(
                    "Инструктор успешно добавлен.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось добавить инструктора.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void PhoneBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d$");
        }

        private void PhoneBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back && PhoneBox.CaretIndex <= 4)
            {
                e.Handled = true;
            }
        }

        private void PhoneBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (isFormattingPhone)
                return;

            isFormattingPhone = true;

            try
            {
                string digits = Regex.Replace(PhoneBox.Text, @"\D", "");

                if (digits.StartsWith("7") || digits.StartsWith("8"))
                    digits = digits.Substring(1);

                if (digits.Length > 10)
                    digits = digits.Substring(0, 10);

                string formatted = "+7 (";

                if (digits.Length > 0)
                    formatted += digits.Substring(0, Math.Min(3, digits.Length));

                if (digits.Length >= 3)
                    formatted += ")";

                if (digits.Length > 3)
                    formatted += " " + digits.Substring(3, Math.Min(3, digits.Length - 3));

                if (digits.Length > 6)
                    formatted += "-" + digits.Substring(6, Math.Min(2, digits.Length - 6));

                if (digits.Length > 8)
                    formatted += "-" + digits.Substring(8, Math.Min(2, digits.Length - 8));

                PhoneBox.Text = formatted;
                PhoneBox.CaretIndex = PhoneBox.Text.Length;
            }
            finally
            {
                isFormattingPhone = false;
            }
        }
    }
}
