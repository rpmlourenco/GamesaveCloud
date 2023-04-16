using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GamesaveCloudLib
{

    public class GoogleDriveFile : ICloudFile
    {
        public Google.Apis.Drive.v3.Data.File file;

        public GoogleDriveFile(Google.Apis.Drive.v3.Data.File file)
        {
            this.file = file;
            this.Id = file.Id;
            if (file.ModifiedTime != null)
                this.ModifiedTime = (DateTime)file.ModifiedTime;
            if (file.ModifiedTime != null)
                this.CreatedTime = (DateTime)file.CreatedTime;
            this.Name = file.Name;
        }
    }

    public class GoolgeDriveHelper : ICloudDriveHelper
    {

        private readonly UserCredential credentials;
        private readonly DriveService service;


        public GoolgeDriveHelper()
        {
            string resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith("googledrive_secrets.json"));
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

            credentials = Authenticate(stream);
            service = OpenService(credentials);
        }

        private static UserCredential Authenticate(Stream stream)
        {
            UserCredential credential;

            using (stream)
            {
                string pathAtual = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string pathCredential = Path.Combine(pathAtual, "credential");
                Directory.CreateDirectory(pathCredential);
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.FromStream(stream).Secrets, new[] { DriveService.Scope.Drive }, "user", System.Threading.CancellationToken.None, new FileDataStore(pathCredential, true)).Result;
            }
            return credential;
        }

        private DriveService OpenService(UserCredential credential)
        {
            var service = new DriveService(new BaseClientService.Initializer() { HttpClientInitializer = credential });

            var request = service.About.Get();
            request.Fields = "user";
            var about = request.Execute();
            _username = about.User.EmailAddress;

            return service;
        }

        private IList<ICloudFile> Query(string sQuery, string sFields)
        {
            var request = service.Files.List();
            request.Q = sQuery;
            request.Spaces = "drive";
            request.Fields = sFields;

            var files = request.Execute().Files;

            List<ICloudFile> ret = new();
            foreach (var file in files)
            {
                ret.Add(new GoogleDriveFile(file));
            }
            return ret;
        }

        public override IList<ICloudFile> GetFiles(string parentId)
        {
            string sQuery;
            //if (name is not null)
            //{
            //    sQuery = "mimeType != 'application/vnd.google-apps.folder' and trashed = false and parents in '" + parentId + "' and name = '" + name + "'";
            //}
            //else
            //{
            sQuery = "mimeType != 'application/vnd.google-apps.folder' and trashed = false and parents in '" + parentId + "'";
            //}

            var files = Query(sQuery, "files(id,name,createdTime,modifiedTime,size)");
            return files;
        }

        public override IList<ICloudFile> GetFolders(string parentId)
        {

            string sQuery;
            //if (name is not null)
            //{
            //    sQuery = "mimeType = 'application/vnd.google-apps.folder' and trashed = false and parents in '" + parentId + "' and name = '" + name + "'";
            //}
            //else
            //{
            sQuery = "mimeType = 'application/vnd.google-apps.folder' and trashed = false and parents in '" + parentId + "'";
            //}

            var files = Query(sQuery, "files(id,name)");
            return files;
        }

        public override ICloudFile GetFile(string parentId, string name)
        {

            var files = GetFiles(parentId);
            if (files is not null && files.Count > 0)
            {
                foreach (var file in files)
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

        public override ICloudFile GetFolder(string parentId, string name)
        {

            var folders = GetFolders(parentId);
            if (folders is not null && folders.Count > 0)
            {
                foreach (var folder in folders)
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

        public override ICloudFile UploadFile(string filePath, string folderId, bool checkExists)
        {

            var plist = new List<string>
            {
                folderId // Set parent driveFolder
            };

            string fileName = Path.GetFileName(filePath);

            if (File.Exists(filePath))
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = Path.GetFileName(filePath),
                    Parents = plist,
                    CreatedTime = File.GetCreationTime(filePath),
                    ModifiedTime = File.GetLastWriteTime(filePath)
                };

                if (checkExists)
                {
                    var drivefile = GetFile(folderId, fileName);
                    if (drivefile is not null)
                    {
                        service.Files.Delete(drivefile.Id).Execute();
                    }
                }

                FilesResource.CreateMediaUpload request;

                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    request = service.Files.Create(fileMetadata, stream, "application/octet-stream");
                    request.Fields = "id, parents";
                    request.Upload();
                }

                return new GoogleDriveFile(request.ResponseBody);
            }
            else
            {
                return null;
            }

        }

        /*
        private bool DownloadFileByName(string fileName, string parentId, string outputPath)
        {

            string fullOutputPath = Path.Combine(outputPath, fileName);
            var outputStream = new FileStream(fullOutputPath, FileMode.Create, FileAccess.ReadWrite);
            var driveFile = GetFile(parentId, fileName);

            if (driveFile is not null)
            {

                var request = service.Files.Get(driveFile.Id);
                request.Download(outputStream);
                outputStream.Flush();
                outputStream.Close();

                File.SetLastWriteTime(fullOutputPath, (DateTime)driveFile.ModifiedTime);
                File.SetCreationTime(fullOutputPath, (DateTime)driveFile.CreatedTime);

                return true;
            }
            else
            {
                return false;
            }
        }
        */

        public override bool DownloadFile(ICloudFile driveFile, string outputPath)
        {

            //string fileName = Path.GetFileName(outputPath);
            var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite);

            var request = service.Files.Get(driveFile.Id);
            request.Download(outputStream);
            outputStream.Flush();
            outputStream.Close();

            File.SetLastWriteTime(outputPath, (DateTime)driveFile.ModifiedTime);
            File.SetCreationTime(outputPath, (DateTime)driveFile.CreatedTime);

            return true;

        }

        /*
        private void DeleteFileByName(string fileName, string parentId)
        {
            var driveFile = GetFile(parentId, fileName);
            if (driveFile is not null)
            {
                service.Files.Delete(driveFile.Id).Execute();
            }
        }
        */

        public override void DeleteFile(string itemId)
        {
            service.Files.Delete(itemId).Execute();
        }


        public override ICloudFile NewFolder(string name, string parentId, DateTime createdTime = default, DateTime modifiedTime = default)
        {

            var plist = new List<string>
            {
                parentId // Set parent driveFolder
            };

            var folder = new Google.Apis.Drive.v3.Data.File
            {
                Name = name,
                MimeType = "application/vnd.google-apps.folder",
                Parents = plist
            };
            if (createdTime != default)
            {
                folder.CreatedTime = createdTime;
            }
            if (modifiedTime != default)
            {
                folder.ModifiedTime = modifiedTime;
            }

            var request = service.Files.Create(folder);
            return new GoogleDriveFile(request.Execute());

        }

    }
}