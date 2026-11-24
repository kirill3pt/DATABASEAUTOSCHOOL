using System;
using System.Data;
using System.Data.SqlClient;

namespace DATABASEAUTOSCHOOL.Classes
{
    internal class AUTH : IAUTH
    {
        public bool Authenticate(string role, string login, string password)
        {
            int instructorId;

            return Authenticate(role, login, password, out instructorId);
        }

        public bool Authenticate(
            string role,
            string login,
            string password,
            out int instructorId)
        {
            instructorId = -1;

            if (string.IsNullOrWhiteSpace(role) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrEmpty(password))
                return false;

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT PasswordHash, InstructorID
                    FROM dbo.Users
                    WHERE Login = @Login
                      AND Role = @Role
                      AND IsActive = 1";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@Login", SqlDbType.NVarChar, 100).Value = login;
                    command.Parameters.Add("@Role", SqlDbType.NVarChar, 60).Value = role;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return false;

                        string passwordHash = reader["PasswordHash"].ToString();

                        if (!PasswordHasher.VerifyHash(password, passwordHash))
                            return false;

                        if (role == "Инструктор")
                        {
                            if (reader["InstructorID"] == DBNull.Value)
                                return false;

                            instructorId = Convert.ToInt32(reader["InstructorID"]);
                        }

                        return true;
                    }
                }
            }
        }
    }
}