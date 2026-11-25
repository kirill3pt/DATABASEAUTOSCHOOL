
using System.Data;
using System.Data.SqlClient;

namespace DATABASEAUTOSCHOOL.Classes
{
    public class InstructorStudentsService
    {
        public DataTable GetInstructorStudents(int instructorId)
        {
            DataTable table = new DataTable();

            string query = @"
                SELECT
                    s.StudentID AS [ID],

                    LTRIM(RTRIM(CONCAT(
                        s.LastName, N' ',
                        s.FirstName, N' ',
                        ISNULL(s.MiddleName, N'')
                    ))) AS [ФИО],

                    s.Phone AS [Телефон],
                    g.GroupName AS [Группа],
                    s.Status AS [Статус],

                    COUNT(l.LessonID) AS [Количество занятий]

                FROM dbo.Students s

                INNER JOIN dbo.Groups g
                    ON s.GroupID = g.GroupID

                INNER JOIN dbo.Lessons l
                    ON s.StudentID = l.StudentID

                WHERE l.InstructorID = @InstructorID

                GROUP BY
                    s.StudentID,
                    s.LastName,
                    s.FirstName,
                    s.MiddleName,
                    s.Phone,
                    g.GroupName,
                    s.Status

                ORDER BY s.LastName, s.FirstName";

            using (SqlConnection connection = DB.GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add(
                    "@InstructorID",
                    SqlDbType.Int).Value = instructorId;

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }
    }
}
