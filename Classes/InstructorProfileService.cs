
using System.Data;
using System.Data.SqlClient;

namespace DATABASEAUTOSCHOOL.Classes
{
    public class InstructorProfileService
    {
        public DataTable GetProfile(int instructorId)
        {
            DataTable table = new DataTable();

            string query = @"
                SELECT
                    LTRIM(RTRIM(CONCAT(
                        i.LastName, N' ',
                        i.FirstName, N' ',
                        ISNULL(i.MiddleName, N'')
                    ))) AS [ФИО],

                    i.Phone AS [Телефон],
                    u.Login AS [Логин],
                    u.IsActive AS [Активен]

                FROM dbo.Instructors i

                INNER JOIN dbo.Users u
                    ON u.InstructorID = i.InstructorID

                WHERE i.InstructorID = @InstructorID
                  AND u.Role = N'Инструктор'";

            using (SqlConnection connection = DB.GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@InstructorID", SqlDbType.Int).Value =
                    instructorId;

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }
    }
}
