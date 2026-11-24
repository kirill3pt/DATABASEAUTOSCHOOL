
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleAdminWindow
    {
        // Загрузка списка учеников
        private void LoadStudents(string searchText = "")
        {
            try
            {
                StudentsDataGrid.ItemsSource =
                    studentService.GetStudents(searchText).DefaultView;
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка загрузки учеников: " + ex.Message);

                MessageBox.Show(
                    "Не удалось загрузить список учеников.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Загрузка количества учеников
        private void LoadStudentsCount()
        {
            try
            {
                StudentsCountText.Text =
                    studentService.GetStudentsCount().ToString();
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка загрузки количества учеников: " + ex.Message);
            }
        }

        // Поиск учеников
        private void StudentSearchBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (IsInitialized)
                LoadStudents(StudentSearchBox.Text.Trim());
        }

        // Обновление списка учеников
        private void RefreshStudents_Click(
            object sender,
            RoutedEventArgs e)
        {
            StudentSearchBox.Clear();
            LoadStudents("");

            logger.LOG("Список учеников обновлён.");
        }

        // Удаление выбранных учеников
        private void DeleteStudents_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (StudentsDataGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Выберите одного или нескольких учеников для удаления.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            List<int> ids = new List<int>();

            foreach (DataRowView row in StudentsDataGrid.SelectedItems)
                ids.Add(Convert.ToInt32(row["ID"]));

            MessageBoxResult result = MessageBox.Show(
                $"Вы действительно хотите удалить выбранных учеников ({ids.Count})?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                studentService.DeleteStudents(ids);

                logger.LOG($"Удалено учеников: {ids.Count}");

                LoadStudents();

                MessageBox.Show(
                    $"Успешно удалено учеников: {ids.Count}.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка удаления учеников: " + ex);

                MessageBox.Show(
                    "Не удалось удалить учеников:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            LoadStudentsCount();
        }

        // Добавление ученика
        private void AddStudent_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddStudentWindow window = new AddStudentWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadStudents(StudentSearchBox.Text);
                logger.LOG("Добавлен новый ученик.");
            }

            LoadStudentsCount();
        }

        // Изменение данных ученика
        private void EditStudent_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBox.Show(
                "Окно изменения данных ученика будет подключено следующим шагом.",
                "Редактирование ученика",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Открытие раздела учеников
        private void Students_Click(
            object sender,
            RoutedEventArgs e)
        {
            logger.LOG("Открыт раздел учеников.");

            SetActiveButton(StudentsButton);
            HideAllPanels();

            ShowPanel(StudentsPanel);

            LoadStudents();
        }
    }
}
