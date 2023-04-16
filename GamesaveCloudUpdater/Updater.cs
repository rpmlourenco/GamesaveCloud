using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Xml.Linq;

namespace GamesaveCloudUpdater
{
    public class Updater
    {
        static readonly string remoteVersionURL = "https://raw.githubusercontent.com/rpmlourenco/GamesaveCloud/master/GamesaveCloudCLI/GamesaveCloudCLI.csproj";
        static readonly string baseRemoteUrl = "https://github.com/rpmlourenco/GamesaveCloud/raw/master/published/GamesaveCloudCLI";
        static readonly string executable = "GamesaveCloudCLI.exe";
        static readonly string logDir = "logs";
        static readonly string logFileName = "GamesaveCloud.Log";

        string? assemblyWithoutExtendion;
        string? assemblyPath;
        string? fileToUpdate;
        readonly string[] args;
        string? localVersion;
        string? remoteVersion;
        private StreamWriter? logFile;
        private bool logLastNewline;

        public Updater(string[] args)
        {
            this.args = args;
            this.logLastNewline = true;
        }

        ~Updater()
        {
            logFile?.Close();
        }

        public void LogWrite(string text)
        {
            if (logLastNewline)
            {
                Console.Write(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + text);
                logFile?.Write(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + text);
            }
            else
            {
                Console.Write(text);
                logFile?.Write(text);
            }
            logLastNewline = false;
        }

        public void LogWriteLine(string text)
        {
            if (logLastNewline)
            {
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + text);
                logFile?.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + text);
            }
            else
            {
                Console.WriteLine(text);
                logFile?.WriteLine(text);
            }

            logLastNewline = true;
        }

        public static string? GetLocalVersion(string file)
        {
            if (!File.Exists(file))
            {
                return null;
            }
            else
            {
                return FileVersionInfo.GetVersionInfo(file).FileVersion;
            }
        }

        public static string? GetRemoteVersion()
        {
            XElement xdocument = XElement.Load(remoteVersionURL);
            XElement? xversion;
            XElement? xproperty = xdocument.Descendants("PropertyGroup").FirstOrDefault();
            if (xproperty != null)
            {
                xversion = xproperty.Descendants("Version").FirstOrDefault();
                if (xversion != null)
                {
                    return xversion.Value;
                } 
                else { return null; }
                
            }
            else
            {
                return null;
            }
        }


        public int CheckVersion()
        {

            //string assembly = Assembly.GetExecutingAssembly().Location;
            //string? assembly = Environment.ProcessPath;
            //this.assemblyWithoutExtendion = Path.GetFileNameWithoutExtension(assembly);
            //this.assemblyPath = Path.GetDirectoryName(assembly);

            string? assembly = Environment.ProcessPath;

            if (assembly != null)
            {
                this.assemblyPath = Path.GetDirectoryName(assembly);
                if (this.assemblyPath != null)
                {
                    this.fileToUpdate = Path.Combine(this.assemblyPath, executable);
                    this.assemblyWithoutExtendion = Path.GetFileNameWithoutExtension(assembly);
                }
                else
                {
                    LogWriteLine("Error: could not determine current path.");
                    return 1;
                }
            } 
            else
            {
                LogWriteLine("Error: could not determine current path.");
                return 1;

            }

            string pathLog = Path.Combine(assemblyPath, logDir);
            Directory.CreateDirectory(pathLog);
            logFile = new StreamWriter(Path.Combine(pathLog, logFileName), true);


            this.remoteVersion = Updater.GetRemoteVersion();
            this.localVersion = Updater.GetLocalVersion(this.fileToUpdate);

            if (this.remoteVersion != null)
            {
                if (this.localVersion != null)
                { 
                    if (this.remoteVersion.Equals(this.localVersion))
                    {
                        LogWriteLine($"Version {this.localVersion} is up to date.");
                        LaunchExecutable();
                        return 0;
                    }
                    else
                    {
                        LogWriteLine($"Current remoteVersion {this.localVersion} is outdated. New remoteVersion {this.remoteVersion} found.");
                        PerformUpdate();
                        return 0;
                    }

                }
                else
                {
                    LogWriteLine($"Local installation not found. Installing latest verion {this.remoteVersion}.");
                    PerformUpdate();
                    return 0;
                }

            }
            else
            {
                if (this.localVersion != null)
                {
                    LogWriteLine("No remote version found.");
                    LaunchExecutable();
                    return 0;
                }
                else
                {
                    LogWriteLine("No local or remote versions found. Exiting.");
                    return 1;
                }
            }
        }


        private bool PerformUpdate()
        {
            string remoteUrl = baseRemoteUrl + "-" + this.remoteVersion + ".zip";

            LogWriteLine("Beginning update...");
            string downloadDestination = Path.GetTempFileName();

            LogWrite($"Downloading to {downloadDestination} - ");

            HttpClient client = new();
            var task = Task.Run(() => client.GetAsync(remoteUrl));
            task.Wait();
            if (task.Result != null)
            {
                var response = task.Result;
                using var fs = new FileStream(downloadDestination, FileMode.Create);
                response.Content.CopyToAsync(fs).Wait();
                LogWriteLine("done.");
            }
            else
            {
                LogWriteLine("Could not download update.");
            }

            LogWrite("Extracting archive - ");
            string extractTarget;
            if (this.assemblyPath != null)
            {
                extractTarget = Path.Combine(this.assemblyPath, "downloadedFiles"); ;
            }
            else
            {
                LogWriteLine("Error: could not determine current path.");
                return false;
            }

            try
            {
                Directory.Delete(extractTarget, true);
            }
            catch { }

            ZipFile.ExtractToDirectory(downloadDestination, extractTarget);

            if (this.assemblyWithoutExtendion != null) { 
                foreach (string newPath in Directory.GetFiles(assemblyPath, "*.*", SearchOption.TopDirectoryOnly))
                {
                    if (!Path.GetFileName(newPath).Contains(this.assemblyWithoutExtendion)) {
                        File.Delete(newPath);
                    }
                }
            }

            // Copy the extracted files and replace everything in the current directory to finish the update
            // C# doesn't easily let us extract & replace at the same time
            // From http://stackoverflow.com/a/3822913/1460422
            foreach (string newPath in Directory.GetFiles(extractTarget, "*.*", SearchOption.TopDirectoryOnly))
                File.Copy(newPath, newPath.Replace(extractTarget, assemblyPath), true);
            LogWriteLine("done.");

            // Clean up the temporary files
            LogWrite("Cleaning up - ");
            Directory.Delete(extractTarget, true);
            LogWriteLine("done.");

            LaunchExecutable();

            return true;
        }

        private void LaunchExecutable()
        {
            
            logFile?.Flush();
            logFile?.Close();
            logFile = null;

            /*
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = this.fileToUpdate;

            p.StartInfo.Arguments = string.Join(" ", args);

            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Verb = "runas";
                
            p.Start();
            p.WaitForExit();            
            */

        }

    }
}
