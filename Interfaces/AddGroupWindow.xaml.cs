using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Windows;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class AddGroupWindow : Window
    {
        private readonly GroupService groupService = new GroupService();

        public AddGroupWindow()
        {
            InitializeComponent();
        }

        // Добавление группы
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string groupName = GroupNameBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(groupName))
                {
                    MessageBox.Show(
                        "Введите название группы.",
                        "Проверка данных",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    GroupNameBox.Focus();
                    return;
                }

                DateTime? startDate = StartDatePicker.SelectedDate;
                DateTime? endDate = EndDatePicker.SelectedDate;

                groupService.AddGroup(
                    groupName,
                    startDate,
                    endDate);

                MessageBox.Show(
                    "Группа успешно добавлена.",
                    "Добавление группы",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось добавить группу.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Закрытие окна
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
