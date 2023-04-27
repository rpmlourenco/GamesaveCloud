using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System.Reflection;

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

        public static IPublicClientApplication? BuildOneDriveClient()
        {
            string resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith("onedrive_secrets.json"));

            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly != null)
            {
                Stream? stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var sr = new StreamReader(stream);
                    string content = sr.ReadToEnd();
                    JObject json = JObject.Parse(content);

                    if (json != null)
                    {
                        var tokenClientID = json["client_id"];
                        var tokenTenant = json["tenant"];

                        if (tokenClientID != null && tokenTenant != null)
                        {
                            string? ClientId = tokenClientID.Value<string>();
                            string? Tenant = tokenTenant.Value<string>();

                            //BrokerOptions options = new(BrokerOptions.OperatingSystems.Windows);

                            var builder = PublicClientApplicationBuilder.Create(ClientId)
                                .WithAuthority($"https://login.microsoftonline.com/{Tenant}")
                                .WithDefaultRedirectUri()
                                .WithBroker(true);
                                // WithWindowsDesktopFeatures needed when used in Windows Desktop
                                //.WithWindowsDesktopFeatures(options);
                            //.WithParentActivityOrWindow(handle);

                            return builder.Build();
                        }
                        else
                        {
                            MessageBox.Show("Tokens not found when parsing onedrive_secrets.json");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Not possible to parse onedrive_secrets.json");
                    }
                }
                else
                {
                    MessageBox.Show("Resource not found when loading onedrive_secrets.json");
                }
            }
            else
            {
                MessageBox.Show("Assembly not found when loading onedrive_secrets.json");
            }
            return null;

        }

    }
}
