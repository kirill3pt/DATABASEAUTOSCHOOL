using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATABASEAUTOSCHOOL.Classes
{
    public interface IAUTH
    {
        bool Authenticate(string role, string login, string password);

        bool Authenticate(
            string role,
            string login,
            string password,
            out int instructorId);
    }
}