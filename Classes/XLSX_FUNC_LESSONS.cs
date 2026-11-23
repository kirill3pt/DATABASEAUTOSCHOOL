
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace Berezhetskiy_K.T.__IVT_2__2_course__MYPROGRAMM
{
    public class XLSX_FUNC_LESSONS
    {
        ILOGGER logger = new LOGGER();

        // Проверка подключения к базе данных
        public void CREATE()
        {
            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка подключения к базе данных:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка проверки таблицы занятий: " + ex.Message);
            }
        }

        // Поиск ID по отображаемому значению.
        // Если найдено несколько совпадений или ни одного,
        // выбрасывается исключение.
        private int FindUniqueID(
            SqlConnection connection,
            SqlTransaction transaction,
            string query,
            string parameterName,
            string value)
        {
            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.Add(
                    parameterName,
                    SqlDbType.NVarChar,
                    200
                ).Value = value.Trim();

                List<int> foundIDs = new List<int>();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        foundIDs.Add(reader.GetInt32(0));
                    }
                }

                if (foundIDs.Count == 0)
                {
                    throw new Exception(
                        "Не удалось найти запись: " + value
                    );
                }

                if (foundIDs.Count > 1)
                {
                    throw new Exception(
                        "Найдено несколько записей для значения: " + value +
                        ". Необходимо уточнить выбранную запись."
                    );
                }

                return foundIDs[0];
            }
        }

        // Добавление занятия
        public void SAVE(
    string date,
    string time,
    int studentID,
    int instructorID,
    int carID,
    string status = "запланировано")
        {
            try
            {
                DateTime lessonDate;
                TimeSpan lessonTime;

                // Проверка даты
                bool dateValid = DateTime.TryParse(
                    date,
                    CultureInfo.CurrentCulture,
                    DateTimeStyles.None,
                    out lessonDate
                );

                if (!dateValid)
                {
                    dateValid = DateTime.TryParse(
                        date,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out lessonDate
                    );
                }

                if (!dateValid)
                {
                    MessageBox.Show(
                        "Некорректная дата занятия.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // Проверка времени
                if (!TimeSpan.TryParse(
                    time,
                    CultureInfo.CurrentCulture,
                    out lessonTime))
                {
                    MessageBox.Show(
                        "Некорректное время занятия.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                if (lessonTime < TimeSpan.Zero ||
                    lessonTime >= TimeSpan.FromDays(1))
                {
                    MessageBox.Show(
                        "Время занятия должно находиться в пределах суток.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // Проверка статуса
                if (status != "запланировано" &&
                    status != "отменено" &&
                    status != "проведено")
                {
                    MessageBox.Show(
                        "Указан некорректный статус занятия.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                INSERT INTO Lessons
                (
                    LessonDate,
                    LessonTime,
                    StudentID,
                    InstructorID,
                    CarID,
                    Status
                )
                VALUES
                (
                    @Date,
                    @Time,
                    @StudentID,
                    @InstructorID,
                    @CarID,
                    @Status
                )";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Date", SqlDbType.Date)
                            .Value = lessonDate.Date;

                        command.Parameters.Add("@Time", SqlDbType.Time)
                            .Value = lessonTime;

                        command.Parameters.Add("@StudentID", SqlDbType.Int)
                            .Value = studentID;

                        command.Parameters.Add("@InstructorID", SqlDbType.Int)
                            .Value = instructorID;

                        command.Parameters.Add("@CarID", SqlDbType.Int)
                            .Value = carID;

                        command.Parameters.Add("@Status", SqlDbType.NVarChar, 30)
                            .Value = status;

                        command.ExecuteNonQuery();
                    }
                }

                logger.LOG(
                    "Добавлено занятие: " +
                    lessonDate.ToShortDateString() + " " +
                    lessonTime
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при добавлении занятия:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка добавления занятия: " + ex.Message);
            }
        }

        // Отображение расписания
        public DataTable SHOW()
        {
            DataTable table = new DataTable();

            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT
                            L.LessonID AS [ID],

                            CONVERT(VARCHAR(10), L.LessonDate, 104)
                                AS [Дата],

                            CONVERT(VARCHAR(5), L.LessonTime, 108)
                                AS [Время],

                            S.FIO AS [Ученик (Ф.И.О)],

                            I.FIO AS [Инструктор (Ф.И.О)],

                            C.Brand + N' ' + C.Model + N', ' +
                                C.RegistrationNumber
                                AS [Автомобиль (Марка, модель, гос. номер],

                            L.Status AS [Статус]

                        FROM Lessons L

                        INNER JOIN Students S
                            ON L.StudentID = S.StudentID

                        INNER JOIN Instructors I
                            ON L.InstructorID = I.InstructorID

                        INNER JOIN Cars C
                            ON L.CarID = C.CarID

                        ORDER BY L.LessonDate, L.LessonTime";

                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(table);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при получении расписания:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка отображения расписания: " + ex.Message);
            }

            return table;
        }

        // Изменение статуса занятия
        public void statusLESSON(int id, string status)
        {
            try
            {
                if (status != "запланировано" &&
                    status != "отменено" &&
                    status != "проведено")
                {
                    MessageBox.Show(
                        "Недопустимый статус занятия.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        UPDATE Lessons
                        SET Status = @Status
                        WHERE LessonID = @ID";

                    using (SqlCommand command =
                        new SqlCommand(query, connection))
                    {
                        command.Parameters.Add(
                            "@Status",
                            SqlDbType.NVarChar,
                            30
                        ).Value = status;

                        command.Parameters.Add(
                            "@ID",
                            SqlDbType.Int
                        ).Value = id;

                        int affectedRows = command.ExecuteNonQuery();

                        if (affectedRows > 0)
                        {
                            logger.LOG(
                                "Изменён статус занятия ID " +
                                id + ": " + status
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка изменения статуса занятия:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка изменения статуса занятия: " + ex.Message);
            }
        }

        // Удаление выбранных занятий
        public void DELETE(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return;

            DialogResult result = MessageBox.Show(
                "Вы действительно хотите удалить выбранные занятия?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    using (SqlTransaction transaction =
                        connection.BeginTransaction())
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
                                    string parameterName = "@id" + i;

                                    parameters.Add(parameterName);

                                    command.Parameters.Add(
                                        parameterName,
                                        SqlDbType.Int
                                    ).Value = ids[i];
                                }

                                command.CommandText =
                                    "DELETE FROM Lessons WHERE LessonID IN (" +
                                    string.Join(", ", parameters) + ")";

                                int deletedCount = command.ExecuteNonQuery();

                                transaction.Commit();

                                logger.LOG(
                                    "Удалено занятий: " + deletedCount
                                );
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
                MessageBox.Show(
                    "Ошибка при удалении занятий:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка удаления занятий: " + ex.Message);
            }
        }
    }
}
