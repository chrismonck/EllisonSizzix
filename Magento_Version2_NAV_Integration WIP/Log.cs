using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magento_Version2_NAV_Integration_Sizzix
{
 
    using System.IO;
    public static class Log
    {
        private static string GetTempPath()
        {
            string path = "E:\\Azzure\\MagentoSizzixAudit\\";
                
                //System.Environment.GetEnvironmentVariable("TEMP");
            if (!path.EndsWith("\\")) path += "\\";
            return path;
        }
        public enum LogLevel
        {
            Info,
            Warning,
            Error
        }
        public static void Write(string msg, LogLevel level = LogLevel.Info)
        {
            System.IO.StreamWriter sw = System.IO.File.AppendText(
                GetTempPath() + "Azzure_Magento2_Connector.txt");
            try
            {
                string logLine = System.String.Format(
                    "{0:G}: {1} - {2}.", System.DateTime.Now, level.ToString().PadRight(10, ' '), msg);
                sw.WriteLine(logLine);
            }
            finally
            {
                sw.Close();
            }
        }

        public static void WriteInfo(string message) { Write(message, LogLevel.Info); }
        public static void WriteWarning(string message) { Write(message, LogLevel.Warning); }
        public static void WriteError(string message) { Write(message, LogLevel.Error); }

    }

}
