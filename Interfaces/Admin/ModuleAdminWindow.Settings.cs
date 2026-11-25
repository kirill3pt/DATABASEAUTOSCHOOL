
using DATABASEAUTOSCHOOL.Classes;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class ModuleAdminWindow
    {
        private void LoadTuitionCost()
        {
            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT SettingValue
                        FROM dbo.Settings
                        WHERE SettingName = @SettingName";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@SettingName", SqlDbType.NVarChar, 100).Value = "TuitionCost";

                        object result = command.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            decimal cost = Convert.ToDecimal(result);

                            TuitionCostTextBox.Text =
                                cost.ToString("0.00", CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            MessageBox.Show(
                                "Настройка стоимости обучения не найдена.",
                                "Настройки",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить стоимость обучения.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SaveTuitionCost_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(
                TuitionCostTextBox.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out decimal cost) || cost <= 0 || cost > 99999999.99m)
            {
                MessageBox.Show(
                    "Введите корректную стоимость обучения.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                TuitionCostTextBox.Focus();
                return;
            }

            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        UPDATE dbo.Settings
                        SET SettingValue = @SettingValue
                        WHERE SettingName = @SettingName";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlParameter costParameter =
                            command.Parameters.Add("@SettingValue", SqlDbType.Decimal);

                        costParameter.Precision = 10;
                        costParameter.Scale = 2;
                        costParameter.Value = cost;

                        command.Parameters.Add("@SettingName", SqlDbType.NVarChar, 100).Value = "TuitionCost";

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            MessageBox.Show(
                                "Настройка стоимости обучения не найдена.",
                                "Настройки",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }
                    }
                }

                MessageBox.Show(
                    "Стоимость обучения успешно обновлена.",
                    "Настройки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось сохранить стоимость обучения.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
