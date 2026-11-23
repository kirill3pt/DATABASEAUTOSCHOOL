
using System;
using System.IO;
using System.Windows;

namespace DATABASEAUTOSCHOOL.Classes
{
    public class LOGGER : ILOGGER
    {
        public static string logFILEPATH_ { get; private set; } =
            Path.Combine("Logs", "log.txt");

        public LOGGER()
        {
            logFILEPATH_ = Path.Combine("Logs", "log.txt");
        }

        public void SETLOGFOLDER(string folderPath)
        {
            if (!string.IsNullOrWhiteSpace(folderPath))
                logFILEPATH_ = Path.Combine(folderPath, "log.txt");
        }

        public void LOG(string message)
        {
            try
            {
                string directory = Path.GetDirectoryName(logFILEPATH_);

                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                File.AppendAllText(
                    logFILEPATH_,
                    $"[{DateTime.Now}]: {message}{Environment.NewLine}"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка записи лога: {ex.Message}",
                    "Ошибка журналирования",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
