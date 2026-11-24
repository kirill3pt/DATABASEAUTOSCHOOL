
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DATABASEAUTOSCHOOL.Classes
{
    public class InstructorService
    {
        public DataTable GetInstructors(string searchText = "")
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    SELECT
                        i.InstructorID AS [ID],
                        i.LastName AS [Фамилия],
                        i.FirstName AS [Имя],
                        i.MiddleName AS [Отчество],
                        i.Phone AS [Телефон],
                        CASE
                            WHEN u.UserID IS NULL THEN N'Не зарегистрирован'
                            WHEN u.IsActive = 0 THEN N'Заблокирован'
                            ELSE N'Зарегистрирован'
                        END AS [Регистрация]
                    FROM dbo.Instructors i
                    LEFT JOIN dbo.Users u
                        ON i.InstructorID = u.InstructorID
                    WHERE
                        i.LastName LIKE @SearchText
                        OR i.FirstName LIKE @SearchText
                        OR ISNULL(i.MiddleName, N'') LIKE @SearchText
                        OR i.Phone LIKE @SearchText
                    ORDER BY i.LastName, i.FirstName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@SearchText", SqlDbType.NVarChar, 100)
                        .Value = "%" + (searchText ?? "").Trim() + "%";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            return table;
        }

        public void AddInstructor(
            string lastName,
            string firstName,
            string middleName,
            string phone)
        {
            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    INSERT INTO dbo.Instructors
                        (LastName, FirstName, MiddleName, Phone)
                    VALUES
                        (@LastName, @FirstName, @MiddleName, @Phone)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = lastName;
                    command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = firstName;
                    command.Parameters.Add("@MiddleName", SqlDbType.NVarChar, 100).Value =
                        string.IsNullOrWhiteSpace(middleName) ? (object)DBNull.Value : middleName;
                    command.Parameters.Add("@Phone", SqlDbType.NVarChar, 30).Value = phone;

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public DataRow GetInstructorById(int instructorId)
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
            SELECT
                InstructorID,
                LastName,
                FirstName,
                MiddleName,
                Phone
            FROM dbo.Instructors
            WHERE InstructorID = @InstructorID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@InstructorID", SqlDbType.Int).Value = instructorId;

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

        public void UpdateInstructor(
            int instructorId,
            string lastName,
            string firstName,
            string middleName,
            string phone)
        {
            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    UPDATE dbo.Instructors
                    SET
                        LastName = @LastName,
                        FirstName = @FirstName,
                        MiddleName = @MiddleName,
                        Phone = @Phone
                    WHERE InstructorID = @InstructorID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@InstructorID", SqlDbType.Int).Value = instructorId;
                    command.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = lastName;
                    command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = firstName;
                    command.Parameters.Add("@MiddleName", SqlDbType.NVarChar, 100).Value =
                        string.IsNullOrWhiteSpace(middleName) ? (object)DBNull.Value : middleName;
                    command.Parameters.Add("@Phone", SqlDbType.NVarChar, 30).Value = phone;

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteInstructors(List<int> instructorIds)
        {
            if (instructorIds == null || instructorIds.Count == 0)
                return;

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (int instructorId in instructorIds)
                        {
                            // Проверяем наличие занятий
                            string checkLessonsQuery = @"
                        SELECT COUNT(*)
                        FROM dbo.Lessons
                        WHERE InstructorID = @InstructorID";

                            using (SqlCommand command =
                                new SqlCommand(checkLessonsQuery, connection, transaction))
                            {
                                command.Parameters.Add("@InstructorID", SqlDbType.Int)
                                    .Value = instructorId;

                                int lessonsCount = Convert.ToInt32(command.ExecuteScalar());

                                if (lessonsCount > 0)
                                {
                                    throw new InvalidOperationException(
                                        "Нельзя удалить инструктора, у которого есть занятия.");
                                }
                            }

                            // Проверяем наличие учётной записи
                            string checkUsersQuery = @"
                        SELECT COUNT(*)
                        FROM dbo.Users
                        WHERE InstructorID = @InstructorID";

                            using (SqlCommand command =
                                new SqlCommand(checkUsersQuery, connection, transaction))
                            {
                                command.Parameters.Add("@InstructorID", SqlDbType.Int)
                                    .Value = instructorId;

                                int usersCount = Convert.ToInt32(command.ExecuteScalar());

                                if (usersCount > 0)
                                {
                                    throw new InvalidOperationException(
                                        "Сначала удалите учётную запись инструктора.");
                                }
                            }

                            // Удаляем историю приглашений
                            string deleteInvitationsQuery = @"
                        DELETE FROM dbo.InstructorInvitations
                        WHERE InstructorID = @InstructorID";

                            using (SqlCommand command =
                                new SqlCommand(deleteInvitationsQuery, connection, transaction))
                            {
                                command.Parameters.Add("@InstructorID", SqlDbType.Int)
                                    .Value = instructorId;

                                command.ExecuteNonQuery();
                            }

                            // Удаляем профиль инструктора
                            string deleteQuery = @"
                        DELETE FROM dbo.Instructors
                        WHERE InstructorID = @InstructorID";

                            using (SqlCommand command =
                                new SqlCommand(deleteQuery, connection, transaction))
                            {
                                command.Parameters.Add("@InstructorID", SqlDbType.Int)
                                    .Value = instructorId;

                                if (command.ExecuteNonQuery() == 0)
                                {
                                    throw new InvalidOperationException(
                                        "Инструктор не найден.");
                                }
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
    }
}
