
using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class AddPaymentWindow : Window
    {
        private readonly PaymentService paymentService = new PaymentService();

        private DateTime? selectedPaymentDate = DateTime.Today;
        private DateTime displayedPaymentMonth = DateTime.Today;

        public AddPaymentWindow()
        {
            InitializeComponent();

            PaymentDateText.Text = DateTime.Today.ToString("dd.MM.yyyy");
            UpdatePaymentCalendar();

            LoadStudents();

            PaymentStatusComboBox.SelectedIndex = 0;
        }

        private void PaymentCalendarToggleButton_Click(object sender, RoutedEventArgs e)
        {
            PaymentCalendarPopup.IsOpen = !PaymentCalendarPopup.IsOpen;
        }

        private void PaymentPreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            displayedPaymentMonth = displayedPaymentMonth.AddMonths(-1);
            UpdatePaymentCalendar();
        }

        private void PaymentNextMonth_Click(object sender, RoutedEventArgs e)
        {
            displayedPaymentMonth = displayedPaymentMonth.AddMonths(1);
            UpdatePaymentCalendar();
        }

        private void UpdatePaymentCalendar()
        {
            PaymentCalendarMonthText.Text = displayedPaymentMonth.ToString(
                "MMMM yyyy",
                new CultureInfo("ru-RU"));

            PaymentCalendarDaysPanel.Children.Clear();

            DateTime firstDay = new DateTime(
                displayedPaymentMonth.Year,
                displayedPaymentMonth.Month,
                1);

            int offset = ((int)firstDay.DayOfWeek + 6) % 7;

            int daysInMonth = DateTime.DaysInMonth(
                displayedPaymentMonth.Year,
                displayedPaymentMonth.Month);

            for (int i = 0; i < offset; i++)
            {
                PaymentCalendarDaysPanel.Children.Add(new Border
                {
                    Height = 36
                });
            }

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(
                    displayedPaymentMonth.Year,
                    displayedPaymentMonth.Month,
                    day);

                Button dayButton = new Button
                {
                    Content = day.ToString(),
                    Tag = date,
                    Height = 36,
                    Margin = new Thickness(2),
                    FontSize = 13,
                    FontWeight = FontWeights.Normal,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Style = (Style)FindResource("CalendarButtonStyle"),
                    Background = Brushes.Transparent,
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(41, 51, 48))
                };

                if (selectedPaymentDate.HasValue &&
                    selectedPaymentDate.Value.Date == date.Date)
                {
                    dayButton.Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#A8B5A5"));

                    dayButton.FontWeight = FontWeights.SemiBold;
                }

                dayButton.Click += PaymentCalendarDay_Click;

                PaymentCalendarDaysPanel.Children.Add(dayButton);
            }
        }

        private void PaymentCalendarDay_Click(object sender, RoutedEventArgs e)
        {
            Button dayButton = sender as Button;

            if (dayButton == null)
                return;

            selectedPaymentDate = (DateTime)dayButton.Tag;

            PaymentDateText.Text = selectedPaymentDate.Value.ToString("dd.MM.yyyy");
            PaymentCalendarPopup.IsOpen = false;

            UpdatePaymentCalendar();
        }

        private void LoadStudents()
        {
            try
            {
                DataTable students = paymentService.GetStudentsForComboBox();

                StudentComboBox.SelectedValuePath = "StudentID";
                StudentComboBox.ItemsSource = students.DefaultView;

                if (StudentComboBox.Items.Count > 0)
                    StudentComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить список учеников.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (StudentComboBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Выберите ученика.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!decimal.TryParse(
                AmountBox.Text.Trim().Replace(',', '.'),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal amount) || amount <= 0)
            {
                MessageBox.Show(
                    "Введите корректную сумму платежа.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                AmountBox.Focus();
                return;
            }

            if (!selectedPaymentDate.HasValue)
            {
                MessageBox.Show(
                    "Укажите дату платежа.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (PaymentStatusComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите статус платежа.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                int studentId = Convert.ToInt32(StudentComboBox.SelectedValue);

                string status =
                    ((ComboBoxItem)PaymentStatusComboBox.SelectedItem).Content.ToString();

                paymentService.AddPayment(
                    studentId,
                    amount,
                    selectedPaymentDate.Value,
                    status);

                MessageBox.Show(
                    "Платёж успешно добавлен.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось добавить платёж.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
