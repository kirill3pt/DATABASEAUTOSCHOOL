using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class AddCarWindow : Window
    {
        private readonly CarService carService = new CarService();

        public AddCarWindow()
        {
            InitializeComponent();
        }

        // Автоматическое перемещение между полями номера
        private void Plate_PreviewKeyDown(
            object sender,
            System.Windows.Input.KeyEventArgs e)
        {
            TextBox currentBox = sender as TextBox;

            if (currentBox == null)
                return;

            // Порядок полей государственного номера
            TextBox[] boxes =
            {
        PlateLetterBox,
        PlateDigitsBox,
        PlateLettersBox,
        PlateRegionBox
    };

            int index = Array.IndexOf(boxes, currentBox);

            // Переход назад при Backspace в пустом поле
            if (e.Key == System.Windows.Input.Key.Back &&
                currentBox.SelectionStart == 0 &&
                string.IsNullOrEmpty(currentBox.Text) &&
                index > 0)
            {
                boxes[index - 1].Focus();
                boxes[index - 1].CaretIndex = boxes[index - 1].Text.Length;

                e.Handled = true;
            }
        }

        // Переход вперёд после заполнения поля
        private void Plate_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox currentBox = sender as TextBox;

            if (currentBox == null)
                return;

            if (currentBox.Text.Length < currentBox.MaxLength)
                return;

            TextBox[] boxes =
            {
        PlateLetterBox,
        PlateDigitsBox,
        PlateLettersBox,
        PlateRegionBox
    };

            int index = Array.IndexOf(boxes, currentBox);

            if (index >= 0 && index < boxes.Length - 1)
            {
                boxes[index + 1].Focus();
                boxes[index + 1].CaretIndex = boxes[index + 1].Text.Length;
            }
        }

        // Проверка вводимых символов
        private void Plate_PreviewTextInput(
            object sender,
            System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox box = sender as TextBox;

            if (box == null)
                return;

            if (box.Tag.ToString() == "Letters")
            {
                // Разрешены только буквы, используемые в номерах РФ
                e.Handled = !Regex.IsMatch(
                    e.Text,
                    "^[АВЕКМНОРСТУХавекмнорстух]+$");
            }
            else
            {
                // Разрешены только цифры
                e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
            }
        }

        // Проверка вставляемого текста
        private void Plate_Pasting(
            object sender,
            DataObjectPastingEventArgs e)
        {
            TextBox box = sender as TextBox;

            if (box == null ||
                !e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            string value =
                e.DataObject.GetData(typeof(string)) as string;

            string pattern = box.Tag.ToString() == "Letters"
                ? "^[АВЕКМНОРСТУХавекмнорстух]+$"
                : "^[0-9]+$";

            if (string.IsNullOrEmpty(value) ||
                !Regex.IsMatch(value, pattern) ||
                value.Length > box.MaxLength)
            {
                e.CancelCommand();
            }
        }

        // Сохранение автомобиля
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string brand = BrandBox.Text.Trim();
            string model = ModelBox.Text.Trim();
            string yearText = YearBox.Text.Trim();

            string firstLetter = PlateLetterBox.Text.Trim();
            string digits = PlateDigitsBox.Text.Trim();
            string lastLetters = PlateLettersBox.Text.Trim();
            string region = PlateRegionBox.Text.Trim();

            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(brand) ||
                string.IsNullOrWhiteSpace(model) ||
                string.IsNullOrWhiteSpace(yearText))
            {
                MessageBox.Show(
                    "Заполните все поля автомобиля.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // Проверка государственного номера
            if (firstLetter.Length != 1 ||
                digits.Length != 3 ||
                lastLetters.Length != 2 ||
                (region.Length != 2 && region.Length != 3))
            {
                MessageBox.Show(
                    "Введите корректный государственный номер.\n" +
                    "Формат: А123ВС|77 или А123ВС|777.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            string number =
                firstLetter + digits + lastLetters + region;

            // Проверка года выпуска
            if (!int.TryParse(yearText, out int year))
            {
                MessageBox.Show(
                    "Год выпуска должен быть целым числом.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (year < 1900 || year > DateTime.Now.Year + 1)
            {
                MessageBox.Show(
                    "Указан некорректный год выпуска автомобиля.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                carService.AddCar(brand, model, number, year);

                MessageBox.Show(
                    "Автомобиль успешно добавлен.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось добавить автомобиль:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
