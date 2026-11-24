
using System;
using System.Windows;
using System.Windows.Controls;

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


        // Редактирование занятия
        private void EditLesson_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Форма редактирования занятия будет подключена следующим этапом.",
                "Занятия",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Удаление занятий
        private void DeleteLessons_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Удаление занятий будет подключено следующим этапом.",
                "Занятия",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
