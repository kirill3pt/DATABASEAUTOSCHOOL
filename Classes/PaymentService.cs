
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

        public void AddPayment(int studentId, decimal amount, DateTime paymentDate, string paymentStatus)
        {
            if (studentId <= 0)
                throw new ArgumentException("Выберите ученика.");

            if (amount <= 0)
                throw new ArgumentException("Сумма платежа должна быть больше нуля.");

            if (paymentStatus != "Оплачен" &&
                paymentStatus != "Ожидает оплаты" &&
                paymentStatus != "Отменён")
            {
                throw new ArgumentException("Некорректный статус платежа.");
            }

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction =
                    connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        decimal tuitionCost;

                        string costQuery = @"
                    SELECT SettingValue
                    FROM dbo.Settings
                    WHERE SettingName = N'TuitionCost'";

                        using (SqlCommand command =
                            new SqlCommand(costQuery, connection, transaction))
                        {
                            object result = command.ExecuteScalar();

                            if (result == null || result == DBNull.Value)
                                throw new Exception(
                                    "Не задана стоимость обучения в настройках.");

                            tuitionCost = Convert.ToDecimal(result);
                        }

                        decimal paidAmount;

                        string paidQuery = @"
                    SELECT COALESCE(SUM(Amount), 0)
                    FROM dbo.Payments WITH (UPDLOCK, HOLDLOCK)
                    WHERE StudentID = @StudentID
                      AND PaymentStatus = N'Оплачен'";

                        using (SqlCommand command =
                            new SqlCommand(paidQuery, connection, transaction))
                        {
                            command.Parameters.Add(
                                "@StudentID", SqlDbType.Int).Value = studentId;

                            paidAmount = Convert.ToDecimal(
                                command.ExecuteScalar());
                        }

                        decimal remaining = tuitionCost - paidAmount;

                        if (paymentStatus == "Оплачен" && amount > remaining)
                        {
                            throw new Exception(
                                "Сумма платежа превышает остаток стоимости обучения.\n\n" +
                                "Стоимость обучения: " + tuitionCost.ToString("N2") + " ₽\n" +
                                "Уже оплачено: " + paidAmount.ToString("N2") + " ₽\n" +
                                "Осталось оплатить: " +
                                Math.Max(remaining, 0).ToString("N2") + " ₽");
                        }

                        string insertQuery = @"
                    INSERT INTO dbo.Payments
                        (StudentID, Amount, PaymentDate, PaymentStatus)
                    VALUES
                        (@StudentID, @Amount, @PaymentDate, @PaymentStatus)";

                        using (SqlCommand command =
                            new SqlCommand(insertQuery, connection, transaction))
                        {
                            command.Parameters.Add(
                                "@StudentID", SqlDbType.Int).Value = studentId;

                            SqlParameter amountParameter =
                                command.Parameters.Add("@Amount", SqlDbType.Decimal);

                            amountParameter.Precision = 10;
                            amountParameter.Scale = 2;
                            amountParameter.Value = amount;

                            command.Parameters.Add(
                                "@PaymentDate", SqlDbType.Date).Value =
                                paymentDate.Date;

                            command.Parameters.Add(
                                "@PaymentStatus", SqlDbType.NVarChar, 60).Value =
                                paymentStatus;

                            command.ExecuteNonQuery();
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

        public decimal GetTuitionCost()
        {
            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
            SELECT SettingValue
            FROM dbo.Settings
            WHERE SettingName = N'TuitionCost'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    object result = command.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        throw new Exception("Не задана стоимость обучения в настройках.");

                    return Convert.ToDecimal(result);
                }
            }
        }

        public DataTable GetStudentPaymentSummary(int studentId)
        {
            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
            SELECT
                s.StudentID,
                LTRIM(RTRIM(
                    s.LastName + N' ' +
                    s.FirstName + N' ' +
                    ISNULL(s.MiddleName, N'')
                )) AS FIO,
                st.SettingValue AS [Стоимость обучения],

                COALESCE(
                    SUM(CASE
                        WHEN p.PaymentStatus = N'Оплачен'
                        THEN p.Amount
                        ELSE 0
                    END), 0
                ) AS [Оплачено],

                st.SettingValue - COALESCE(
                    SUM(CASE
                        WHEN p.PaymentStatus = N'Оплачен'
                        THEN p.Amount
                        ELSE 0
                    END), 0
                ) AS [Осталось],

                CASE
                    WHEN COALESCE(SUM(CASE
                        WHEN p.PaymentStatus = N'Оплачен'
                        THEN p.Amount
                        ELSE 0
                    END), 0) >= st.SettingValue
                        THEN N'Оплачено'

                    WHEN COALESCE(SUM(CASE
                        WHEN p.PaymentStatus = N'Оплачен'
                        THEN p.Amount
                        ELSE 0
                    END), 0) > 0
                        THEN N'Частичная оплата'

                    ELSE N'Не оплачено'
                END AS [Финансовый статус]

            FROM dbo.Students s
            CROSS JOIN dbo.Settings st
            LEFT JOIN dbo.Payments p
                ON s.StudentID = p.StudentID

            WHERE s.StudentID = @StudentID
                AND st.SettingName = N'TuitionCost'

            GROUP BY
                s.StudentID,
                s.LastName,
                s.FirstName,
                s.MiddleName,
                st.SettingValue";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@StudentID", studentId);

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable table = new DataTable();

                adapter.Fill(table);

                return table;
            }
        }

        public void UpdatePaymentStatus(int paymentId, string newStatus)
        {
            if (paymentId <= 0)
                throw new ArgumentException("Некорректный ID платежа.");

            if (newStatus != "Оплачен" &&
                newStatus != "Ожидает оплаты" &&
                newStatus != "Отменён")
                throw new ArgumentException("Недопустимый статус платежа.");

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                SqlTransaction transaction =
                    connection.BeginTransaction(IsolationLevel.Serializable);

                try
                {
                    int studentId;
                    decimal amount;

                    string paymentQuery = @"
                SELECT StudentID, Amount
                FROM dbo.Payments WITH (UPDLOCK, HOLDLOCK)
                WHERE PaymentID = @PaymentID";

                    using (SqlCommand command =
                        new SqlCommand(paymentQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@PaymentID", paymentId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new Exception("Платёж не найден.");

                            studentId = Convert.ToInt32(reader["StudentID"]);
                            amount = Convert.ToDecimal(reader["Amount"]);
                        }
                    }

                    if (newStatus == "Оплачен")
                    {
                        decimal tuitionCost;

                        string costQuery = @"
                    SELECT SettingValue
                    FROM dbo.Settings
                    WHERE SettingName = N'TuitionCost'";

                        using (SqlCommand command =
                            new SqlCommand(costQuery, connection, transaction))
                        {
                            object result = command.ExecuteScalar();

                            if (result == null || result == DBNull.Value)
                                throw new Exception("Не задана стоимость обучения.");

                            tuitionCost = Convert.ToDecimal(result);
                        }

                        string paidQuery = @"
                    SELECT COALESCE(SUM(Amount), 0)
                    FROM dbo.Payments WITH (UPDLOCK, HOLDLOCK)
                    WHERE StudentID = @StudentID
                      AND PaymentStatus = N'Оплачен'
                      AND PaymentID <> @PaymentID";

                        decimal alreadyPaid;

                        using (SqlCommand command =
                            new SqlCommand(paidQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@StudentID", studentId);
                            command.Parameters.AddWithValue("@PaymentID", paymentId);

                            alreadyPaid = Convert.ToDecimal(command.ExecuteScalar());
                        }

                        if (alreadyPaid + amount > tuitionCost)
                        {
                            decimal remaining = tuitionCost - alreadyPaid;

                            throw new Exception(
                                "Нельзя подтвердить платёж: сумма превысит стоимость обучения.\n\n" +
                                "Стоимость обучения: " + tuitionCost.ToString("N2") + " ₽\n" +
                                "Уже оплачено: " + alreadyPaid.ToString("N2") + " ₽\n" +
                                "Осталось оплатить: " + remaining.ToString("N2") + " ₽");
                        }
                    }

                    string updateQuery = @"
                UPDATE dbo.Payments
                SET PaymentStatus = @PaymentStatus
                WHERE PaymentID = @PaymentID";

                    using (SqlCommand command =
                        new SqlCommand(updateQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@PaymentStatus", newStatus);
                        command.Parameters.AddWithValue("@PaymentID", paymentId);

                        command.ExecuteNonQuery();
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
}
