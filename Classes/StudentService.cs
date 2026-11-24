
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DATABASEAUTOSCHOOL.Classes
{
    public class StudentService
    {
        private readonly ILOGGER logger = new LOGGER();

        // Получение списка учеников
        public DataTable GetStudents(string searchText = "")
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT
                        s.StudentID AS [ID],
                        s.LastName AS [Фамилия],
                        s.FirstName AS [Имя],
                        s.MiddleName AS [Отчество],
                        s.Phone AS [Телефон],
                        s.Status AS [Статус],
                        g.GroupName AS [Группа],
                        s.GroupID
                    FROM Students s
                    INNER JOIN Groups g ON s.GroupID = g.GroupID
                    WHERE
                        @SearchText = ''
                        OR s.LastName LIKE @SearchPattern
                        OR s.FirstName LIKE @SearchPattern
                        OR s.MiddleName LIKE @SearchPattern
                        OR s.Phone LIKE @SearchPattern
                    ORDER BY s.LastName, s.FirstName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@SearchText", SqlDbType.NVarChar, 200)
                        .Value = searchText.Trim();

                    command.Parameters.Add("@SearchPattern", SqlDbType.NVarChar, 202)
                        .Value = "%" + searchText.Trim() + "%";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            return table;
        }

        // Получение учебных групп для списка выбора
        public DataTable GetGroupsForComboBox()
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT
                        GroupID,
                        GroupName
                    FROM Groups
                    ORDER BY GroupName";

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }

        // Добавление ученика
        public void AddStudent(
    string lastName,
    string firstName,
    string middleName,
    string phone,
    int groupId)
        {
            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                string query = @"
            INSERT INTO Students
                (LastName, FirstName, MiddleName, Phone, Status, GroupID)
            VALUES
                (@LastName, @FirstName, @MiddleName, @Phone, @Status, @GroupID)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LastName", lastName);
                    command.Parameters.AddWithValue("@FirstName", firstName);

                    command.Parameters.AddWithValue(
                        "@MiddleName",
                        string.IsNullOrWhiteSpace(middleName)
                            ? (object)DBNull.Value
                            : middleName);

                    command.Parameters.AddWithValue("@Phone", phone);
                    command.Parameters.AddWithValue("@Status", "Обучается");
                    command.Parameters.AddWithValue("@GroupID", groupId);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Изменение данных ученика
        public void UpdateStudent(
            int studentId,
            string lastName,
            string firstName,
            string middleName,
            string phone,
            string status,
            int groupId)
        {
            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        UPDATE Students
                        SET
                            LastName = @LastName,
                            FirstName = @FirstName,
                            MiddleName = @MiddleName,
                            Phone = @Phone,
                            Status = @Status,
                            GroupID = @GroupID
                        WHERE StudentID = @StudentID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@StudentID", SqlDbType.Int).Value = studentId;
                        command.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = lastName.Trim();
                        command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = firstName.Trim();

                        command.Parameters.Add("@MiddleName", SqlDbType.NVarChar, 100).Value =
                            string.IsNullOrWhiteSpace(middleName)
                                ? (object)DBNull.Value
                                : middleName.Trim();

                        command.Parameters.Add("@Phone", SqlDbType.NVarChar, 60).Value = phone.Trim();
                        command.Parameters.Add("@Status", SqlDbType.NVarChar, 60).Value = status.Trim();
                        command.Parameters.Add("@GroupID", SqlDbType.Int).Value = groupId;

                        command.ExecuteNonQuery();
                    }
                }

                logger.LOG("Изменены данные ученика. ID: " + studentId);
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка изменения данных ученика: " + ex.Message);
                throw;
            }
        }

        // Удаление учеников
        public void DeleteStudents(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return;

            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            List<string> parameters = new List<string>();

                            using (SqlCommand command = new SqlCommand())
                            {
                                command.Connection = connection;
                                command.Transaction = transaction;

                                for (int i = 0; i < ids.Count; i++)
                                {
                                    string parameter = "@id" + i;
                                    parameters.Add(parameter);

                                    command.Parameters.Add(parameter, SqlDbType.Int).Value = ids[i];
                                }

                                string idList = string.Join(", ", parameters);

                                command.CommandText = @"
                                    IF EXISTS (
                                        SELECT 1
                                        FROM Lessons
                                        WHERE StudentID IN (" + idList + @")
                                    )
                                    OR EXISTS (
                                        SELECT 1
                                        FROM Payments
                                        WHERE StudentID IN (" + idList + @")
                                    )
                                    BEGIN
                                        THROW 50010,
                                            N'Нельзя удалить ученика, у которого есть занятия или платежи.',
                                            1;
                                    END;

                                    DELETE FROM Students
                                    WHERE StudentID IN (" + idList + @");";

                                int count = command.ExecuteNonQuery();

                                transaction.Commit();

                                logger.LOG("Удалено учеников: " + count);
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LOG("Ошибка удаления учеников: " + ex.Message);
                throw;
            }
        }
        // Получение общего количества учеников
        public int GetStudentsCount()
        {
            using (SqlConnection connection = DB.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM dbo.Students";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
    }
}
