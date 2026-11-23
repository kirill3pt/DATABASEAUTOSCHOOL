
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace DATABASEAUTOSCHOOL.Classes
{
    public class CarService
    {
        private readonly ILOGGER logger = new LOGGER();

        // Получение списка автомобилей
        public DataTable GetCars()
        {
            DataTable table = new DataTable();

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

            return table;
        }

        // Добавление автомобиля
        public void AddCar(string brand, string model, string number, int year)
        {
            try
            {
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
                        command.Parameters.Add("@Brand", SqlDbType.NVarChar, 50).Value = brand;
                        command.Parameters.Add("@Model", SqlDbType.NVarChar, 50).Value = model;
                        command.Parameters.Add("@RegistrationNumber", SqlDbType.NVarChar, 20).Value = number;
                        command.Parameters.Add("@Year", SqlDbType.Int).Value = year;

                        command.ExecuteNonQuery();
                    }
                }

                logger.LOG("Добавлен автомобиль: " + brand + " " + model + ", " + number);
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка добавления автомобиля: " + ex.Message);
                throw;
            }
        }

        // Удаление автомобилей
        public void DeleteCars(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
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
                                    string parameter = "@id" + i;
                                    parameters.Add(parameter);

                                    command.Parameters.Add(parameter, SqlDbType.Int).Value = ids[i];
                                }

                                command.CommandText =
                                    "DELETE FROM Cars WHERE CarID IN (" +
                                    string.Join(", ", parameters) + ")";

                                int count = command.ExecuteNonQuery();

                                transaction.Commit();

                                logger.LOG("Удалено автомобилей: " + count);
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
            catch (Exception ex)
            {
                logger.LOG("Ошибка удаления автомобилей: " + ex.Message);
                throw;
            }
        }

        // Получение автомобилей для списков выбора
        public DataTable GetCarsForComboBox()
        {
            DataTable table = new DataTable();

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

            return table;
        }
    }
}
