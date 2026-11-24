
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DATABASEAUTOSCHOOL.Classes
{
    public class GroupService
    {
        // Получение списка групп с поиском по названию
        // Получение списка групп с поиском по названию и подсчётом учеников
        public DataTable GetGroups(string searchText = "")
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
            SELECT
                g.GroupID AS [ID],
                g.GroupName AS [Название группы],
                g.StartDate AS [Дата начала],
                g.EndDate AS [Дата окончания],
                COUNT(s.StudentID) AS [Количество учеников]
            FROM dbo.Groups g
            LEFT JOIN dbo.Students s
                ON g.GroupID = s.GroupID
            WHERE g.GroupName LIKE @SearchText
            GROUP BY
                g.GroupID,
                g.GroupName,
                g.StartDate,
                g.EndDate
            ORDER BY g.GroupID DESC";

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

        // Добавление группы
        public void AddGroup(
            string groupName,
            DateTime? startDate,
            DateTime? endDate)
        {
            ValidateGroup(groupName, startDate, endDate);

            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    INSERT INTO Groups (GroupName, StartDate, EndDate)
                    VALUES (@GroupName, @StartDate, @EndDate)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddGroupParameters(command, groupName, startDate, endDate);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // Изменение группы
        public void UpdateGroup(
            int groupId,
            string groupName,
            DateTime? startDate,
            DateTime? endDate)
        {
            ValidateGroup(groupName, startDate, endDate);

            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    UPDATE Groups
                    SET
                        GroupName = @GroupName,
                        StartDate = @StartDate,
                        EndDate = @EndDate
                    WHERE GroupID = @GroupID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddGroupParameters(command, groupName, startDate, endDate);

                    command.Parameters.Add("@GroupID", SqlDbType.Int).Value = groupId;

                    connection.Open();

                    if (command.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("Группа не найдена.");
                }
            }
        }

        // Удаление одной группы
        public void DeleteGroup(int groupId)
        {
            DeleteGroups(new List<int> { groupId });
        }

        // Удаление нескольких групп
        public void DeleteGroups(List<int> groupIds)
        {
            if (groupIds == null || groupIds.Count == 0)
                throw new ArgumentException("Не выбраны группы для удаления.");

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (int groupId in groupIds)
                        {
                            string checkQuery = @"
                                SELECT COUNT(*)
                                FROM Students
                                WHERE GroupID = @GroupID";

                            using (SqlCommand checkCommand =
                                new SqlCommand(checkQuery, connection, transaction))
                            {
                                checkCommand.Parameters.Add("@GroupID", SqlDbType.Int)
                                    .Value = groupId;

                                int studentsCount = (int)checkCommand.ExecuteScalar();

                                if (studentsCount > 0)
                                {
                                    throw new InvalidOperationException(
                                        "Нельзя удалить группу, к которой привязаны ученики.");
                                }
                            }

                            string deleteQuery = @"
                                DELETE FROM Groups
                                WHERE GroupID = @GroupID";

                            using (SqlCommand deleteCommand =
                                new SqlCommand(deleteQuery, connection, transaction))
                            {
                                deleteCommand.Parameters.Add("@GroupID", SqlDbType.Int)
                                    .Value = groupId;

                                if (deleteCommand.ExecuteNonQuery() == 0)
                                {
                                    throw new InvalidOperationException(
                                        "Одна из выбранных групп не найдена.");
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

        // Проверка данных группы
        private void ValidateGroup(
            string groupName,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                throw new ArgumentException("Введите название группы.");

            if (groupName.Trim().Length > 100)
                throw new ArgumentException(
                    "Название группы не должно превышать 100 символов.");

            if (startDate.HasValue && endDate.HasValue &&
                startDate.Value.Date > endDate.Value.Date)
            {
                throw new ArgumentException(
                    "Дата начала не может быть позже даты окончания.");
            }
        }

        // Подготовка параметров для запросов добавления и изменения
        private void AddGroupParameters(
            SqlCommand command,
            string groupName,
            DateTime? startDate,
            DateTime? endDate)
        {
            command.Parameters.Add("@GroupName", SqlDbType.NVarChar, 100)
                .Value = groupName.Trim();

            command.Parameters.Add("@StartDate", SqlDbType.Date)
                .Value = startDate.HasValue
                    ? (object)startDate.Value.Date
                    : DBNull.Value;

            command.Parameters.Add("@EndDate", SqlDbType.Date)
                .Value = endDate.HasValue
                    ? (object)endDate.Value.Date
                    : DBNull.Value;
        }
    }
}
