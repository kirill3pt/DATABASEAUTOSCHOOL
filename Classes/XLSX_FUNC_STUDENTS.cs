
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Berezhetskiy_K.T.__IVT_2__2_course__MYPROGRAMM
{
    public class XLSX_FUNC_STUDENTS
    {
        ILOGGER logger = new LOGGER();

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

                logger.LOG("Ошибка проверки таблицы учеников: " + ex.Message);
            }
        }

        // Добавление ученика
        public void SAVE(string fio, string phone, string group)
        {
            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        INSERT INTO Students (FIO, Phone, GroupName)
                        VALUES (@FIO, @Phone, @GroupName)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@FIO", SqlDbType.NVarChar, 150).Value = fio;
                        command.Parameters.Add("@Phone", SqlDbType.NVarChar, 30).Value = phone;
                        command.Parameters.Add("@GroupName", SqlDbType.NVarChar, 50).Value = group;

                        command.ExecuteNonQuery();
                    }
                }

                logger.LOG("Добавлен ученик: " + fio);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при добавлении ученика:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка добавления ученика: " + ex.Message);
            }
        }

        // Получение списка учеников для DataGridView
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
                            StudentID AS [ID],
                            FIO AS [ФИО],
                            Phone AS [Телефон],
                            GroupName AS [Группа],
                            Status AS [Статус]
                        FROM Students
                        ORDER BY StudentID";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(table);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при получении списка учеников:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка отображения учеников: " + ex.Message);
            }

            return table;
        }

        // Заполнение ComboBox списком учеников
        public void MakeCOMBOBOX(ComboBox comboBox)
        {
            try
            {
                DataTable students = ReadSTUDENTS();

                comboBox.Items.Clear();

                foreach (DataRow row in students.Rows)
                {
                    comboBox.Items.Add(row["ФИО"].ToString());
                }

                if (comboBox.Items.Count > 0)
                {
                    comboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка заполнения списка учеников:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка заполнения ComboBox учеников: " + ex.Message);
            }
        }

        // Чтение всех данных об учениках
        public DataTable ReadSTUDENTS()
        {
            DataTable table = new DataTable();

            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT
                            StudentID AS [ID],
                            FIO AS [ФИО],
                            Phone AS [Телефон],
                            GroupName AS [Группа],
                            Status AS [Статус]
                        FROM Students
                        ORDER BY StudentID";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(table);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка чтения данных учеников:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка чтения учеников: " + ex.Message);
            }

            return table;
        }

        // Удаление выбранных учеников
        public void DELETE(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return;

            DialogResult result = MessageBox.Show(
                "Вы действительно хотите удалить выбранных учеников?",
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
                                    string parameterName = "@id" + i;
                                    parameters.Add(parameterName);

                                    command.Parameters.Add(
                                        parameterName,
                                        SqlDbType.Int
                                    ).Value = ids[i];
                                }

                                command.CommandText =
                                    "DELETE FROM Students WHERE StudentID IN (" +
                                    string.Join(", ", parameters) + ")";

                                int deletedCount = command.ExecuteNonQuery();

                                transaction.Commit();

                                logger.LOG(
                                    "Удалено учеников: " + deletedCount
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
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "Не удалось удалить учеников.\n\n" +
                    "Возможно, некоторые из них уже присутствуют в расписании занятий.\n\n" +
                    "Подробности: " + ex.Message,
                    "Ошибка удаления",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                logger.LOG("Ошибка удаления учеников: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при удалении учеников:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка удаления учеников: " + ex.Message);
            }
        }
        // Чтение данных учеников для ComboBox
        public DataTable ReadSTUDENTS_FOR_COMBOBOX()
        {
            DataTable table = new DataTable();

            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                SELECT
                    StudentID,
                    FIO
                FROM Students
                ORDER BY FIO";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(table);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки списка учеников:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка загрузки списка учеников: " + ex.Message);
            }

            return table;
        }
    }
}

