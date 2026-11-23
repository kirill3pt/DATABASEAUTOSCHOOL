using System;
using System.Data.SqlClient;

namespace DATABASEAUTOSCHOOL.Classes
{
    public static class DB
    {
        private static readonly string connectionString =
            @"Data Source=.\SQLEXPRESS;
              Initial Catalog=DrivingSchoolDB;
              Integrated Security=True;
              TrustServerCertificate=True;";

        // Создание подключения к базе данных
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        // Проверка подключения к базе данных
        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "Ошибка подключения к базе данных:\n" + ex.Message,
                    "Ошибка",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );

                return false;
            }
        }
    }
}
