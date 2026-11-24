
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleAdminWindow
    {
        // Загрузка списка автомобилей
        private void LoadCars()
        {
            try
            {
                CarsDataGrid.ItemsSource = carService.GetCars().DefaultView;

                logger.LOG("Список автомобилей успешно загружен.");
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка загрузки автомобилей: " + ex);

                MessageBox.Show(
                    "Не удалось загрузить автомобили:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Открытие раздела автомобилей
        private void Cars_Click(object sender, RoutedEventArgs e)
        {
            logger.LOG("Открыт раздел автомобилей.");

            SetActiveButton(CarsButton);
            HideAllPanels();

            ShowPanel(CarsPanel);

            LoadCars();
        }

        // Добавление автомобиля
        private void AddCar_Click(object sender, RoutedEventArgs e)
        {
            logger.LOG("Администратор открыл окно добавления автомобиля.");

            AddCarWindow window = new AddCarWindow
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                logger.LOG("Автомобиль успешно добавлен.");
                LoadCars();
            }
            else
            {
                logger.LOG("Добавление автомобиля отменено.");
            }
        }

        // Удаление выбранных автомобилей
        private void DeleteCars_Click(object sender, RoutedEventArgs e)
        {
            if (CarsDataGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Выберите один или несколько автомобилей для удаления.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                logger.LOG("Попытка удаления автомобилей без выбора записей.");
                return;
            }

            List<int> ids = new List<int>();

            foreach (DataRowView row in CarsDataGrid.SelectedItems)
            {
                ids.Add(Convert.ToInt32(row["ID"]));
            }

            MessageBoxResult result = MessageBox.Show(
                $"Вы действительно хотите удалить выбранные автомобили ({ids.Count})?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                logger.LOG("Удаление автомобилей отменено пользователем.");
                return;
            }

            try
            {
                carService.DeleteCars(ids);

                logger.LOG(
                    $"Удалено автомобилей: {ids.Count}. ID: {string.Join(", ", ids)}");

                LoadCars();

                MessageBox.Show(
                    $"Успешно удалено автомобилей: {ids.Count}.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка удаления автомобилей: " + ex);

                MessageBox.Show(
                    "Не удалось удалить автомобили:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Обновление таблицы автомобилей
        private void RefreshCars_Click(object sender, RoutedEventArgs e)
        {
            logger.LOG("Администратор обновил список автомобилей.");
            LoadCars();
        }
    }
}
