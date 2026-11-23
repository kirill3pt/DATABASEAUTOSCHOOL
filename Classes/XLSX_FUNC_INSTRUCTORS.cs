
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Berezhetskiy_K.T.__IVT_2__2_course__MYPROGRAMM
{
    public class XLSX_FUNC_INSTRUCTORS
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

                logger.LOG("Ошибка проверки таблицы инструкторов: " + ex.Message);
            }
        }

        // Добавление инструктора
        public void SAVE(string fio, string number)
        {
            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        INSERT INTO Instructors (FIO, Phone)
                        VALUES (@FIO, @Phone)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@FIO", SqlDbType.NVarChar, 150).Value = fio;
                        command.Parameters.Add("@Phone", SqlDbType.NVarChar, 30).Value = number;

                        command.ExecuteNonQuery();
                    }
                }

                logger.LOG("Добавлен инструктор: " + fio);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при добавлении инструктора:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка добавления инструктора: " + ex.Message);
            }
        }

        // Отображение инструкторов в DataGridView
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
                    I.InstructorID AS [ID],
                    I.FIO AS [ФИО],
                    I.Phone AS [Телефон]
                FROM Instructors I
                INNER JOIN Users U ON I.InstructorID = U.InstructorID
                WHERE U.Role = N'Инструктор'
                ORDER BY I.InstructorID";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(table);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при получении списка инструкторов:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка отображения инструкторов: " + ex.Message);
            }

            return table;
        }

        // Заполнение ComboBox инструкторами
        public void MakeCOMBOBOX(ComboBox comboBox)
        {
            try
            {
                DataTable instructors = ReadINSTRUCTORS();

                comboBox.Items.Clear();

                foreach (DataRow row in instructors.Rows)
                {
                    string fio = row["ФИО"].ToString();
                    string phone = row["Телефон"].ToString();

                    comboBox.Items.Add($"{fio}, {phone}");
                }

                if (comboBox.Items.Count > 0)
                {
                    comboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка заполнения списка инструкторов:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка заполнения ComboBox инструкторов: " + ex.Message);
            }
        }

        // Чтение всех инструкторов
        public DataTable ReadINSTRUCTORS()
        {
            DataTable table = new DataTable();

            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT
                            InstructorID AS [ID],
                            FIO AS [ФИО],
                            Phone AS [Телефон]
                        FROM Instructors
                        ORDER BY InstructorID";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(table);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка чтения данных инструкторов:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка чтения инструкторов: " + ex.Message);
            }

            return table;
        }

        // Удаление выбранных инструкторов
        public void DELETE(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return;

            DialogResult result = MessageBox.Show(
                "Вы действительно хотите удалить выбранных инструкторов?",
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
                                    "DELETE FROM Instructors WHERE InstructorID IN (" +
                                    string.Join(", ", parameters) + ")";

                                int deletedCount = command.ExecuteNonQuery();

                                transaction.Commit();

                                logger.LOG("Удалено инструкторов: " + deletedCount);
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
                    "Не удалось удалить инструкторов.\n\n" +
                    "Возможно, некоторые инструкторы уже назначены на занятия.\n\n" +
                    "Подробности: " + ex.Message,
                    "Ошибка удаления",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                logger.LOG("Ошибка удаления инструкторов: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при удалении инструкторов:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка удаления инструкторов: " + ex.Message);
            }
        }
        // Чтение данных инструкторов для ComboBox
        public DataTable ReadINSTRUCTORS_FOR_COMBOBOX()
        {
            DataTable table = new DataTable();

            try
            {
                using (SqlConnection connection = DB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                SELECT
                    InstructorID,
                    FIO,
                    Phone
                FROM Instructors
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
                    "Ошибка загрузки списка инструкторов:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                logger.LOG("Ошибка загрузки инструкторов: " + ex.Message);
            }

            return table;
        }
    }
}

