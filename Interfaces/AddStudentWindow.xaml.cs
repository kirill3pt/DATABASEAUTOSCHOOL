
using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class AddStudentWindow : Window
    {
        private readonly StudentService studentService = new StudentService();

        public AddStudentWindow()
        {
            InitializeComponent();

            PhoneBox.Text = "+7";

            LoadGroups();
        }


        // Загрузка учебных групп
        private void LoadGroups()
        {
            try
            {
                DataTable groups = studentService.GetGroupsForComboBox();

                GroupComboBox.SelectedValuePath = "GroupID";
                GroupComboBox.ItemsSource = groups.DefaultView;

                if (GroupComboBox.Items.Count > 0)
                    GroupComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить список учебных групп.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
        }

        // Добавление ученика
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string lastName = LastNameBox.Text.Trim();
                string firstName = FirstNameBox.Text.Trim();
                string middleName = MiddleNameBox.Text.Trim();
                string phone = PhoneBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(lastName))
                {
                    MessageBox.Show("Введите фамилию ученика.",
                        "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LastNameBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(firstName))
                {
                    MessageBox.Show("Введите имя ученика.",
                        "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                    FirstNameBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(phone))
                {
                    MessageBox.Show("Введите номер телефона.",
                        "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                    PhoneBox.Focus();
                    return;
                }
                string phoneDigits = new string(phone.Where(char.IsDigit).ToArray());

                if (phoneDigits.Length != 11 || !phoneDigits.StartsWith("7"))
                {
                    MessageBox.Show(
                        "Введите полный номер телефона в формате +7 (999) 123-45-67.",
                        "Проверка данных",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    PhoneBox.Focus();
                    return;
                }
                if (GroupComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Выберите учебную группу.",
                        "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                    GroupComboBox.Focus();
                    return;
                }

                int groupId = Convert.ToInt32(GroupComboBox.SelectedValue);

                studentService.AddStudent(
                    lastName,
                    firstName,
                    middleName,
                    phone,
                    groupId);

                MessageBox.Show("Ученик успешно добавлен.",
                    "Добавление ученика", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось добавить ученика.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool isPhoneUpdating = false;

        private void PhoneBox_PreviewTextInput(
            object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void PhoneBox_PreviewKeyDown(
            object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Space)
                e.Handled = true;
        }


        private void PhoneBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isPhoneUpdating)
                return;

            isPhoneUpdating = true;

            try
            {
                string digits = new string(
                    PhoneBox.Text.Where(char.IsDigit).ToArray());

                if (digits.StartsWith("7") || digits.StartsWith("8"))
                    digits = digits.Substring(1);

                if (digits.Length > 10)
                    digits = digits.Substring(0, 10);

                string formatted = "+7";

                if (digits.Length > 0)
                    formatted += " (" + digits.Substring(0, Math.Min(3, digits.Length));

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
                isPhoneUpdating = false;
            }
        }


        // Закрытие окна
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
