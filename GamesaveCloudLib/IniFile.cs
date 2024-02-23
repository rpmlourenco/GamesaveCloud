using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace GamesaveCloudLib
{
    public partial class IniFile   // revision 11
    {
        readonly string sPath;
        readonly string sFilename;
        readonly string EXE = "GamesaveCloud";
        public string workingPath;

        public string SFilename => sFilename;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);

        public IniFile(string workingPath)
        {
            this.workingPath = workingPath;
            //var pathCurrent = Path.GetDirectoryName(this.workingPath);
            var pathConfigFolder = Path.Combine(this.workingPath, "config");
            if (!Directory.Exists(pathConfigFolder))
            {
                Directory.CreateDirectory(pathConfigFolder);
            }
            var pathIniFile = Path.Combine(pathConfigFolder, EXE + ".ini");

            sPath = new FileInfo(pathIniFile).FullName;
            sFilename = Path.GetFileName(sPath);
        }

        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            _ = GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, sPath);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? EXE, Key, Value, sPath);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }

        public List<KeyValuePair<string, string>> GetSectionKeys(string section)
        {
            var result = new List<KeyValuePair<string, string>>();

            byte[] buffer = new byte[4096];

            GetPrivateProfileSection(section, buffer, 4096, sPath);
            String[] tmp = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0');

            foreach (String entry in tmp)
            {
                if (entry.Contains("="))
                {
                    var key = entry.Substring(0, entry.IndexOf("="));
                    var value = entry.Substring(entry.IndexOf("=") + 1);
                    result.Add(new KeyValuePair<string, string>(key, value));
                }
            }

            return result;
        }
    }
}
