using System;
using System.IO;

namespace GamesaveCloudLib
{
    public class Logger
    {
        private readonly StreamWriter logFile;
        static readonly string logDir = "logs";
        static readonly string logFileName = "GamesaveCloud.log";
        public string workingPath;

        public Logger(string workingPath)
        {
            this.workingPath = workingPath;
            //string workingPath = Environment.ProcessPath;

            if (workingPath != null)
            {
                //var assemblyPath = Path.GetDirectoryName(workingPath);
                //if (assemblyPath != null)
                //{
                string pathLog = Path.Combine(workingPath, logDir);
                Directory.CreateDirectory(pathLog);
                logFile = new StreamWriter(Path.Combine(pathLog, logFileName), true);
                //}
                //else
                //{
                //    throw (new Exception("Error: could not determine current path."));

                //}
            }
            else
            {
                throw (new Exception("Error: could not determine current path."));
            }
        }

        public void Log(string message)
        {
            logFile?.Write(message);
        }

        public void Close()
        {
            logFile?.Close();
        }

    }
}
