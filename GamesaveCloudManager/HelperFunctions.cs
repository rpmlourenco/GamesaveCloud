namespace GamesaveCloudManager
{
    public class HelperFunctions
    {
        public static string ReplaceEnvironmentVariables(string sPath)
        {
            var pos1 = sPath.IndexOf("%");
            if (pos1 == -1)
            {
                return sPath;
            }
            var pos2 = sPath.IndexOf("%", pos1 + 1);
            if (pos2 == -1)
            {
                return sPath;
            }

            var variable = sPath.Substring(pos1, pos2 - pos1 + 1);
            return sPath.Replace(variable, Environment.GetEnvironmentVariable(variable.Replace("%", "")));
        }

        public static string UnreplaceEnvironmentVariables(string sPath)
        {
            string[] variables = { "PROGRAMDATA", "COMMONPROGRAMFILES", "COMMONPROGRAMFILES(x86)", "PROGRAMFILES", "PROGRAMFILES(X86)", "LOCALAPPDATA", "APPDATA", "USERPROFILE", "PUBLIC" };

            foreach (string variable in variables)
            {
                var variableValue = Environment.GetEnvironmentVariable(variable);
                if (!String.IsNullOrEmpty(variableValue))
                {
                    var pos = sPath.IndexOf(variableValue);
                    if (pos != -1)
                    {
                        return sPath.Replace(variableValue, "%" + variable + "%");
                    }
                }
            }

            return sPath;
        }

    }
}
