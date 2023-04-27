using System.IO;
using System.Reflection;

namespace GamesaveCloudLib
{
    public class Logger
    {
        private readonly StreamWriter logFile;

        public Logger()
        {
            //var pathLog = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "logs");
            var pathLog = Path.Combine(Path.GetDirectoryName(System.AppContext.BaseDirectory), "logs");
            Directory.CreateDirectory(pathLog);
            logFile = new StreamWriter(Path.Combine(pathLog, "GamesaveCloud.Log"), true);
        }

        public void Log(string message)
        {
            logFile?.Write(message);
        }

        public void Close()
        {
            logFile?.Close();
        }

        //~Logger() {
        //    logFile?.Close();
        //}
    }
}
