using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Berezhetskiy_K.T.__IVT_2__2_course__MYPROGRAMM
{
    public class XLSX_FUNC_CARS
    {
        ILOGGER logger = new LOGGER();

        // Проверка подключения к базе данных
        public void CREATE()
        {
            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка подключения к базе данных:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка проверки таблицы автомобилей: " + ex.Message);
            }
        }

        // Добавление автомобиля
        public void SAVE(string marka, string model, string number, string year)
        {
            try
            {
                int yearValue;

                if (!int.TryParse(year, out yearValue))
                {
                    MessageBox.Show(
                        "Год выпуска должен быть целым числом.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                if (yearValue < 1886 || yearValue > DateTime.Now.Year + 1)
                {
                    MessageBox.Show(
                        "Указан некорректный год выпуска автомобиля.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        INSERT INTO Cars
                            (Brand, Model, RegistrationNumber, Year)
                        VALUES
                            (@Brand, @Model, @RegistrationNumber, @Year)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Brand", SqlDbType.NVarChar, 50).Value = marka;
                        command.Parameters.Add("@Model", SqlDbType.NVarChar, 50).Value = model;
                        command.Parameters.Add("@RegistrationNumber", SqlDbType.NVarChar, 20).Value = number;
                        command.Parameters.Add("@Year", SqlDbType.Int).Value = yearValue;

                        command.ExecuteNonQuery();
                    }
                }

                logger.LOG("Добавлен автомобиль: " + marka + " " + model + ", " + number);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при добавлении автомобиля:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка добавления автомобиля: " + ex.Message);
            }
        }

        // Отображение автомобилей в DataGridView
        public DataTable SHOW()
        {
            DataTable table = new DataTable();

            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT
                            CarID AS [ID],
                            Brand AS [Марка],
                            Model AS [Модель],
                            RegistrationNumber AS [Гос. номер],
                            Year AS [Год выпуска]
                        FROM Cars
                        ORDER BY CarID";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(table);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при получении списка автомобилей:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка отображения автомобилей: " + ex.Message);
            }

            return table;
        }

        // Заполнение ComboBox автомобилями
        public void MakeCOMBOBOX(ComboBox carBOX)
        {
            try
            {
                DataTable cars = ReadCARS();

                carBOX.Items.Clear();

                foreach (DataRow row in cars.Rows)
                {
                    string marka = row["Марка"].ToString();
                    string model = row["Модель"].ToString();
                    string number = row["Гос. номер"].ToString();

                    carBOX.Items.Add($"{marka} {model}, {number}");
                }

                if (carBOX.Items.Count > 0)
                {
                    carBOX.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка заполнения списка автомобилей:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка заполнения ComboBox автомобилей: " + ex.Message);
            }
        }

        // Чтение всех автомобилей
        public DataTable ReadCARS()
        {
            DataTable table = new DataTable();

            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT
                            CarID AS [ID],
                            Brand AS [Марка],
                            Model AS [Модель],
                            RegistrationNumber AS [Гос. номер],
                            Year AS [Год выпуска]
                        FROM Cars
                        ORDER BY CarID";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(table);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка чтения автомобилей:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка чтения автомобилей: " + ex.Message);
            }

            return table;
        }

        // Удаление выбранных автомобилей
        public void DELETE(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return;

            DialogResult result = MessageBox.Show(
                "Вы действительно хотите удалить выбранные автомобили?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            List<string> parameters = new List<string>();

                            using (SqlCommand command = new SqlCommand())
                            {
                                command.Connection = connection;
                                command.Transaction = transaction;

                                for (int i = 0; i < ids.Count; i++)
                                {
                                    string parameterName = "@id" + i;
                                    parameters.Add(parameterName);

                                    command.Parameters.Add(
                                        parameterName,
                                        SqlDbType.Int
                                    ).Value = ids[i];
                                }

                                command.CommandText =
                                    "DELETE FROM Cars WHERE CarID IN (" +
                                    string.Join(", ", parameters) + ")";

                                int deletedCount = command.ExecuteNonQuery();

                                transaction.Commit();

                                logger.LOG("Удалено автомобилей: " + deletedCount);
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "Не удалось удалить автомобили.\n\n" +
                    "Возможно, некоторые автомобили уже используются в расписании занятий.\n\n" +
                    "Подробности: " + ex.Message,
                    "Ошибка удаления",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                logger.LOG("Ошибка удаления автомобилей: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при удалении автомобилей:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка удаления автомобилей: " + ex.Message);
            }
        }
        // Чтение данных автомобилей для ComboBox
        public DataTable ReadCARS_FOR_COMBOBOX()
        {
            DataTable table = new DataTable();

            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                SELECT
                    CarID,
                    Brand,
                    Model,
                    RegistrationNumber
                FROM Cars
                ORDER BY Brand, Model";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(table);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки списка автомобилей:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка загрузки автомобилей: " + ex.Message);
            }

            return table;
        }
    }
}
