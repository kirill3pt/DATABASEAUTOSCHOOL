
using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class EditInstructorWindow : Window
    {
        private readonly InstructorService instructorService = new InstructorService();
        private readonly int instructorId;
        private bool isFormattingPhone;

        public EditInstructorWindow(int id)
        {
            InitializeComponent();

            instructorId = id;

            LoadInstructor();
        }

        private void LoadInstructor()
        {
            try
            {
                DataRow instructor = instructorService.GetInstructorById(instructorId);

                if (instructor == null)
                {
                    MessageBox.Show(
                        "Инструктор не найден.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    Close();
                    return;
                }

                LastNameBox.Text = instructor["LastName"].ToString();
                FirstNameBox.Text = instructor["FirstName"].ToString();

                MiddleNameBox.Text = instructor["MiddleName"] == DBNull.Value
                    ? ""
                    : instructor["MiddleName"].ToString();

                PhoneBox.Text = instructor["Phone"].ToString();
                PhoneBox.CaretIndex = PhoneBox.Text.Length;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить данные инструктора.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
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
                instructorService.UpdateInstructor(
                    instructorId,
                    lastName,
                    firstName,
                    string.IsNullOrWhiteSpace(middleName) ? null : middleName,
                    phone);

                MessageBox.Show(
                    "Данные инструктора успешно обновлены.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось сохранить изменения.\n\n" + ex.Message,
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
                e.Handled = true;
        }

        private void PhoneBox_TextChanged(object sender, TextChangedEventArgs e)
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

                StringBuilder formatted = new StringBuilder("+7 (");

                if (digits.Length > 0)
                    formatted.Append(digits.Substring(0, Math.Min(3, digits.Length)));

                if (digits.Length >= 3)
                    formatted.Append(")");

                if (digits.Length > 3)
                    formatted.Append(" " + digits.Substring(3, Math.Min(3, digits.Length - 3)));

                if (digits.Length > 6)
                    formatted.Append("-" + digits.Substring(6, Math.Min(2, digits.Length - 6)));

                if (digits.Length > 8)
                    formatted.Append("-" + digits.Substring(8, digits.Length - 8));

                PhoneBox.Text = formatted.ToString();
                PhoneBox.CaretIndex = PhoneBox.Text.Length;
            }
            finally
            {
                isFormattingPhone = false;
            }
        }
    }
}
