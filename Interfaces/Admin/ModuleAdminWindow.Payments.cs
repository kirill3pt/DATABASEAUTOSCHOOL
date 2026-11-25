
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleAdminWindow
    {
        // Открытие раздела платежей
        private void Payments_Click(object sender, RoutedEventArgs e)
        {
            logger.LOG("Открыт раздел платежей.");

            SetActiveButton(PaymentsButton);
            HideAllPanels();

            ShowPanel(PaymentsPanel);

            LoadPayments();
        }

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

        // История платежей ученика
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

            string studentFio =
                (lastName + " " + firstName + " " + middleName).Trim();

            StudentPaymentsWindow window =
                new StudentPaymentsWindow(studentId, studentFio);

            window.Owner = this;
            window.ShowDialog();
        }

        // Изменение платежа

        private void EditPayment_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsDataGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Выберите платёж для изменения статуса.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (PaymentsDataGrid.SelectedItems.Count > 1)
            {
                MessageBox.Show(
                    "Для изменения статуса выберите только один платёж.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            ContextMenu menu = EditPaymentButton.ContextMenu;

            menu.PlacementTarget = EditPaymentButton;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            menu.IsOpen = true;
        }


        private void PaymentStatusMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item == null)
                return;

            DataRowView row = PaymentsDataGrid.SelectedItem as DataRowView;

            if (row == null)
                return;

            int paymentId = Convert.ToInt32(row["ID"]);
            string currentStatus = row["Статус"].ToString();
            string newStatus = item.Tag.ToString();

            if (currentStatus == newStatus)
                return;

            try
            {
                paymentService.UpdatePaymentStatus(paymentId, newStatus);

                LoadPayments(PaymentSearchBox.Text);

                logger.LOG(
                    "Изменён статус платежа ID " + paymentId +
                    ". Новый статус: " + newStatus);

                MessageBox.Show(
                    "Статус платежа успешно изменён.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка изменения статуса платежа: " + ex);

                MessageBox.Show(
                    "Не удалось изменить статус платежа:\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

    }
}
