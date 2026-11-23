using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATABASEAUTOSCHOOL.Classes
{
    internal interface ILOGGER
    {
        void SETLOGFOLDER(string folderPath);
        void LOG(string message);
    }
}
