using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace GamesaveCloudUpdater
{
    public class Updater
    {
        //static readonly string githubURL = "https://raw.githubusercontent.com/rpmlourenco/GamesaveCloud/master/";
        static readonly string githubURL = "https://github.com/rpmlourenco/GamesaveCloud/raw/master/";
        static readonly string[] remoteVersionURL = { "GamesaveCloudCLI/GamesaveCloudCLI.csproj", "GamesaveCloudManager/GamesaveCloudManager.csproj" };
        static readonly string[] remotePubURL = { "published/GamesaveCloudCLI", "published/GamesaveCloudManager" };
        static readonly string[] executables = { "GamesaveCloudCLI.exe", "GamesaveCloudManager.exe" };
        static readonly string logDir = "logs";
        static readonly string logFileName = "GamesaveCloud.Log";

        string? updaterAssembly;
        //string? updaterWithoutExtendion;
        string? assemblyPath;
        string? fileToUpdate;
        //readonly string[] args;
        string? localVersion;
        string? remoteVersion;
        private StreamWriter? logFile;
        private bool logLastNewline;

        public Updater()
        {
            //this.args = args;
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

        public static string? GetRemoteVersion(int index)
        {
            try
            {
                XElement xdocument;

                var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                using (var request = new HttpRequestMessage(HttpMethod.Get, githubURL + remoteVersionURL[index]))
                {
                    var response = httpClient.Send(request);
                    response.EnsureSuccessStatusCode();
                    using var stream = response.Content.ReadAsStream();
                    xdocument = XElement.Load(stream);
                }

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
            catch (Exception ex)
            {
                return null;
            }

        }

        public string? GetAssemblyPath()
        {
            updaterAssembly = Environment.ProcessPath;
            if (updaterAssembly != null)
            {
                return Path.GetDirectoryName(updaterAssembly);
            }
            else
            {
                LogWriteLine("Error: could not determine current path.");
                return null;
            }
        }


        public int CheckVersion()
        {
            this.assemblyPath = GetAssemblyPath();
            if (this.assemblyPath == null)
            {
                LogWriteLine("Error: could not determine current path.");
                return 1;
            }

            //this.updaterWithoutExtendion = Path.GetFileNameWithoutExtension(updaterAssembly);
            string pathLog = Path.Combine(assemblyPath, logDir);
            Directory.CreateDirectory(pathLog);
            logFile = new StreamWriter(Path.Combine(pathLog, logFileName), true);

            for (int i = 0; i < executables.Length; i++)
            {
                this.fileToUpdate = Path.Combine(this.assemblyPath, executables[i]);
                this.remoteVersion = Updater.GetRemoteVersion(i);
                this.localVersion = Updater.GetLocalVersion(this.fileToUpdate);

                if (this.remoteVersion != null)
                {
                    if (this.localVersion != null)
                    {
                        if (this.remoteVersion.Equals(this.localVersion))
                        {
                            LogWriteLine($"{executables[i]}: Version {this.localVersion} is up to date.");
                            //Close();
                        }
                        else
                        {
                            LogWriteLine($"{executables[i]}:Current remoteVersion {this.localVersion} is outdated. New remoteVersion {this.remoteVersion} found.");
                            PerformUpdate(i);
                        }

                    }
                    else
                    {
                        LogWriteLine($"{executables[i]}: Local installation not found. Installing latest verion {this.remoteVersion}.");
                        PerformUpdate(i);
                    }

                }
                else
                {
                    if (this.localVersion != null)
                    {
                        LogWriteLine($"{executables[i]}: No remote version found.");
                        //Close();
                    }
                    else
                    {
                        LogWriteLine($"{executables[i]}: No local or remote versions found. Exiting.");
                    }
                }
            }
            Close();
            return 0;
        }


        private bool PerformUpdate(int index)
        {
            string remoteUrl = githubURL + remotePubURL[index] + "-" + this.remoteVersion + ".zip";

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

            /*
            if (this.updaterWithoutExtendion != null)
            {
                foreach (string newPath in Directory.GetFiles(this.assemblyPath, "*.*", SearchOption.TopDirectoryOnly))
                {
                    if (!Path.GetFileName(newPath).Contains(this.updaterWithoutExtendion))
                    {
                        File.Delete(newPath);
                    }
                }
            }
            */

            // Copy the extracted files and replace everything in the current directory to finish the update
            // C# doesn't easily let us extract & replace at the same time
            // From http://stackoverflow.com/a/3822913/1460422
            foreach (string newPath in Directory.GetFiles(extractTarget, "*.*", SearchOption.TopDirectoryOnly))
                File.Copy(newPath, newPath.Replace(extractTarget, this.assemblyPath), true);
            LogWriteLine("done.");

            // Clean up the temporary files
            LogWrite("Cleaning up - ");
            Directory.Delete(extractTarget, true);
            LogWriteLine("done.");

            //Close();
            return true;
        }

        private void Close()
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
