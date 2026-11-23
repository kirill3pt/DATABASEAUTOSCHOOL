using System;
using System.Security.Cryptography;

namespace DATABASEAUTOSCHOOL.Classes
{
    internal static class PasswordHasher
    {
        private const int Iterations = 100000;

        public static string CreateHash(string password)
        {
            byte[] salt = new byte[16];

            using (var generator = RandomNumberGenerator.Create())
                generator.GetBytes(salt);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                return Iterations + "." +
                    Convert.ToBase64String(salt) + "." +
                    Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyHash(string password, string storedHash)
        {
            try
            {
                string[] parts = storedHash.Split('.');

                int iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] originalHash = Convert.FromBase64String(parts[2]);

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
                {
                    byte[] newHash = pbkdf2.GetBytes(originalHash.Length);
                    int difference = 0;

                    for (int i = 0; i < newHash.Length; i++)
                        difference |= newHash[i] ^ originalHash[i];

                    return difference == 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}