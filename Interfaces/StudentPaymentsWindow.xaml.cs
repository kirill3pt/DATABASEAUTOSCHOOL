
using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Data;
using System.Windows;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class StudentPaymentsWindow : Window
    {
        private readonly PaymentService paymentService =
            new PaymentService();

        private readonly int studentId;

        public StudentPaymentsWindow(int studentId, string studentFio)
        {
            InitializeComponent();

            this.studentId = studentId;

            StudentNameText.Text = studentFio;

            LoadPayments();
            LoadFinancialSummary();
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

        private void LoadFinancialSummary()
        {
            try
            {
                DataTable summary =
                    paymentService.GetStudentPaymentSummary(studentId);

                if (summary.Rows.Count == 0)
                {
                    throw new Exception(
                        "Не удалось получить финансовую информацию ученика.");
                }

                DataRow row = summary.Rows[0];

                decimal tuitionCost =
                    Convert.ToDecimal(row["Стоимость обучения"]);

                decimal paid =
                    Convert.ToDecimal(row["Оплачено"]);

                decimal remaining =
                    Convert.ToDecimal(row["Осталось"]);

                string status =
                    row["Финансовый статус"].ToString();

                TuitionCostText.Text =
                    tuitionCost.ToString("N2") + " ₽";

                PaidAmountText.Text =
                    paid.ToString("N2") + " ₽";

                RemainingAmountText.Text =
                    remaining.ToString("N2") + " ₽";

                FinancialStatusText.Text = status;

                double progress = tuitionCost > 0
                    ? (double)(paid / tuitionCost * 100)
                    : 0;

                progress = Math.Max(0, Math.Min(progress, 100));

                PaymentProgressBar.Value = progress;

                PaymentProgressText.Text =
                    "Оплачено " + progress.ToString("0.0") + "%";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить финансовую сводку.\n" + ex.Message,
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
