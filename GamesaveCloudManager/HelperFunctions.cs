using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace GamesaveCloudManager
{
    public class HelperFunctions
    {
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

                            BrokerOptions options = new(BrokerOptions.OperatingSystems.Windows);

                            var builder = PublicClientApplicationBuilder.Create(ClientId)
                                .WithAuthority($"https://login.microsoftonline.com/{Tenant}")
                                .WithDefaultRedirectUri();
                            //  .WithBroker(true);
                            // WithWindowsDesktopFeatures needed when used in Windows Desktop
                            //  .WithBroker(options)
                            //  .WithWindowsDesktopFeatures(options);
                            //  .WithParentActivityOrWindow(handle);

                            return builder.Build();
                        }
                        else
                        {
                            MessageBox.Show("Tokens not found when parsing onedrive_secrets.json", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Not possible to parse onedrive_secrets.json", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Resource not found when loading onedrive_secrets.json", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Assembly not found when loading onedrive_secrets.json", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;

        }
    }
}
