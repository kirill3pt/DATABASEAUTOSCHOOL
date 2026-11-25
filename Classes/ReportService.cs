
using System;
using System.Data;
using System.Data.SqlClient;

namespace DATABASEAUTOSCHOOL.Classes
{
    public class ReportService
    {
        public int GetStudentsCount()
        {
            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    SELECT COUNT(*)
                    FROM dbo.vw_StudentDetails";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public int GetLessonsCount()
        {
            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    SELECT COUNT(*)
                    FROM dbo.vw_LessonDetails";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public int GetCompletedLessonsCount()
        {
            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    SELECT COUNT(*)
                    FROM dbo.vw_LessonDetails
                    WHERE LessonStatus = N'проведено'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public decimal GetTotalPayments()
        {
            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    SELECT ISNULL(SUM(Amount), 0)
                    FROM dbo.vw_StudentPayments
                    WHERE PaymentStatus = N'Оплачен'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }

        // Распределение занятий по статусам
        public DataTable GetLessonStatusStatistics()
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    SELECT
                        LessonStatus AS [Статус],
                        COUNT(*) AS [Количество]
                    FROM dbo.vw_LessonDetails
                    GROUP BY LessonStatus
                    ORDER BY LessonStatus";

                using (SqlDataAdapter adapter =
                    new SqlDataAdapter(query, connection))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }

        // Поступления за последние шесть месяцев
        public DataTable GetMonthlyPaymentsStatistics()
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    ;WITH Months AS
                    (
                        SELECT DATEADD(
                            MONTH,
                            -5,
                            DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)
                        ) AS MonthStart

                        UNION ALL

                        SELECT DATEADD(MONTH, 1, MonthStart)
                        FROM Months
                        WHERE MonthStart < DATEFROMPARTS(
                            YEAR(GETDATE()),
                            MONTH(GETDATE()),
                            1
                        )
                    )
                    SELECT
                        m.MonthStart,
                        ISNULL(SUM(
                            CASE
                                WHEN p.PaymentStatus = N'Оплачен'
                                THEN p.Amount
                                ELSE 0
                            END
                        ), 0) AS [Сумма]
                    FROM Months m
                    LEFT JOIN dbo.vw_StudentPayments p
                        ON p.PaymentDate >= m.MonthStart
                        AND p.PaymentDate < DATEADD(MONTH, 1, m.MonthStart)
                    GROUP BY m.MonthStart
                    ORDER BY m.MonthStart
                    OPTION (MAXRECURSION 6)";

                using (SqlDataAdapter adapter =
                    new SqlDataAdapter(query, connection))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }

        // Количество занятий по инструкторам
        public DataTable GetInstructorLessonStatistics()
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    SELECT TOP 10
                        InstructorFIO AS [Инструктор],
                        COUNT(*) AS [Количество занятий]
                    FROM dbo.vw_LessonDetails
                    GROUP BY InstructorFIO
                    ORDER BY COUNT(*) DESC";

                using (SqlDataAdapter adapter =
                    new SqlDataAdapter(query, connection))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }
    }
}
