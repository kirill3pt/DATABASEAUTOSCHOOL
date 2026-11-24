using System;
using System.Data;
using System.Data.SqlClient;

namespace DATABASEAUTOSCHOOL.Classes
{
    public class UserService
    {
        public DataRow GetAccountByInstructorId(int instructorId)
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    SELECT
                        UserID,
                        Login,
                        Role,
                        IsActive
                    FROM dbo.Users
                    WHERE InstructorID = @InstructorID
                      AND Role = N'Инструктор'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@InstructorID", SqlDbType.Int)
                        .Value = instructorId;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            if (table.Rows.Count == 0)
                return null;

            return table.Rows[0];
        }

        public void SetAccountActive(int instructorId, bool isActive)
        {
            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    UPDATE dbo.Users
                    SET IsActive = @IsActive
                    WHERE InstructorID = @InstructorID
                      AND Role = N'Инструктор'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@InstructorID", SqlDbType.Int)
                        .Value = instructorId;

                    command.Parameters.Add("@IsActive", SqlDbType.Bit)
                        .Value = isActive;

                    connection.Open();

                    if (command.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException(
                            "Учётная запись инструктора не найдена.");
                }
            }
        }

        public void DeleteAccount(int instructorId)
        {
            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    DELETE FROM dbo.Users
                    WHERE InstructorID = @InstructorID
                      AND Role = N'Инструктор'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@InstructorID", SqlDbType.Int)
                        .Value = instructorId;

                    connection.Open();

                    if (command.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException(
                            "Учётная запись инструктора не найдена.");
                }
            }
        }
    }
}