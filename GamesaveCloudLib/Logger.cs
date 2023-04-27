using System;
using System.IO;

namespace GamesaveCloudLib
{
    public class Logger
    {
        private readonly StreamWriter logFile;
        static readonly string logDir = "logs";
        static readonly string logFileName = "GamesaveCloud.log";

        public Logger()
        {
            //var pathLog = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "logs");

            string assembly = Environment.ProcessPath;

            if (assembly != null)
            {
                var assemblyPath = Path.GetDirectoryName(assembly);
                if (assemblyPath != null)
                {
                    string pathLog = Path.Combine(assemblyPath, logDir);
                    Directory.CreateDirectory(pathLog);
                    logFile = new StreamWriter(Path.Combine(pathLog, logFileName), true);
                }
                else
                {
                    throw (new Exception("Error: could not determine current path."));

                }
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
