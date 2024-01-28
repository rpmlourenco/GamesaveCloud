using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace GamesaveCloudLib
{
    public partial class IniFile   // revision 11
    {
        readonly string sPath;
        readonly string sFilename;
        readonly string EXE = Assembly.GetExecutingAssembly().GetName().Name;
        public string workingPath;

        public string SFilename => sFilename;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public IniFile(string workingPath, string IniPath = null)
        {
            this.workingPath = workingPath; 
            var pathCurrent = Path.GetDirectoryName(this.workingPath);
            var pathConfigFolder = Path.Combine(pathCurrent, "config");
            if (!Directory.Exists(pathConfigFolder))
            {
                Directory.CreateDirectory(pathConfigFolder);
            }
            var pathIniFile = Path.Combine(pathConfigFolder, EXE + ".ini");

            sPath = new FileInfo(IniPath ?? pathIniFile).FullName;
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
    }
}
