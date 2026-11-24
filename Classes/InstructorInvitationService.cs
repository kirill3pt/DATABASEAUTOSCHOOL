
using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace DATABASEAUTOSCHOOL.Classes
{
    public class InstructorInvitationService
    {
        public string CreateInvitation(int instructorId)
        {
            string invitationCode = GenerateCode();
            byte[] codeHash = GetCodeHash(invitationCode);

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string checkQuery = @"
                            SELECT COUNT(*)
                            FROM dbo.Instructors
                            WHERE InstructorID = @InstructorID";

                        using (SqlCommand command = new SqlCommand(checkQuery, connection, transaction))
                        {
                            command.Parameters.Add("@InstructorID", SqlDbType.Int).Value = instructorId;

                            int count = Convert.ToInt32(command.ExecuteScalar());

                            if (count == 0)
                                throw new InvalidOperationException("Инструктор не найден.");
                        }

                        string accountQuery = @"
                            SELECT COUNT(*)
                            FROM dbo.Users
                            WHERE InstructorID = @InstructorID";

                        using (SqlCommand command = new SqlCommand(accountQuery, connection, transaction))
                        {
                            command.Parameters.Add("@InstructorID", SqlDbType.Int).Value = instructorId;

                            if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                                throw new InvalidOperationException(
                                    "У этого инструктора уже есть учётная запись.");
                        }

                        string cancelQuery = @"
                            UPDATE dbo.InstructorInvitations
                            SET
                                IsRevoked = 1,
                                RevokedAt = SYSDATETIME()
                            WHERE InstructorID = @InstructorID
                              AND IsUsed = 0
                              AND IsRevoked = 0";

                        using (SqlCommand command = new SqlCommand(cancelQuery, connection, transaction))
                        {
                            command.Parameters.Add("@InstructorID", SqlDbType.Int).Value = instructorId;
                            command.ExecuteNonQuery();
                        }

                        string insertQuery = @"
                            INSERT INTO dbo.InstructorInvitations
                                (InstructorID, InvitationCodeHash, ExpirationDate)
                            VALUES
                                (@InstructorID, @CodeHash, @ExpirationDate)";

                        using (SqlCommand command = new SqlCommand(insertQuery, connection, transaction))
                        {
                            command.Parameters.Add("@InstructorID", SqlDbType.Int).Value = instructorId;
                            command.Parameters.Add("@CodeHash", SqlDbType.VarBinary, 32).Value = codeHash;
                            command.Parameters.Add("@ExpirationDate", SqlDbType.DateTime2)
                                .Value = DateTime.Now.AddHours(24);

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

            return invitationCode;
        }

        private string GenerateCode()
        {
            byte[] bytes = new byte[16];

            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
            }

            StringBuilder result = new StringBuilder();

            foreach (byte value in bytes)
                result.Append(value.ToString("X2"));

            return result.ToString();
        }

        private byte[] GetCodeHash(string code)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
            }
        }

        public int? ValidateInvitation(string invitationCode)
        {
            if (string.IsNullOrWhiteSpace(invitationCode))
                return null;

            byte[] codeHash = GetCodeHash(invitationCode.Trim().ToUpperInvariant());

            using (SqlConnection connection = DB.GetConnection())
            {
                string query = @"
                    SELECT InstructorID
                    FROM dbo.InstructorInvitations
                    WHERE InvitationCodeHash = @CodeHash
                      AND IsUsed = 0
                      AND IsRevoked = 0
                      AND ExpirationDate > SYSDATETIME()";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@CodeHash", SqlDbType.VarBinary, 32).Value = codeHash;

                    connection.Open();

                    object result = command.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        return null;

                    return Convert.ToInt32(result);
                }
            }
        }

        public void RegisterInstructor(string invitationCode, string login, string password)
        {
            if (string.IsNullOrWhiteSpace(invitationCode))
                throw new InvalidOperationException("Введите код приглашения.");

            if (string.IsNullOrWhiteSpace(login))
                throw new InvalidOperationException("Введите логин.");

            if (string.IsNullOrEmpty(password))
                throw new InvalidOperationException("Введите пароль.");

            byte[] codeHash = GetCodeHash(invitationCode.Trim().ToUpperInvariant());
            string passwordHash = PasswordHasher.CreateHash(password);

            using (SqlConnection connection = DB.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction =
                    connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int instructorId;
                        int invitationId;

                        string invitationQuery = @"
                    SELECT InvitationID, InstructorID
                    FROM dbo.InstructorInvitations WITH (UPDLOCK, HOLDLOCK)
                    WHERE InvitationCodeHash = @CodeHash
                      AND IsUsed = 0
                      AND IsRevoked = 0
                      AND ExpirationDate > SYSDATETIME()";

                        using (SqlCommand command =
                            new SqlCommand(invitationQuery, connection, transaction))
                        {
                            command.Parameters.Add("@CodeHash", SqlDbType.VarBinary, 32)
                                .Value = codeHash;

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (!reader.Read())
                                    throw new InvalidOperationException(
                                        "Приглашение недействительно или срок его действия истёк.");

                                invitationId = Convert.ToInt32(reader["InvitationID"]);
                                instructorId = Convert.ToInt32(reader["InstructorID"]);
                            }
                        }

                        string accountQuery = @"
                    SELECT COUNT(*)
                    FROM dbo.Users
                    WHERE InstructorID = @InstructorID";

                        using (SqlCommand command =
                            new SqlCommand(accountQuery, connection, transaction))
                        {
                            command.Parameters.Add("@InstructorID", SqlDbType.Int)
                                .Value = instructorId;

                            if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                                throw new InvalidOperationException(
                                    "У этого инструктора уже есть учётная запись.");
                        }

                        string loginQuery = @"
                    SELECT COUNT(*)
                    FROM dbo.Users
                    WHERE Login = @Login";

                        using (SqlCommand command =
                            new SqlCommand(loginQuery, connection, transaction))
                        {
                            command.Parameters.Add("@Login", SqlDbType.NVarChar, 100)
                                .Value = login.Trim();

                            if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                                throw new InvalidOperationException(
                                    "Такой логин уже занят.");
                        }

                        string insertQuery = @"
                    INSERT INTO dbo.Users
                        (Login, PasswordHash, Role, InstructorID)
                    VALUES
                        (@Login, @PasswordHash, N'Инструктор', @InstructorID)";

                        using (SqlCommand command =
                            new SqlCommand(insertQuery, connection, transaction))
                        {
                            command.Parameters.Add("@Login", SqlDbType.NVarChar, 100)
                                .Value = login.Trim();

                            command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 510)
                                .Value = passwordHash;

                            command.Parameters.Add("@InstructorID", SqlDbType.Int)
                                .Value = instructorId;

                            command.ExecuteNonQuery();
                        }

                        string updateQuery = @"
                    UPDATE dbo.InstructorInvitations
                    SET
                        IsUsed = 1,
                        UsedAt = SYSDATETIME()
                    WHERE InvitationID = @InvitationID
                      AND IsUsed = 0
                      AND IsRevoked = 0";

                        using (SqlCommand command =
                            new SqlCommand(updateQuery, connection, transaction))
                        {
                            command.Parameters.Add("@InvitationID", SqlDbType.Int)
                                .Value = invitationId;

                            if (command.ExecuteNonQuery() != 1)
                                throw new InvalidOperationException(
                                    "Не удалось подтвердить использование приглашения.");
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
