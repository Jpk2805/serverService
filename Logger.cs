using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicesA07
{
    internal class Logger
    {
            private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server.log");

            static Logger()
            {
                if (!File.Exists(LogFilePath))
                {
                    File.Create(LogFilePath).Dispose();
                }
            }

            public static void Log(string message)
            {
                string timestampedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

                Console.WriteLine(timestampedMessage);

                try
                {
                    File.AppendAllText(LogFilePath, timestampedMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                }
            }

}
}
