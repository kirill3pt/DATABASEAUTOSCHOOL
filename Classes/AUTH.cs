using System.Data.SqlClient;

namespace DATABASEAUTOSCHOOL.Classes
{
    internal class AUTH : IAUTH
    {
        public bool Authenticate(string role, string login, string password)
        {
            if (string.IsNullOrWhiteSpace(role) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrEmpty(password))
                return false;

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT PasswordHash
                    FROM Users
                    WHERE Login = @Login AND Role = @Role";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Role", role);

                    object result = command.ExecuteScalar();

                    if (result == null || result == System.DBNull.Value)
                        return false;

                    return PasswordHasher.VerifyHash(
                        password,
                        result.ToString()
                    );
                }
            }
        }
    }
}