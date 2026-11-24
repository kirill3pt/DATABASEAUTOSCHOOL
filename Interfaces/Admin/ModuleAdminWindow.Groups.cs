
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleAdminWindow
    {
        // Загрузка списка групп
        private void LoadGroups(string searchText = "")
        {
            try
            {
                GroupsDataGrid.ItemsSource =
                    groupService.GetGroups(searchText).DefaultView;
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка загрузки групп: " + ex.Message);

                MessageBox.Show(
                    "Не удалось загрузить список групп.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Открытие раздела групп
        private void Groups_Click(
            object sender,
            RoutedEventArgs e)
        {
            logger.LOG("Открыт раздел учебных групп.");

            SetActiveButton(GroupsButton);
            HideAllPanels();

            ShowPanel(GroupsPanel);

            LoadGroups();
        }

        // Поиск групп
        private void GroupSearchBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            LoadGroups(GroupSearchBox.Text);
        }

        // Обновление списка групп
        private void RefreshGroups_Click(
            object sender,
            RoutedEventArgs e)
        {
            GroupSearchBox.Clear();
            LoadGroups();
        }

        // Добавление группы
        private void AddGroup_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddGroupWindow window = new AddGroupWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadGroups(GroupSearchBox.Text);
                logger.LOG("Добавлена новая учебная группа.");
            }
        }

        // Изменение группы
        private void EditGroup_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBox.Show(
                "Форма редактирования группы будет подключена следующим шагом.",
                "Редактирование группы",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Удаление выбранных групп
        private void DeleteGroup_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (GroupsDataGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Выберите группу для удаления.",
                    "Удаление группы",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            List<int> groupIds = new List<int>();

            foreach (var item in GroupsDataGrid.SelectedItems)
            {
                if (item is DataRowView row)
                    groupIds.Add(Convert.ToInt32(row["ID"]));
            }

            MessageBoxResult result = MessageBox.Show(
                $"Удалить выбранные группы ({groupIds.Count})?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                groupService.DeleteGroups(groupIds);
                LoadGroups(GroupSearchBox.Text);

                logger.LOG(
                    "Удалены группы: " + string.Join(", ", groupIds));

                MessageBox.Show(
                    "Выбранные группы удалены.",
                    "Удаление групп",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка удаления групп: " + ex.Message);

                MessageBox.Show(
                    ex.Message,
                    "Не удалось удалить группы",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}
