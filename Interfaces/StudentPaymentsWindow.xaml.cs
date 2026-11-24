
using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Windows;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class StudentPaymentsWindow : Window
    {
        private readonly PaymentService paymentService = new PaymentService();
        private readonly int studentId;

        public StudentPaymentsWindow(int studentId, string studentFio)
        {
            InitializeComponent();

            this.studentId = studentId;

            StudentNameText.Text = studentFio;

            LoadPayments();
        }

        private void LoadPayments()
        {
            try
            {
                StudentPaymentsDataGrid.ItemsSource =
                    paymentService.GetPaymentsByStudent(studentId).DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить историю платежей.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
