using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleAdminWindow
    {

        // Открытие раздела занятий
        private void Lessons_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(LessonsButton);
            HideAllPanels();

            ShowPanel(LessonsPanel);

            LoadLessons();

            logger.LOG("Открыт раздел занятий.");
        }

        // Загрузка занятий
        private void LoadLessons(string searchText = "")
        {
            try
            {
                LessonsDataGrid.ItemsSource =
                    lessonService.GetLessons(searchText).DefaultView;
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка загрузки занятий: " + ex.Message);

                MessageBox.Show(
                    "Не удалось загрузить занятия.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Поиск занятий
        private void LessonSearchBox_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            if (IsInitialized)
                LoadLessons(LessonSearchBox.Text);
        }

        // Обновление списка занятий
        private void RefreshLessons_Click(
            object sender, RoutedEventArgs e)
        {
            LessonSearchBox.Clear();
            LoadLessons();
        }

        // Добавление занятия

        private void AddLesson_Click(object sender, RoutedEventArgs e)
        {
            AddLessonWindow window = new AddLessonWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadLessons(LessonSearchBox.Text);
                logger.LOG("Добавлено новое занятие.");
            }
        }


        private void EditLesson_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow =
                LessonsDataGrid.SelectedItem as DataRowView;

            if (selectedRow == null)
            {
                MessageBox.Show(
                    "Выберите занятие для редактирования.",
                    "Занятия",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            EditLessonWindow window = new EditLessonWindow(
                Convert.ToInt32(selectedRow["ID"]),
                Convert.ToInt32(selectedRow["StudentID"]),
                Convert.ToInt32(selectedRow["InstructorID"]),
                Convert.ToInt32(selectedRow["CarID"]),
                Convert.ToDateTime(selectedRow["Дата"]),
                TimeSpan.Parse(selectedRow["Время"].ToString()),
                selectedRow["Статус"].ToString());

            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadLessons(LessonSearchBox.Text);
                logger.LOG("Изменено занятие.");
            }
        }

        // Удаление занятий
        private void DeleteLessons_Click(object sender, RoutedEventArgs e)
        {
            if (LessonsDataGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Выберите одно или несколько занятий для удаления.",
                    "Удаление занятий",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            List<int> lessonIds = new List<int>();

            foreach (DataRowView row in LessonsDataGrid.SelectedItems)
            {
                lessonIds.Add(Convert.ToInt32(row["ID"]));
            }

            MessageBoxResult result = MessageBox.Show(
                "Вы действительно хотите удалить выбранные занятия (" +
                lessonIds.Count + " шт.)?\n\nЭто действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                lessonService.DeleteLessons(lessonIds);

                LoadLessons(LessonSearchBox.Text);

                logger.LOG(
                    "Удалено занятий: " + lessonIds.Count);

                MessageBox.Show(
                    "Выбранные занятия успешно удалены.",
                    "Удаление занятий",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка удаления занятий: " + ex.Message);

                MessageBox.Show(
                    "Не удалось удалить занятия.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
