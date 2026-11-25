
using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleAdminWindow
    {
        private DataTable lessonStatusData;
        private DataTable monthlyPaymentsData;
        private DataTable instructorLessonsData;

        private bool reportsChartEventsAttached;

        private void LoadReportsStatistics()
        {
            try
            {
                int completedLessons = reportService.GetCompletedLessonsCount();
                decimal totalPayments = reportService.GetTotalPayments();

                ReportCompletedLessonsText.Text =
                    completedLessons.ToString();

                ReportPaymentsSumText.Text =
                    totalPayments.ToString("N2") + " ₽";

                lessonStatusData =
                    reportService.GetLessonStatusStatistics();

                monthlyPaymentsData =
                    reportService.GetMonthlyPaymentsStatistics();

                instructorLessonsData =
                    reportService.GetInstructorLessonStatistics();

                if (!reportsChartEventsAttached)
                {
                    LessonStatusChart.SizeChanged += ReportsChart_SizeChanged;
                    MonthlyPaymentsChart.SizeChanged += ReportsChart_SizeChanged;
                    InstructorLessonsChart.SizeChanged += ReportsChart_SizeChanged;

                    reportsChartEventsAttached = true;
                }

                Dispatcher.BeginInvoke(
                    DispatcherPriority.Loaded,
                    new Action(DrawReportsCharts));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить статистику отчётов.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ReportsChart_SizeChanged(
            object sender,
            SizeChangedEventArgs e)
        {
            DrawReportsCharts();
        }

        private void DrawReportsCharts()
        {
            if (LessonStatusChart.ActualWidth > 0 &&
                LessonStatusChart.ActualHeight > 0)
            {
                DrawLessonStatusChart();
            }

            if (MonthlyPaymentsChart.ActualWidth > 0 &&
                MonthlyPaymentsChart.ActualHeight > 0)
            {
                DrawMonthlyPaymentsChart();
            }

            if (InstructorLessonsChart.ActualWidth > 0 &&
                InstructorLessonsChart.ActualHeight > 0)
            {
                DrawInstructorLessonsChart();
            }
        }

        // Распределение занятий по статусам
        private void DrawLessonStatusChart()
        {
            LessonStatusChart.Children.Clear();

            if (lessonStatusData == null ||
                lessonStatusData.Rows.Count == 0)
            {
                ShowChartMessage(LessonStatusChart, "Нет данных");
                return;
            }

            double total = 0;

            foreach (DataRow row in lessonStatusData.Rows)
            {
                total += Convert.ToDouble(row["Количество"]);
            }

            if (total <= 0)
            {
                ShowChartMessage(LessonStatusChart, "Нет данных");
                return;
            }

            double width = LessonStatusChart.ActualWidth;
            double height = LessonStatusChart.ActualHeight;

            double radius = Math.Min(
                height * 0.36,
                width * 0.22);

            if (radius < 15)
                return;

            Point center = new Point(
                radius + 15,
                height / 2);

            Brush[] colors =
            {
                CreateBrush("#A8B5A5"),
                CreateBrush("#293330"),
                CreateBrush("#D6A06D"),
                CreateBrush("#A96D60"),
                CreateBrush("#8296A0")
            };

            double currentAngle = -90;

            for (int i = 0; i < lessonStatusData.Rows.Count; i++)
            {
                DataRow row = lessonStatusData.Rows[i];

                double value = Convert.ToDouble(row["Количество"]);
                double angle = value / total * 360;

                if (angle >= 359.9)
                {
                    Ellipse circle = new Ellipse
                    {
                        Width = radius * 2,
                        Height = radius * 2,
                        Fill = colors[i % colors.Length],
                        Stroke = CreateBrush("#FAF9F6"),
                        StrokeThickness = 2
                    };

                    Canvas.SetLeft(circle, center.X - radius);
                    Canvas.SetTop(circle, center.Y - radius);

                    LessonStatusChart.Children.Add(circle);
                }
                else
                {
                    double startRadians = currentAngle * Math.PI / 180;
                    double endRadians = (currentAngle + angle) * Math.PI / 180;

                    Point startPoint = new Point(
                        center.X + radius * Math.Cos(startRadians),
                        center.Y + radius * Math.Sin(startRadians));

                    Point endPoint = new Point(
                        center.X + radius * Math.Cos(endRadians),
                        center.Y + radius * Math.Sin(endRadians));

                    PathFigure figure = new PathFigure
                    {
                        StartPoint = center,
                        IsClosed = true
                    };

                    figure.Segments.Add(
                        new LineSegment(startPoint, true));

                    figure.Segments.Add(
                        new ArcSegment(
                            endPoint,
                            new Size(radius, radius),
                            0,
                            angle > 180,
                            SweepDirection.Clockwise,
                            true));

                    figure.Segments.Add(
                        new LineSegment(center, true));

                    PathGeometry geometry = new PathGeometry();
                    geometry.Figures.Add(figure);

                    Path sector = new Path
                    {
                        Data = geometry,
                        Fill = colors[i % colors.Length],
                        Stroke = CreateBrush("#FAF9F6"),
                        StrokeThickness = 2
                    };

                    LessonStatusChart.Children.Add(sector);
                }

                currentAngle += angle;
            }

            // Легенда
            double legendX = radius * 2 + 40;
            double legendY = Math.Max(5, height / 2 -
                lessonStatusData.Rows.Count * 15);

            for (int i = 0; i < lessonStatusData.Rows.Count; i++)
            {
                DataRow row = lessonStatusData.Rows[i];

                Rectangle marker = new Rectangle
                {
                    Width = 10,
                    Height = 10,
                    Fill = colors[i % colors.Length],
                    RadiusX = 2,
                    RadiusY = 2
                };

                Canvas.SetLeft(marker, legendX);
                Canvas.SetTop(marker, legendY + i * 30);

                LessonStatusChart.Children.Add(marker);

                string status = Convert.ToString(row["Статус"]);
                string count = Convert.ToString(row["Количество"]);

                TextBlock label = new TextBlock
                {
                    Text = status + " — " + count,
                    FontSize = 11,
                    Foreground = CreateBrush("#293330"),
                    Width = Math.Max(80, width - legendX - 10),
                    TextTrimming = TextTrimming.CharacterEllipsis
                };

                Canvas.SetLeft(label, legendX + 17);
                Canvas.SetTop(label, legendY - 2 + i * 30);

                LessonStatusChart.Children.Add(label);
            }
        }

        // Поступления за последние шесть месяцев
        private void DrawMonthlyPaymentsChart()
        {
            MonthlyPaymentsChart.Children.Clear();

            if (monthlyPaymentsData == null ||
                monthlyPaymentsData.Rows.Count == 0)
            {
                ShowChartMessage(MonthlyPaymentsChart, "Нет данных");
                return;
            }

            double width = MonthlyPaymentsChart.ActualWidth;
            double height = MonthlyPaymentsChart.ActualHeight;

            double maxValue = 0;

            foreach (DataRow row in monthlyPaymentsData.Rows)
            {
                double value = Convert.ToDouble(row["Сумма"]);

                if (value > maxValue)
                    maxValue = value;
            }

            double chartTop = 25;
            double chartBottom = height - 35;
            double chartHeight = chartBottom - chartTop;

            if (chartHeight <= 0)
                return;

            if (maxValue <= 0)
            {
                ShowChartMessage(MonthlyPaymentsChart, "Поступлений пока нет");
                return;
            }

            int count = monthlyPaymentsData.Rows.Count;

            double slotWidth = width / count;
            double barWidth = Math.Min(35, slotWidth * 0.55);

            for (int i = 0; i < count; i++)
            {
                DataRow row = monthlyPaymentsData.Rows[i];

                double value = Convert.ToDouble(row["Сумма"]);

                double barHeight = value / maxValue * chartHeight;

                double x = i * slotWidth +
                    (slotWidth - barWidth) / 2;

                double y = chartBottom - barHeight;

                Rectangle bar = new Rectangle
                {
                    Width = barWidth,
                    Height = Math.Max(2, barHeight),
                    Fill = CreateBrush("#A8B5A5"),
                    RadiusX = 4,
                    RadiusY = 4
                };

                Canvas.SetLeft(bar, x);
                Canvas.SetTop(bar, y);

                MonthlyPaymentsChart.Children.Add(bar);

                TextBlock valueText = new TextBlock
                {
                    Text = value.ToString("N0"),
                    FontSize = 10,
                    Foreground = CreateBrush("#737D76"),
                    TextAlignment = TextAlignment.Center,
                    Width = slotWidth
                };

                Canvas.SetLeft(valueText, i * slotWidth);
                Canvas.SetTop(valueText, Math.Max(0, y - 19));

                MonthlyPaymentsChart.Children.Add(valueText);

                DateTime month = Convert.ToDateTime(row["MonthStart"]);

                TextBlock monthText = new TextBlock
                {
                    Text = month.ToString(
                        "MMM",
                        new CultureInfo("ru-RU")),
                    FontSize = 10,
                    Foreground = CreateBrush("#737D76"),
                    TextAlignment = TextAlignment.Center,
                    Width = slotWidth
                };

                Canvas.SetLeft(monthText, i * slotWidth);
                Canvas.SetTop(monthText, chartBottom + 8);

                MonthlyPaymentsChart.Children.Add(monthText);
            }
        }

        // Количество занятий по инструкторам
        private void DrawInstructorLessonsChart()
        {
            InstructorLessonsChart.Children.Clear();

            if (instructorLessonsData == null ||
                instructorLessonsData.Rows.Count == 0)
            {
                ShowChartMessage(InstructorLessonsChart, "Нет данных");
                return;
            }

            double width = InstructorLessonsChart.ActualWidth;
            double height = InstructorLessonsChart.ActualHeight;

            int count = instructorLessonsData.Rows.Count;

            double maxValue = 0;

            foreach (DataRow row in instructorLessonsData.Rows)
            {
                double value =
                    Convert.ToDouble(row["Количество занятий"]);

                if (value > maxValue)
                    maxValue = value;
            }

            if (maxValue <= 0)
            {
                ShowChartMessage(InstructorLessonsChart, "Нет данных");
                return;
            }

            double rowHeight = height / count;
            double labelWidth = Math.Min(155, width * 0.3);
            double barStart = labelWidth + 10;
            double availableWidth = width - barStart - 45;

            if (availableWidth <= 0)
                return;

            double barHeight = Math.Min(18, rowHeight * 0.5);

            for (int i = 0; i < count; i++)
            {
                DataRow row = instructorLessonsData.Rows[i];

                string instructor =
                    Convert.ToString(row["Инструктор"]);

                double value =
                    Convert.ToDouble(row["Количество занятий"]);

                double y = i * rowHeight +
                    (rowHeight - barHeight) / 2;

                TextBlock nameText = new TextBlock
                {
                    Text = instructor,
                    Width = labelWidth,
                    FontSize = 11,
                    Foreground = CreateBrush("#293330"),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Canvas.SetLeft(nameText, 0);
                Canvas.SetTop(nameText, y - 3);

                InstructorLessonsChart.Children.Add(nameText);

                double barWidth = value / maxValue * availableWidth;

                Rectangle bar = new Rectangle
                {
                    Width = Math.Max(2, barWidth),
                    Height = barHeight,
                    Fill = CreateBrush("#A8B5A5"),
                    RadiusX = 4,
                    RadiusY = 4
                };

                Canvas.SetLeft(bar, barStart);
                Canvas.SetTop(bar, y);

                InstructorLessonsChart.Children.Add(bar);

                TextBlock valueText = new TextBlock
                {
                    Text = value.ToString("N0"),
                    FontSize = 11,
                    Foreground = CreateBrush("#737D76")
                };

                Canvas.SetLeft(valueText, barStart + barWidth + 7);
                Canvas.SetTop(valueText, y - 3);

                InstructorLessonsChart.Children.Add(valueText);
            }
        }

        private void ShowChartMessage(Canvas canvas, string message)
        {
            canvas.Children.Clear();

            TextBlock text = new TextBlock
            {
                Text = message,
                FontSize = 12,
                Foreground = CreateBrush("#737D76")
            };

            canvas.Children.Add(text);

            Canvas.SetLeft(
                text,
                Math.Max(5, (canvas.ActualWidth - 120) / 2));

            Canvas.SetTop(
                text,
                Math.Max(5, (canvas.ActualHeight - 25) / 2));
        }

        private Brush CreateBrush(string color)
        {
            return new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(color));
        }

        private void OpenStudentsReport_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBox.Show(
                "Отчёт по ученикам будет подключён на следующем этапе.",
                "Отчёты",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OpenLessonsReport_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBox.Show(
                "Отчёт по занятиям будет подключён на следующем этапе.",
                "Отчёты",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OpenPaymentsReport_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBox.Show(
                "Финансовый отчёт будет подключён на следующем этапе.",
                "Отчёты",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
 