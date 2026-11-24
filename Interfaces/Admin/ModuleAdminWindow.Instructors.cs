
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleAdminWindow
    {
        private void Instructors_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(InstructorsButton);
            HideAllPanels();

            ShowPanel(InstructorsPanel);

            LoadInstructors();

            logger.LOG("Открыт раздел инструкторов.");
        }

        private void LoadInstructors(string searchText = "")
        {
            try
            {
                InstructorsDataGrid.ItemsSource =
                    instructorService.GetInstructors(searchText).DefaultView;
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка загрузки инструкторов: " + ex.Message);

                MessageBox.Show(
                    "Не удалось загрузить список инструкторов.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void InstructorSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsInitialized)
                LoadInstructors(InstructorSearchBox.Text.Trim());
        }

        private void RefreshInstructors_Click(object sender, RoutedEventArgs e)
        {
            InstructorSearchBox.Clear();
            LoadInstructors("");

            logger.LOG("Список инструкторов обновлён.");
        }

        private void DeleteInstructors_Click(object sender, RoutedEventArgs e)
        {
            List<int> ids = new List<int>();

            foreach (DataRowView row in InstructorsDataGrid.SelectedItems)
            {
                ids.Add(Convert.ToInt32(row["ID"]));
            }

            if (ids.Count == 0)
            {
                MessageBox.Show(
                    "Выберите инструкторов для удаления.",
                    "Удаление",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Вы действительно хотите удалить выбранных инструкторов?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                instructorService.DeleteInstructors(ids);
                LoadInstructors(InstructorSearchBox.Text.Trim());

                logger.LOG("Удалены инструкторы. Количество: " + ids.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить инструкторов.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void AddInstructor_Click(object sender, RoutedEventArgs e)
        {
            AddInstructorWindow window = new AddInstructorWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadInstructors(InstructorSearchBox.Text.Trim());
            }
        }

        private void EditInstructor_Click(object sender, RoutedEventArgs e)
        {
            if (InstructorsDataGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите инструктора для редактирования.",
                    "Редактирование",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = InstructorsDataGrid.SelectedItem as DataRowView;

            if (selectedRow == null)
                return;

            int instructorId = Convert.ToInt32(selectedRow["ID"]);

            EditInstructorWindow window = new EditInstructorWindow(instructorId);
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadInstructors(InstructorSearchBox.Text.Trim());
            }
        }

        private void CreateInvitation_Click(object sender, RoutedEventArgs e)
        {
            if (InstructorsDataGrid.SelectedItems.Count != 1)
            {
                MessageBox.Show(
                    "Выберите одного инструктора.",
                    "Приглашение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = InstructorsDataGrid.SelectedItem as DataRowView;

            if (selectedRow == null)
                return;

            string registrationStatus = Convert.ToString(selectedRow["Регистрация"]);

            if (registrationStatus == "Зарегистрирован")
            {
                MessageBox.Show(
                    "У этого инструктора уже есть учётная запись.",
                    "Приглашение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            int instructorId = Convert.ToInt32(selectedRow["ID"]);

            try
            {
                string invitationCode = invitationService.CreateInvitation(instructorId);

                InvitationCodeWindow window = new InvitationCodeWindow(invitationCode);
                window.Owner = this;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Ошибка создания приглашения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ToggleInstructorAccount_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedInstructor = InstructorsDataGrid.SelectedItem as DataRowView;

            if (selectedInstructor == null)
            {
                MessageBox.Show(
                    "Выберите инструктора.",
                    "Управление учётной записью",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            int instructorId = Convert.ToInt32(selectedInstructor["ID"]);

            try
            {
                DataRow account = userService.GetAccountByInstructorId(instructorId);

                if (account == null)
                {
                    MessageBox.Show(
                        "У этого инструктора нет учётной записи. Сначала создайте приглашение.",
                        "Управление учётной записью",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                bool isActive = Convert.ToBoolean(account["IsActive"]);
                bool newStatus = !isActive;

                string action = newStatus ? "восстановить доступ" : "заблокировать";

                MessageBoxResult result = MessageBox.Show(
                    "Вы действительно хотите " + action +
                    " для учётной записи \"" + account["Login"] + "\"?",
                    "Подтверждение действия",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                userService.SetAccountActive(instructorId, newStatus);

                MessageBox.Show(
                    newStatus
                        ? "Доступ к учётной записи восстановлен."
                        : "Учётная запись заблокирована.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                RefreshInstructors_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось изменить состояние учётной записи.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DeleteInstructorAccount_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedInstructor = InstructorsDataGrid.SelectedItem as DataRowView;

            if (selectedInstructor == null)
            {
                MessageBox.Show(
                    "Выберите инструктора.",
                    "Удаление учётной записи",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            int instructorId = Convert.ToInt32(selectedInstructor["ID"]);

            try
            {
                DataRow account = userService.GetAccountByInstructorId(instructorId);

                if (account == null)
                {
                    MessageBox.Show(
                        "У выбранного инструктора нет учётной записи.",
                        "Удаление учётной записи",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                MessageBoxResult result = MessageBox.Show(
                    "Удалить учётную запись \"" + account["Login"] + "\"?\n\n" +
                    "Профиль инструктора и история его занятий сохранятся.\n" +
                    "Для повторного входа потребуется новое приглашение.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                userService.DeleteAccount(instructorId);

                MessageBox.Show(
                    "Учётная запись удалена. Профиль инструктора сохранён.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                RefreshInstructors_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить учётную запись.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
