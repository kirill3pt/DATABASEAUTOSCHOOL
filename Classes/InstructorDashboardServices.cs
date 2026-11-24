using System.Data;
using System.Data.SqlClient;

namespace DATABASEAUTOSCHOOL.Classes
{
    public class InstructorDashboardService
    {
        public DataTable GetDashboardStats(int instructorId)
        {
            DataTable table = new DataTable();

            string query = @"
                SELECT
                    (
                        SELECT COUNT(DISTINCT StudentID)
                        FROM dbo.Lessons
                        WHERE InstructorID = @InstructorID
                    ) AS StudentCount,

                    (
                        SELECT COUNT(*)
                        FROM dbo.Lessons
                        WHERE InstructorID = @InstructorID
                    ) AS LessonCount";

            using (SqlConnection connection = DB.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@InstructorID", SqlDbType.Int).Value = instructorId;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            return table;
        }
    }
}