using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace GamesaveCloudLib
{

    public class OneDriveFile : ICloudFile
    {
        public Microsoft.Graph.Models.DriveItem file;

        public OneDriveFile(Microsoft.Graph.Models.DriveItem file)
        {
            this.file = file;
            this.Id = file.Id;
            //this.ModifiedTime = ((DateTimeOffset)file.LastModifiedDateTime).UtcDateTime;
            //this.CreatedTime = ((DateTimeOffset)file.CreatedDateTime).UtcDateTime;
            this.ModifiedTime = ((DateTimeOffset)file.FileSystemInfo.LastModifiedDateTime).LocalDateTime;
            this.CreatedTime = ((DateTimeOffset)file.FileSystemInfo.CreatedDateTime).LocalDateTime;
            this.Size = (long)file.Size;
            this.Name = file.Name;

        }
    }
    public class OneDriveHelper : ICloudDriveHelper
    {

        //static RichTextBox? console;
        //static Button? _button;
        static int howToSign;
        static IntPtr handle;
        public string _userDriveId;
        public string _rootId;
        public string pathCredential;

        // - The content of Tenant by the information about the accounts allowed to sign-in in your application:
        //   - For Work or School account in your org, use your tenant ID, or domain
        //   - for any Work or School accounts, use organizations
        //   - for any Work or School accounts, or Microsoft personal account, use common
        //   - for Microsoft Personal account, use consumers
        private readonly string Instance = "https://login.microsoftonline.com/";
        private readonly string MSGraphURL = "https://graph.microsoft.com/v1.0/";
        private readonly IPublicClientApplication _clientApp;
        private GraphServiceClient _graphClient;

        //Set the scope for API call to user.read
        static readonly string[] scopes = new string[] { "user.read", "Files.ReadWrite.All" };

        [SupportedOSPlatform("windows")]
        public OneDriveHelper()
        {
            string pathCurrent = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            pathCredential = Path.Combine(pathCurrent, "credential");

            howToSign = 2;
            handle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            //BrokerOptions options = new(BrokerOptions.OperatingSystems.Windows);

            string resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith("onedrive.json"));
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            using var sr = new StreamReader(stream);
            string content = sr.ReadToEnd();
            JObject json = JObject.Parse(content);
            string ClientId = json["client_id"].Value<string>();
            string Tenant = json["tenant"].Value<string>();

            var builder = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority($"{Instance}{Tenant}")
                .WithDefaultRedirectUri();
            //.WithWindowsDesktopFeatures(options);
            //.WithParentActivityOrWindow(handle);

            _clientApp = builder.Build();
            TokenCacheHelper.EnableSerialization(_clientApp.UserTokenCache);

            SignInAndInitializeGraphServiceClient().Wait();
        }

        /// <summary>
        /// Call AcquireToken - to acquire a token requiring user to sign-in
        /// </summary>
        public async Task<string> GetToken(string[] scopes)
        {

            if (_clientApp == null)
            {
                return "";
            }

            AuthenticationResult authResult = null;
            IAccount firstAccount = null;

            switch (howToSign)
            {
                // 0: Use account used to signed-in in Windows (WAM)
                case 0:
                    // WAM will always get an account in the cache. So if we want
                    // to have a chance to select the accounts interactively, we need to
                    // force the non-account
                    firstAccount = Microsoft.Identity.Client.PublicClientApplication.OperatingSystemAccount;
                    break;

                //  1: Use one of the Accounts known by Windows(WAM)
                case 1:
                    // We force WAM to display the dialog with the accounts
                    firstAccount = null;
                    break;

                //  Try to use the last account
                default:
                    string username = null;
                    try
                    {
                        string fileName = Path.Combine(pathCredential, "OneDrive.accountcache.json");
                        username = File.ReadAllText(fileName);
                    }
                    catch (Exception)
                    {
                    }
                    var myaccounts = await _clientApp.GetAccountsAsync();
                    foreach (var account in myaccounts)
                    {
                        if (username != null)
                        {
                            if (account.Username.Equals(username))
                            {
                                firstAccount = account;
                                break;
                            }
                        }
                        else
                        {
                            firstAccount = account;
                            break;
                        }
                    }
                    break;
            }

            try
            {
                authResult = await _clientApp.AcquireTokenSilent(scopes, firstAccount)
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent. 
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                try
                {
                    authResult = await _clientApp.AcquireTokenInteractive(scopes)
                        .WithAccount(firstAccount)
                        .WithParentActivityOrWindow(handle)
                        .WithPrompt(Microsoft.Identity.Client.Prompt.SelectAccount)
                        .ExecuteAsync();
                }
                catch (MsalException msalex)
                {
                    Log($"Error Acquiring Token:{System.Environment.NewLine}{msalex}");
                }
            }
            catch (Exception ex)
            {
                Log($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}");
                return "";
            }

            if (authResult != null)
            {
                //ResultText.Text = await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken);
                //string jsonStringS = authResult.Account.Username;                

                if (_username == null)
                {
                    File.WriteAllText(Path.Combine(pathCredential, "OneDrive.accountcache.json"), authResult.Account.Username);
                    _username = authResult.Account.Username;
                }

                //DisplayBasicTokenInfo(authResult);
                //this.SignOutButton.Visibility = Visibility.Visible;
                return authResult.AccessToken;
            }
            else { return ""; }

        }

        /// <summary>
        /// Sign in user using MSAL and obtain a token for Microsoft Graph
        /// </summary>
        /// <returns>GraphServiceClient</returns>
        public async Task<GraphServiceClient> SignInAndInitializeGraphServiceClient()
        {
            TokenProvider tokenProvider = new(GetToken, scopes);
            var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);
            var graphClient = new GraphServiceClient(authProvider, MSGraphURL);

            _graphClient = graphClient;

            // Get the user's driveId
            var driveItem = await _graphClient.Me.Drive.GetAsync();
            if (driveItem != null)
            {
                _userDriveId = driveItem.Id;

                var root = await _graphClient.Drives[_userDriveId].Root.GetAsync();
                if (root != null)
                {
                    _rootId = root.Id;
                }

            }
            else { Log("Could not retrieve OneDrive Id"); }

            return await Task.FromResult(graphClient);
        }

        public override ICloudFile GetFolder(string parentId, string name)
        {
            var result = Query("folder ne null", parentId);
            if (result != null && result.Count > 0)
            {
                foreach (var folder in result)
                {
                    if (folder.Name == name) return folder;
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        public override ICloudFile GetFile(string parentId, string name)
        {
            var result = Query("folder eq null", parentId);
            if (result != null && result.Count > 0)
            {
                foreach (var file in result)
                {
                    if (file.Name == name) return file;
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        public IList<ICloudFile> Query(string filter, string parentId, string name = null)
        {
            if (_graphClient == null || parentId == null) { return null; }

            if (parentId.Equals("root")) { parentId = _rootId; }

            List<Microsoft.Graph.Models.DriveItem> children = null;
            if (name is not null)
            {
                var task = Task.Run(() => _graphClient.Drives[_userDriveId].Items[parentId].SearchWithQ(name).GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.QueryParameters.Filter = filter;
                }));
                task.Wait();
                if (task.Result != null)
                {
                    children = task.Result.Value;
                }
            }
            else
            {
                var task = Task.Run(() => _graphClient.Drives[_userDriveId].Items[parentId].Children.GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.QueryParameters.Filter = filter;
                }));
                task.Wait();
                if (task.Result != null)
                {
                    children = task.Result.Value;
                }
            }

            if (children != null)
            {
                List<ICloudFile> ret = new();
                foreach (var child in children)
                {
                    ret.Add(new OneDriveFile(child));
                }
                return ret;
            }
            else
            {
                return null;
            }
        }

        public override IList<ICloudFile> GetFiles(string parentId)
        {
            return Query("folder eq null", parentId);
        }

        public override IList<ICloudFile> GetFolders(string parentId)
        {
            return Query("folder ne null", parentId);
        }

        public override ICloudFile NewFolder(string name, string parentId, DateTime createdTime = default, DateTime modifiedTime = default)
        {
            if (parentId.Equals("root")) { parentId = _rootId; }

            var driveItem = new DriveItem
            {
                Name = name,
                Folder = new Folder
                {
                },
                AdditionalData = new Dictionary<string, object>()
                {
                    {"@microsoft.graph.conflictBehavior", "fail"}
                }
            };

            var task = Task.Run(() => _graphClient.Drives[_userDriveId].Items[parentId].Children.PostAsync(driveItem));
            task.Wait();
            if (task.Result != null)
            {
                return new OneDriveFile(task.Result);
            }
            else { return null; }

        }

        public override void DeleteFile(string itemId)
        {
            _graphClient.Drives[_userDriveId].Items[itemId].DeleteAsync().Wait();
        }

        public override ICloudFile UploadFile(string filePath, string folderId, bool checkExists)
        {
            string fileName = Path.GetFileName(filePath);

            if (folderId.Equals("root")) { folderId = _rootId; }

            if (File.Exists(filePath))
            {
                if (folderId.Equals("root")) { folderId = _rootId; }
                DateTimeOffset modifiedTime = (DateTimeOffset)DateTime.SpecifyKind(File.GetLastWriteTime(filePath), DateTimeKind.Local);
                DateTimeOffset createdTime = (DateTimeOffset)DateTime.SpecifyKind(File.GetCreationTime(filePath), DateTimeKind.Local);

                if (checkExists)
                {
                    var drivefile = GetFile(folderId, fileName);
                    if (drivefile is not null)
                    {
                        _graphClient.Drives[_userDriveId].Items[drivefile.Id].DeleteAsync().Wait();
                    }
                }

                FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read);
                var task = Task.Run(() => _graphClient.Drives[_userDriveId].Items[folderId].ItemWithPath(fileName).Content.PutAsync(fileStream));
                task.Wait();
                if (task.Result != null)
                {
                    var result = new OneDriveFile(task.Result);

                    var driveItem = new DriveItem()
                    {
                        FileSystemInfo = new Microsoft.Graph.Models.FileSystemInfo()
                        {
                            LastModifiedDateTime = modifiedTime,
                            CreatedDateTime = createdTime
                        }
                    };

                    try
                    {
                        _graphClient.Drives[_userDriveId].Items[result.Id].PatchAsync(driveItem).Wait();
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                        _graphClient.Drives[_userDriveId].Items[result.Id].PatchAsync(driveItem).Wait();
                    }

                    return result;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public override bool DownloadFile(ICloudFile driveFile, string outputPath)
        {
            var task = Task.Run(() => _graphClient.Drives[_userDriveId].Items[driveFile.Id].Content.GetAsync());
            task.Wait();
            if (task.Result != null)
            {
                using (var fileStream = File.Create(outputPath))
                {
                    //myOtherObject.InputStream.Seek(0, SeekOrigin.Begin);
                    task.Result.CopyTo(fileStream);
                }
                File.SetLastWriteTime(outputPath, (DateTime)driveFile.ModifiedTime);
                File.SetCreationTime(outputPath, (DateTime)driveFile.CreatedTime);

                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
