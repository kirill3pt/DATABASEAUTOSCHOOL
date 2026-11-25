
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DATABASEAUTOSCHOOL.Classes
{
    public class LessonService
    {
        private const int LessonDurationMinutes = 90;

        public DataTable GetLessons(string searchText = "")
        {
            DataTable table = new DataTable();

            string query = @"
                SELECT
                    l.LessonID AS [ID],
                    l.StudentID AS [StudentID],
                    l.InstructorID AS [InstructorID],
                    l.CarID AS [CarID],
                    l.LessonDate AS [Дата],
                    CONVERT(varchar(5), l.LessonTime, 108) AS [Время],

                    CONCAT(
                        s.LastName, N' ',
                        LEFT(s.FirstName, 1), N'.',
                        CASE
                            WHEN NULLIF(s.MiddleName, N'') IS NOT NULL
                            THEN CONCAT(LEFT(s.MiddleName, 1), N'.')
                            ELSE N''
                        END
                    ) AS [Ученик],

                    CONCAT(
                        i.LastName, N' ',
                        LEFT(i.FirstName, 1), N'.',
                        CASE
                            WHEN NULLIF(i.MiddleName, N'') IS NOT NULL
                            THEN CONCAT(LEFT(i.MiddleName, 1), N'.')
                            ELSE N''
                        END
                    ) AS [Инструктор],

                    CONCAT(c.Brand, N' ', c.Model, N' (',
                        c.RegistrationNumber, N')') AS [Автомобиль],

                    l.Status AS [Статус]

                FROM dbo.Lessons l

                INNER JOIN dbo.Students s
                    ON l.StudentID = s.StudentID

                INNER JOIN dbo.Instructors i
                    ON l.InstructorID = i.InstructorID

                INNER JOIN dbo.Cars c
                    ON l.CarID = c.CarID

                WHERE
                    @Search = N''
                    OR CONCAT(s.LastName, N' ', s.FirstName, N' ',
                        ISNULL(s.MiddleName, N'')) LIKE @Pattern
                    OR CONCAT(i.LastName, N' ', i.FirstName, N' ',
                        ISNULL(i.MiddleName, N'')) LIKE @Pattern
                    OR CONCAT(c.Brand, N' ', c.Model) LIKE @Pattern
                    OR c.RegistrationNumber LIKE @Pattern
                    OR l.Status LIKE @Pattern

                ORDER BY l.LessonDate DESC, l.LessonTime DESC";

            using (SqlConnection connection = DB.GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value =
                    searchText.Trim();

                command.Parameters.Add("@Pattern", SqlDbType.NVarChar, 202).Value =
                    "%" + searchText.Trim() + "%";

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }
        public DataTable GetInstructorLessons(int instructorId)
        {
            DataTable table = new DataTable();

            string query = @"
        SELECT
            l.LessonID AS [ID],
            l.LessonDate AS [Дата],
            CONVERT(varchar(5), l.LessonTime, 108) AS [Время],

            CONCAT(
                s.LastName, N' ',
                LEFT(s.FirstName, 1), N'.',
                CASE
                    WHEN NULLIF(s.MiddleName, N'') IS NOT NULL
                    THEN CONCAT(LEFT(s.MiddleName, 1), N'.')
                    ELSE N''
                END
            ) AS [Ученик],

            CONCAT(
                c.Brand, N' ', c.Model, N' (',
                c.RegistrationNumber, N')'
            ) AS [Автомобиль],

            l.Status AS [Статус]

        FROM dbo.Lessons l

        INNER JOIN dbo.Students s
            ON l.StudentID = s.StudentID

        INNER JOIN dbo.Cars c
            ON l.CarID = c.CarID

        WHERE l.InstructorID = @InstructorID

        ORDER BY l.LessonDate ASC, l.LessonTime ASC";

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

        public DataTable GetStudents()
        {
            DataTable table = new DataTable();

            string query = @"
                SELECT
                    StudentID AS ID,
                    LTRIM(RTRIM(CONCAT(
                        LastName, N' ', FirstName, N' ',
                        ISNULL(MiddleName, N'')))) AS FIO
                FROM dbo.Students
                ORDER BY LastName, FirstName";

            using (SqlConnection connection = DB.GetConnection())
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
            {
                adapter.Fill(table);
            }

            return table;
        }

        public DataTable GetInstructors()
        {
            DataTable table = new DataTable();

            string query = @"
                SELECT
                    InstructorID AS ID,
                    LTRIM(RTRIM(CONCAT(
                        LastName, N' ', FirstName, N' ',
                        ISNULL(MiddleName, N'')))) AS FIO
                FROM dbo.Instructors
                ORDER BY LastName, FirstName";

            using (SqlConnection connection = DB.GetConnection())
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
            {
                adapter.Fill(table);
            }

            return table;
        }
        public DataTable GetInstructorLessonsByMonth(int instructorId, DateTime month)
        {
            DataTable table = new DataTable();

            DateTime monthStart = new DateTime(
                month.Year,
                month.Month,
                1);

            DateTime nextMonth = monthStart.AddMonths(1);

            string query = @"
        SELECT
            l.LessonID AS [ID],
            l.LessonDate AS [Дата],
            CONVERT(varchar(5), l.LessonTime, 108) AS [Время],

            CONCAT(
                s.LastName, N' ',
                LEFT(s.FirstName, 1), N'.',
                CASE
                    WHEN NULLIF(s.MiddleName, N'') IS NOT NULL
                    THEN CONCAT(LEFT(s.MiddleName, 1), N'.')
                    ELSE N''
                END
            ) AS [Ученик],

            CONCAT(
                c.Brand, N' ', c.Model
            ) AS [Автомобиль],

            c.RegistrationNumber AS [Госномер],

            l.Status AS [Статус]

        FROM dbo.Lessons l

        INNER JOIN dbo.Students s
            ON l.StudentID = s.StudentID

        INNER JOIN dbo.Cars c
            ON l.CarID = c.CarID

        WHERE
            l.InstructorID = @InstructorID
            AND l.LessonDate >= @MonthStart
            AND l.LessonDate < @NextMonth

        ORDER BY
            l.LessonDate ASC,
            l.LessonTime ASC";

            using (SqlConnection connection = DB.GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@InstructorID", SqlDbType.Int).Value =
                    instructorId;

                command.Parameters.Add("@MonthStart", SqlDbType.Date).Value =
                    monthStart;

                command.Parameters.Add("@NextMonth", SqlDbType.Date).Value =
                    nextMonth;

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }
        public void UpdateInstructorLessonStatus(int lessonId, int instructorId, string status)
        {
            if (status != "запланировано" &&
                status != "проведено" &&
                status != "отменено")
            {
                throw new InvalidOperationException(
                    "Указан некорректный статус занятия.");
            }

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction =
                    connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        DateTime lessonDate;
                        TimeSpan lessonTime;
                        string selectQuery = @"
                            SELECT LessonDate, LessonTime, Status
                            FROM dbo.Lessons WITH (UPDLOCK, HOLDLOCK)
                            WHERE LessonID = @LessonID
                              AND InstructorID = @InstructorID";

                        using (SqlCommand command =
                            new SqlCommand(selectQuery, connection, transaction))
                        {
                            command.Parameters.Add("@LessonID", SqlDbType.Int).Value =
                                lessonId;

                            command.Parameters.Add("@InstructorID", SqlDbType.Int).Value =
                                instructorId;

                            string currentStatus;

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    throw new InvalidOperationException(
                                        "Занятие не найдено или оно не принадлежит вам.");
                                }

                                lessonDate = Convert.ToDateTime(reader["LessonDate"]);
                                lessonTime = (TimeSpan)reader["LessonTime"];
                                currentStatus = reader["Status"].ToString();
                            }
                            if (currentStatus == "проведено")
                            {
                                throw new InvalidOperationException(
                                    "Проведённое занятие нельзя изменить.");
                            }
                        }

                        DateTime lessonStart = lessonDate.Date.Add(lessonTime);

                        if (status == "проведено" && lessonStart > DateTime.Now)
                        {
                            throw new InvalidOperationException(
                                "Нельзя отметить будущее занятие как проведённое.");
                        }

                        string updateQuery = @"
                    UPDATE dbo.Lessons
                    SET Status = @Status
                    WHERE LessonID = @LessonID
                      AND InstructorID = @InstructorID";

                        using (SqlCommand command =
                            new SqlCommand(updateQuery, connection, transaction))
                        {
                            command.Parameters.Add("@Status", SqlDbType.NVarChar, 60).Value =
                                status;

                            command.Parameters.Add("@LessonID", SqlDbType.Int).Value =
                                lessonId;

                            command.Parameters.Add("@InstructorID", SqlDbType.Int).Value =
                                instructorId;

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
        public DataTable GetCars()
        {
            DataTable table = new DataTable();

            string query = @"
                SELECT
                    CarID AS ID,
                    CONCAT(Brand, N' ', Model, N' (',
                        RegistrationNumber, N')') AS CarName
                FROM dbo.Cars
                ORDER BY Brand, Model";

            using (SqlConnection connection = DB.GetConnection())
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
            {
                adapter.Fill(table);
            }

            return table;
        }

        public void AddLesson(
            int studentId,
            int instructorId,
            int carId,
            DateTime lessonDate,
            TimeSpan lessonTime,
            string status)
        {
            ValidateLesson(lessonTime, status);

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction =
                    connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        CheckConflicts(
                            connection,
                            transaction,
                            studentId,
                            instructorId,
                            carId,
                            lessonDate,
                            lessonTime,
                            null,
                            status);

                        string query = @"
                            INSERT INTO dbo.Lessons
                                (LessonDate, LessonTime, StudentID,
                                 InstructorID, CarID, Status)
                            VALUES
                                (@LessonDate, @LessonTime, @StudentID,
                                 @InstructorID, @CarID, @Status)";

                        using (SqlCommand command =
                            new SqlCommand(query, connection, transaction))
                        {
                            AddLessonParameters(
                                command,
                                studentId,
                                instructorId,
                                carId,
                                lessonDate,
                                lessonTime,
                                status);

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

        public void UpdateLesson(
            int lessonId,
            int studentId,
            int instructorId,
            int carId,
            DateTime lessonDate,
            TimeSpan lessonTime,
            string status)
        {
            ValidateLesson(lessonTime, status);

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction =
                    connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        CheckConflicts(
                            connection,
                            transaction,
                            studentId,
                            instructorId,
                            carId,
                            lessonDate,
                            lessonTime,
                            lessonId,
                            status);

                        string query = @"
                            UPDATE dbo.Lessons
                            SET
                                LessonDate = @LessonDate,
                                LessonTime = @LessonTime,
                                StudentID = @StudentID,
                                InstructorID = @InstructorID,
                                CarID = @CarID,
                                Status = @Status
                            WHERE LessonID = @LessonID";

                        using (SqlCommand command =
                            new SqlCommand(query, connection, transaction))
                        {
                            AddLessonParameters(
                                command,
                                studentId,
                                instructorId,
                                carId,
                                lessonDate,
                                lessonTime,
                                status);

                            command.Parameters.Add("@LessonID", SqlDbType.Int).Value =
                                lessonId;

                            if (command.ExecuteNonQuery() == 0)
                                throw new InvalidOperationException(
                                    "Занятие не найдено.");
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

        public void DeleteLessons(List<int> lessonIds)
        {
            if (lessonIds == null || lessonIds.Count == 0)
                throw new InvalidOperationException(
                    "Выберите занятия для удаления.");

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        StringBuilder query = new StringBuilder(
                            "DELETE FROM dbo.Lessons WHERE LessonID IN (");

                        using (SqlCommand command =
                            new SqlCommand("", connection, transaction))
                        {
                            for (int i = 0; i < lessonIds.Count; i++)
                            {
                                string parameterName = "@ID" + i;

                                if (i > 0)
                                    query.Append(", ");

                                query.Append(parameterName);

                                command.Parameters.Add(
                                    parameterName,
                                    SqlDbType.Int).Value = lessonIds[i];
                            }

                            query.Append(")");
                            command.CommandText = query.ToString();

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

        private void CheckConflicts(
            SqlConnection connection,
            SqlTransaction transaction,
            int studentId,
            int instructorId,
            int carId,
            DateTime lessonDate,
            TimeSpan lessonTime,
            int? lessonId,
            string status)
        {
            if (status == "отменено")
                return;

            DateTime newStart = lessonDate.Date.Add(lessonTime);
            DateTime newEnd = newStart.AddMinutes(LessonDurationMinutes);

            string query = @"
                SELECT TOP 1
                    CASE
                        WHEN l.InstructorID = @InstructorID
                            THEN N'У инструктора уже есть занятие в это время.'
                        WHEN l.CarID = @CarID
                            THEN N'Автомобиль уже занят в это время.'
                        ELSE N'У ученика уже есть занятие в это время.'
                    END
                FROM dbo.Lessons l WITH (UPDLOCK, HOLDLOCK)
                CROSS APPLY
                (
                    SELECT DATEADD(
                        SECOND,
                        DATEDIFF(SECOND, CONVERT(time, '00:00:00'), l.LessonTime),
                        CONVERT(datetime2, l.LessonDate)
                    ) AS ExistingStart
                ) t
                WHERE l.Status <> N'отменено'
                  AND (@LessonID IS NULL OR l.LessonID <> @LessonID)
                  AND (
                        l.InstructorID = @InstructorID
                        OR l.CarID = @CarID
                        OR l.StudentID = @StudentID
                  )
                  AND t.ExistingStart < @NewEnd
                  AND DATEADD(MINUTE, 90, t.ExistingStart) > @NewStart";

            using (SqlCommand command =
                new SqlCommand(query, connection, transaction))
            {
                command.Parameters.Add("@StudentID", SqlDbType.Int).Value =
                    studentId;

                command.Parameters.Add("@InstructorID", SqlDbType.Int).Value =
                    instructorId;

                command.Parameters.Add("@CarID", SqlDbType.Int).Value =
                    carId;

                command.Parameters.Add("@NewStart", SqlDbType.DateTime2).Value =
                    newStart;

                command.Parameters.Add("@NewEnd", SqlDbType.DateTime2).Value =
                    newEnd;

                command.Parameters.Add("@LessonID", SqlDbType.Int).Value =
                    lessonId.HasValue ? (object)lessonId.Value : DBNull.Value;

                object conflict = command.ExecuteScalar();

                if (conflict != null && conflict != DBNull.Value)
                    throw new InvalidOperationException(conflict.ToString());
            }
        }

        private void ValidateLesson(TimeSpan lessonTime, string status)
        {
            if (lessonTime < TimeSpan.Zero ||
                lessonTime >= TimeSpan.FromDays(1))
                throw new InvalidOperationException(
                    "Указано некорректное время занятия.");

            if (status != "запланировано" &&
                status != "проведено" &&
                status != "отменено")
                throw new InvalidOperationException(
                    "Указан некорректный статус занятия.");
        }

        private void AddLessonParameters(
            SqlCommand command,
            int studentId,
            int instructorId,
            int carId,
            DateTime lessonDate,
            TimeSpan lessonTime,
            string status)
        {
            command.Parameters.Add("@LessonDate", SqlDbType.Date).Value =
                lessonDate.Date;

            command.Parameters.Add("@LessonTime", SqlDbType.Time).Value =
                lessonTime;

            command.Parameters.Add("@StudentID", SqlDbType.Int).Value =
                studentId;

            command.Parameters.Add("@InstructorID", SqlDbType.Int).Value =
                instructorId;

            command.Parameters.Add("@CarID", SqlDbType.Int).Value =
                carId;

            command.Parameters.Add("@Status", SqlDbType.NVarChar, 60).Value =
                status;
        }
        public int GetLessonsCount()
        {
            using (SqlConnection connection = DB.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM dbo.Lessons";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
    }
}
