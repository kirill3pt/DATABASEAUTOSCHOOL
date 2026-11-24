
using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class AddPaymentWindow : Window
    {
        private readonly PaymentService paymentService = new PaymentService();

        public AddPaymentWindow()
        {
            InitializeComponent();

            LoadStudents();

            PaymentDatePicker.SelectedDate = DateTime.Today;
            PaymentStatusComboBox.SelectedIndex = 0;
        }

        private void LoadStudents()
        {
            try
            {
                DataTable students = paymentService.GetStudentsForComboBox();

                StudentComboBox.SelectedValuePath = "StudentID";
                StudentComboBox.ItemsSource = students.DefaultView;

                if (StudentComboBox.Items.Count > 0)
                    StudentComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить список учеников.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (StudentComboBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Выберите ученика.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!decimal.TryParse(
                AmountBox.Text.Trim().Replace(',', '.'),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal amount) || amount <= 0)
            {
                MessageBox.Show(
                    "Введите корректную сумму платежа.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                AmountBox.Focus();
                return;
            }

            if (PaymentDatePicker.SelectedDate == null)
            {
                MessageBox.Show(
                    "Укажите дату платежа.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (PaymentStatusComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите статус платежа.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                int studentId = Convert.ToInt32(StudentComboBox.SelectedValue);

                string status =
                    ((ComboBoxItem)PaymentStatusComboBox.SelectedItem).Content.ToString();

                paymentService.AddPayment(
                    studentId,
                    amount,
                    PaymentDatePicker.SelectedDate.Value,
                    status);

                MessageBox.Show(
                    "Платёж успешно добавлен.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось добавить платёж.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
