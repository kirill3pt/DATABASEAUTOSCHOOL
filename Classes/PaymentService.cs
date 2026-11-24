
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DATABASEAUTOSCHOOL.Classes
{
    public class PaymentService
    {
        // Получение списка платежей
        public DataTable GetPayments(string searchText = "")
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    SELECT
                        PaymentID AS [ID],
                        FIO AS [Ученик],
                        Phone AS [Телефон],
                        GroupName AS [Группа],
                        Amount AS [Сумма],
                        PaymentDate AS [Дата платежа],
                        PaymentStatus AS [Статус]
                    FROM dbo.vw_StudentPayments
                    WHERE
                        @SearchText = ''
                        OR FIO LIKE @SearchPattern
                        OR Phone LIKE @SearchPattern
                        OR GroupName LIKE @SearchPattern
                        OR PaymentStatus LIKE @SearchPattern
                    ORDER BY PaymentDate DESC, PaymentID DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchText", searchText.Trim());
                    command.Parameters.AddWithValue(
                        "@SearchPattern",
                        "%" + searchText.Trim() + "%");

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            return table;
        }


        public DataTable GetPaymentsByStudent(int studentId)
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
            SELECT
                PaymentID AS [ID],
                Amount AS [Сумма],
                PaymentDate AS [Дата платежа],
                PaymentStatus AS [Статус]
            FROM dbo.vw_StudentPayments
            WHERE StudentID = @StudentID
            ORDER BY PaymentDate DESC, PaymentID DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@StudentID", SqlDbType.Int).Value = studentId;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            return table;
        }


        // Удаление выбранных платежей
        public void DeletePayments(List<int> paymentIds)
        {
            if (paymentIds == null || paymentIds.Count == 0)
                throw new ArgumentException("Не выбраны платежи для удаления.");

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (int paymentId in paymentIds)
                        {
                            string query = @"
                                DELETE FROM dbo.Payments
                                WHERE PaymentID = @PaymentID";

                            using (SqlCommand command = new SqlCommand(
                                query, connection, transaction))
                            {
                                command.Parameters.AddWithValue(
                                    "@PaymentID", paymentId);

                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public DataTable GetStudentsForComboBox()
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
            SELECT
                StudentID,
                LTRIM(RTRIM(
                    LastName + N' ' + FirstName +
                    CASE
                        WHEN MiddleName IS NOT NULL AND MiddleName <> ''
                        THEN N' ' + MiddleName
                        ELSE N''
                    END
                )) AS FIO
            FROM dbo.Students
            ORDER BY LastName, FirstName";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }
        public void AddPayment(
    int studentId,
    decimal amount,
    DateTime paymentDate,
    string paymentStatus)
        {
            if (studentId <= 0)
                throw new ArgumentException("Выберите ученика.");

            if (amount <= 0)
                throw new ArgumentException("Сумма платежа должна быть больше нуля.");

            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
            INSERT INTO dbo.Payments
                (StudentID, Amount, PaymentDate, PaymentStatus)
            VALUES
                (@StudentID, @Amount, @PaymentDate, @PaymentStatus)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@StudentID", SqlDbType.Int).Value = studentId;
                    command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = amount;
                    command.Parameters["@Amount"].Precision = 10;
                    command.Parameters["@Amount"].Scale = 2;

                    command.Parameters.Add("@PaymentDate", SqlDbType.Date).Value =
                        paymentDate.Date;

                    command.Parameters.Add("@PaymentStatus", SqlDbType.NVarChar, 60).Value =
                        paymentStatus;

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
