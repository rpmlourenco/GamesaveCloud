using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GamesaveCloudLib
{
    public abstract class ICloudDriveHelper
    {
        public string _username;

        protected IProgress<string> progress;

        public abstract ICloudFile GetFolder(string parentId, string name);
        public abstract IList<ICloudFile> GetFolders(string parentId);
        public abstract ICloudFile GetFile(string parentId, string name);
        public abstract IList<ICloudFile> GetFiles(string parentId);
        public abstract bool DownloadFile(ICloudFile driveFile, string outputPath);
        public abstract Task<bool> DownloadFileAsync(ICloudFile driveFile, string outputPath);
        public abstract ICloudFile UploadFile(string filePath, string folderId, bool checkExists);
        public abstract Task<bool> UploadFileAsync(string filePath, string folderId, bool checkExists);
        public abstract Task<bool> DeleteFile(string itemId);
        public abstract ICloudFile NewFolder(string name, string parentId, DateTime createdTime = default, DateTime modifiedTime = default);

        public static ICloudFile FindFile(string name, IList<ICloudFile> files)
        {

            if (files is not null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (file.Name.Equals(name))
                    {
                        return file;
                    }
                }
            }
            else
            {
                return null;
            }

            return null;
        }

        static String WildCardToRegular(String value)
        {
            // If you want to implement both "*" and "?"
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        public static bool IsMatch(string text, string filter)
        {
            var filters = filter.Split(';');
            foreach (string splitFilter in filters)
            {
                if (Regex.IsMatch(text, WildCardToRegular(splitFilter)))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsMatch(string text, string filterIn, string filterOut)
        {
            if (!string.IsNullOrEmpty(filterIn))
            {
                if (!string.IsNullOrEmpty(filterOut))
                {
                    if (IsMatch(text, filterIn) && !IsMatch(text, filterOut))
                    {
                        return true;
                    }
                }
                else
                {
                    if (IsMatch(text, filterIn))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(filterOut))
                {
                    if (!IsMatch(text, filterOut))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public static IList<ICloudFile> FilterFiles(string filterIn, string filterOut, IList<ICloudFile> files)
        {
            IList<ICloudFile> result = new List<ICloudFile>();

            if (files is not null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (IsMatch(file.Name, filterIn, filterOut))
                    {
                        result.Add(file);
                    }
                }
            }
            else
            {
                return null;
            }
            return result;
        }

        public static string[] FilterFiles(string filterIn, string filterOut, string[] files)
        {
            List<string> result = new();

            if (files is not null && files.Length > 0)
            {
                foreach (var file in files)
                {
                    if (IsMatch(Path.GetFileName(file), filterIn, filterOut))
                    {
                        result.Add(file);
                    }
                }
            }
            else
            {
                return null;
            }

            return result.ToArray();
        }


        public DateTime LastModifiedDate(string folderId, ref int totalFiles, int recursive, string filterIn, string filterOut, int level = 1)
        {
            DateTime lastModified = default;
            IList<ICloudFile> files;

            if (!String.IsNullOrEmpty(filterIn) || !String.IsNullOrEmpty(filterOut))
            {
                var allfiles = GetFiles(folderId);
                files = FilterFiles(filterIn, filterOut, allfiles);
            }
            else
            {
                files = GetFiles(folderId);
            }

            if (files is not null && files.Count > 0)
            {
                totalFiles += files.Count;
                foreach (var file in files)
                {

                    if (lastModified == default || file.ModifiedTime > lastModified)
                    {
                        lastModified = (DateTime)file.ModifiedTime;
                    }

                }
            }

            if (recursive == 0 || level < recursive)
            {
                var folders = GetFolders(folderId);
                totalFiles += folders.Count;
                if (folders is not null && folders.Count > 0)
                {
                    foreach (var folder in folders)
                    {

                        var folderLastModified = LastModifiedDate(folder.Id, ref totalFiles, recursive, filterIn, filterOut, level+1);
                        if (lastModified == default || folderLastModified > lastModified)
                        {
                            lastModified = folderLastModified;
                        }

                    }
                }
            }

            return lastModified;
        }
        public static DateTime LocalLastModifiedDate(string folderPath, ref int totalFiles, int recursive, string filterIn, string filterOut, int level = 1)
        {

            DateTime lastModified = default;

            //string[] fileEntries = Directory.GetFiles(folderPath);
            string[] fileEntries;
            if (!String.IsNullOrEmpty(filterIn) || !String.IsNullOrEmpty(filterOut))
            {
                var allFileEntries = Directory.GetFiles(folderPath);
                fileEntries = FilterFiles(filterIn, filterOut, allFileEntries);
            }
            else
            {
                fileEntries = Directory.GetFiles(folderPath);
            }

            if (fileEntries != null && fileEntries.Length > 0)
            {
                totalFiles += fileEntries.Length;
                foreach (var fileName in fileEntries)
                {
                    var fileLastModified = File.GetLastWriteTime(fileName);
                    if (lastModified == default || fileLastModified > lastModified)
                    {
                        lastModified = fileLastModified;
                    }
                }
            }

            if (recursive == 0 || level < recursive)
            {
                string[] folderEntries = Directory.GetDirectories(folderPath);
                if (folderEntries != null && folderEntries.Length > 0)
                {
                    totalFiles += folderEntries.Length;
                    foreach (var folderEntry in folderEntries)
                    {
                        var folderLastModified = LocalLastModifiedDate(folderEntry, ref totalFiles, recursive, filterIn, filterOut, level+1);
                        if (lastModified == default || folderLastModified > lastModified)
                        {
                            lastModified = folderLastModified;
                        }
                    }
                }
            }

            return lastModified;
        }

        public void SyncFromLocal(string folderPath, string folderId, int recursive, string filterIn, string filterOut, int level = 1)
        {

            // Delete files in drive which do not exist locally
            // var files = GetFiles(folderId);
            IList<ICloudFile> files;
            if (!String.IsNullOrEmpty(filterIn) || !String.IsNullOrEmpty(filterOut))
            {
                var allfiles = GetFiles(folderId);
                files = FilterFiles(filterIn, filterOut, allfiles);
            }
            else
            {
                files = GetFiles(folderId);
            }

            if (files is not null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    string checkFile = Path.Combine(folderPath, file.Name);
                    if (!File.Exists(checkFile))
                    {
                        DeleteFile(file.Id).Wait();
                    }
                }
            }

            IList<ICloudFile> folders = null;
            if (recursive == 0 || level < recursive)
                folders = GetFolders(folderId);

            if (folders is not null && folders.Count > 0)
            {
                foreach (var folder in folders)
                {
                    string checkFolder = Path.Combine(folderPath, folder.Name);
                    if (!Directory.Exists(checkFolder))
                    {
                        DeleteFile(folder.Id).Wait();
                    }
                }
            }

            // Uploads non existing or different files
            //string[] fileEntries = Directory.GetFiles(folderPath);
            string[] fileEntries;
            if (!String.IsNullOrEmpty(filterIn) || !String.IsNullOrEmpty(filterOut))
            {
                var allFileEntries = Directory.GetFiles(folderPath);
                fileEntries = FilterFiles(filterIn, filterOut, allFileEntries);
            }
            else
            {
                fileEntries = Directory.GetFiles(folderPath);
            }
            if (fileEntries != null && fileEntries.Length > 0)
            {
                foreach (var fileEntry in fileEntries)
                {
                    var fileLastModified = File.GetLastWriteTime(fileEntry);
                    long size = new FileInfo(fileEntry).Length;

                    var driveFile = FindFile(Path.GetFileName(fileEntry), files);
                    if (driveFile is null)
                    {
                        UploadFile(fileEntry, folderId, false);
                    }
                    else
                    {
                        DateTime driveLastModified = (DateTime)driveFile.ModifiedTime;
                        if (fileLastModified.ToString("yyyyMMdd HHmmss") != driveLastModified.ToString("yyyyMMdd HHmmss") || driveFile.Size != size)
                        {
                            DeleteFile(driveFile.Id).Wait();
                            UploadFile(fileEntry, folderId, false);
                        }
                    }
                }
            }

            if (recursive == 0 || level < recursive)
            {
                string[] folderEntries = Directory.GetDirectories(folderPath);
                if (folderEntries != null && folderEntries.Length > 0)
                {
                    foreach (var folderEntry in folderEntries)
                    {
                        string folderName = Path.GetFileName(folderEntry);
                        var driveFolder = FindFile(folderName, folders);
                        string driveFolderId;
                        if (driveFolder is null)
                        {
                            // driveFolder = DriveNewFolder(service, folderName, folderId, Directory.GetCreationTime(folderEntry), Directory.GetLastWriteTime(folderEntry))
                            driveFolderId = NewFolder(folderName, folderId).Id;
                        }
                        else
                        {
                            driveFolderId = driveFolder.Id;
                        }

                        SyncFromLocal(folderEntry, driveFolderId, recursive, filterIn, filterOut, level+1);
                    }
                }
            }
        }

        public void SyncFromDrive(string folderPath, string folderId, int recursive, string filterIn, string filterOut, int level = 1)
        {
            IList<ICloudFile> files;
            if (!String.IsNullOrEmpty(filterIn) || !String.IsNullOrEmpty(filterOut))
            {
                var allfiles = GetFiles(folderId);
                files = FilterFiles(filterIn, filterOut, allfiles);
            }
            else
            {
                files = GetFiles(folderId);
            }

            IList<ICloudFile> folders = null;
            if (recursive == 0 || level < recursive)
                folders = GetFolders(folderId);

            //string[] fileEntries = Directory.GetFiles(folderPath);
            string[] fileEntries;
            if (!String.IsNullOrEmpty(filterIn) || !String.IsNullOrEmpty(filterOut))
            {
                var allFileEntries = Directory.GetFiles(folderPath);
                fileEntries = FilterFiles(filterIn, filterOut, allFileEntries);
            }
            else
            {
                fileEntries = Directory.GetFiles(folderPath);
            }
            if (fileEntries != null && fileEntries.Length > 0)
            {
                foreach (var fileEntry in fileEntries)
                {

                    var driveFile = FindFile(Path.GetFileName(fileEntry), files);
                    if (driveFile is null)
                    {
                        File.Delete(fileEntry);
                    }
                }
            }

            if ((recursive == 0 || level < recursive) && folders != null)
            {
                string[] folderEntries = Directory.GetDirectories(folderPath);
                foreach (var folderEntry in folderEntries)
                {

                    string folderName = Path.GetFileName(folderEntry);
                    var driveFolder = FindFile(folderName, folders);

                    if (driveFolder is null)
                    {
                        Directory.Delete(folderEntry, true);
                    }
                }
            }

            if (files is not null && files.Count > 0)
            {
                foreach (var fileEntry in files)
                {
                    string checkFile = Path.Combine(folderPath, fileEntry.Name);
                    if (!File.Exists(checkFile))
                    {
                        DownloadFile(fileEntry, checkFile);
                    }
                    else
                    {
                        var fileLastModified = File.GetLastWriteTime(checkFile);
                        long size = new FileInfo(checkFile).Length;
                        DateTime driveLastModified = (DateTime)fileEntry.ModifiedTime;

                        if (fileLastModified.ToString("yyyyMMdd HHmmss") != driveLastModified.ToString("yyyyMMdd HHmmss") || fileEntry.Size != size)
                        {
                            DownloadFile(fileEntry, checkFile);
                        }
                    }
                }
            }

            if (folders is not null && folders.Count > 0)
            {
                foreach (var folder in folders)
                {

                    string checkFolder = Path.Combine(folderPath, folder.Name);
                    if (!Directory.Exists(checkFolder))
                    {
                        Directory.CreateDirectory(checkFolder);
                        // Directory.SetCreationTime(checkFolder, driveFolder.CreatedTime)
                        // Directory.SetLastWriteTime(checkFolder, driveFolder.ModifiedTime)
                    }

                    SyncFromDrive(checkFolder, folder.Id, recursive, filterIn, filterOut, level+1);
                }
            }

        }

        public async Task<bool> SyncFromLocalAsync(string folderPath, string folderId, int recursive, string filterIn, string filterOut, int level = 1)
        {
            //Console.WriteLine("Synching " + folderPath);

            // Delete files in drive which do not exist locally
            // var files = GetFiles(folderId);
            IList<ICloudFile> files;
            if (!String.IsNullOrEmpty(filterIn) || !String.IsNullOrEmpty(filterOut))
            {
                var allfiles = GetFiles(folderId);
                files = FilterFiles(filterIn, filterOut, allfiles);
            }
            else
            {
                files = GetFiles(folderId);
            }

            if (files is not null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    string checkFile = Path.Combine(folderPath, file.Name);
                    if (!File.Exists(checkFile))
                    {
                        await DeleteFile(file.Id);
                    }
                }
            }

            IList<ICloudFile> folders = null;
            if (recursive == 0 || level < recursive)
                folders = GetFolders(folderId);

            if (folders is not null && folders.Count > 0)
            {
                foreach (var folder in folders)
                {
                    string checkFolder = Path.Combine(folderPath, folder.Name);
                    if (!Directory.Exists(checkFolder))
                    {
                        await DeleteFile(folder.Id);
                    }
                }
            }


            // Uploads non existing or different files
            //string[] fileEntries = Directory.GetFiles(folderPath);
            string[] fileEntries;
            if (!String.IsNullOrEmpty(filterIn) || !String.IsNullOrEmpty(filterOut))
            {
                var allFileEntries = Directory.GetFiles(folderPath);
                fileEntries = FilterFiles(filterIn, filterOut, allFileEntries);
            }
            else
            {
                fileEntries = Directory.GetFiles(folderPath);
            }

            if (fileEntries != null && fileEntries.Length > 0)
            {
                var uploadTasks = new List<Task>();
                foreach (var fileEntry in fileEntries)
                {
                    var fileLastModified = File.GetLastWriteTime(fileEntry);
                    long size = new FileInfo(fileEntry).Length;

                    var driveFile = FindFile(Path.GetFileName(fileEntry), files);
                    if (driveFile is null)
                    {
                        //Console.WriteLine("Uploading " + fileEntry);
                        var uploadTask = UploadFileAsync(fileEntry, folderId, false);
                        uploadTasks.Add(uploadTask);
                    }
                    else
                    {
                        DateTime driveLastModified = (DateTime)driveFile.ModifiedTime;
                        if (fileLastModified.ToString("yyyyMMdd HHmmss") != driveLastModified.ToString("yyyyMMdd HHmmss") || driveFile.Size != size)
                        {
                            await DeleteFile(driveFile.Id);
                            //Console.WriteLine("Uploading " + fileEntry);
                            var uploadTask = UploadFileAsync(fileEntry, folderId, false);
                            uploadTasks.Add(uploadTask);
                        }
                    }
                }
                while (uploadTasks.Count > 0)
                {
                    Task finishedTask = await Task.WhenAny(uploadTasks);
                    //Console.WriteLine("Upload completed");
                    await finishedTask;
                    uploadTasks.Remove(finishedTask);
                }
            }

            if (recursive == 0 || level < recursive)
            {
                string[] folderEntries = Directory.GetDirectories(folderPath);
                if (folderEntries != null && folderEntries.Length > 0)
                {
                    var syncTasks = new List<Task>();
                    foreach (var folderEntry in folderEntries)
                    {
                        string folderName = Path.GetFileName(folderEntry);
                        var driveFolder = FindFile(folderName, folders);
                        string driveFolderId;
                        if (driveFolder is null)
                        {
                            // driveFolder = DriveNewFolder(service, folderName, folderId, Directory.GetCreationTime(folderEntry), Directory.GetLastWriteTime(folderEntry))
                            driveFolderId = NewFolder(folderName, folderId).Id;
                        }
                        else
                        {
                            driveFolderId = driveFolder.Id;
                        }
                        //await SyncFromLocalAsync(folderEntry, driveFolderId, recursive, filter);
                        var syncTask = SyncFromLocalAsync(folderEntry, driveFolderId, recursive, filterIn, filterOut, level+1);
                        syncTasks.Add(syncTask);
                    }
                    while (syncTasks.Count > 0)
                    {
                        Task finishedTask = await Task.WhenAny(syncTasks);
                        //Console.WriteLine("Sync completed");
                        await finishedTask;
                        syncTasks.Remove(finishedTask);
                    }
                }
            }
            return true;
        }

        public async Task<bool> SyncFromDriveAsync(string folderPath, string folderId, int recursive, string filterIn, string filterOut, int level = 1)
        {
            //Console.WriteLine("Synching " + folderPath);
            IList<ICloudFile> files;
            if (!String.IsNullOrEmpty(filterIn) || !String.IsNullOrEmpty(filterOut))
            {
                var allfiles = GetFiles(folderId);
                files = FilterFiles(filterIn, filterOut, allfiles);
            }
            else
            {
                files = GetFiles(folderId);
            }

            IList<ICloudFile> folders = null;
            if (recursive == 0 || level < recursive)
                folders = GetFolders(folderId);

            //string[] fileEntries = Directory.GetFiles(folderPath);
            string[] fileEntries;
            if (!String.IsNullOrEmpty(filterIn) || !String.IsNullOrEmpty(filterOut))
            {
                var allFileEntries = Directory.GetFiles(folderPath);
                fileEntries = FilterFiles(filterIn, filterOut, allFileEntries);
            }
            else
            {
                fileEntries = Directory.GetFiles(folderPath);
            }
            if (fileEntries != null && fileEntries.Length > 0)
            {
                foreach (var fileEntry in fileEntries)
                {

                    var driveFile = FindFile(Path.GetFileName(fileEntry), files);
                    if (driveFile is null)
                    {
                        File.Delete(fileEntry);
                    }
                }
            }

            if ((recursive == 0 || level < recursive) && folders != null)
            {
                string[] folderEntries = Directory.GetDirectories(folderPath);
                foreach (var folderEntry in folderEntries)
                {

                    string folderName = Path.GetFileName(folderEntry);
                    var driveFolder = FindFile(folderName, folders);

                    if (driveFolder is null)
                    {
                        Directory.Delete(folderEntry, true);
                    }
                }
            }

            if (files is not null && files.Count > 0)
            {
                var downloadTasks = new List<Task>();
                foreach (var fileEntry in files)
                {
                    string checkFile = Path.Combine(folderPath, fileEntry.Name);
                    if (!File.Exists(checkFile))
                    {
                        //Console.WriteLine("Downloading " + fileEntry.Name);
                        var downloadTask = DownloadFileAsync(fileEntry, checkFile);
                        downloadTasks.Add(downloadTask);
                    }
                    else
                    {
                        var fileLastModified = File.GetLastWriteTime(checkFile);
                        long size = new FileInfo(checkFile).Length;
                        DateTime driveLastModified = (DateTime)fileEntry.ModifiedTime;

                        if (fileLastModified.ToString("yyyyMMdd HHmmss") != driveLastModified.ToString("yyyyMMdd HHmmss") || fileEntry.Size != size)
                        {
                            //Console.WriteLine("Downloading " + fileEntry.Name);
                            var downloadTask = DownloadFileAsync(fileEntry, checkFile);
                            downloadTasks.Add(downloadTask);
                        }
                    }
                }
                while (downloadTasks.Count > 0)
                {
                    Task finishedTask = await Task.WhenAny(downloadTasks);
                    //Console.WriteLine("Download completed");
                    await finishedTask;
                    downloadTasks.Remove(finishedTask);
                }
            }

            if (folders is not null && folders.Count > 0)
            {
                var syncTasks = new List<Task>();
                foreach (var folder in folders)
                {

                    string checkFolder = Path.Combine(folderPath, folder.Name);
                    if (!Directory.Exists(checkFolder))
                    {
                        Directory.CreateDirectory(checkFolder);
                        // Directory.SetCreationTime(checkFolder, driveFolder.CreatedTime)
                        // Directory.SetLastWriteTime(checkFolder, driveFolder.ModifiedTime)
                    }

                    var syncTask = SyncFromDriveAsync(checkFolder, folder.Id, recursive, filterIn, filterOut, level+1);
                    syncTasks.Add(syncTask);
                }
                while (syncTasks.Count > 0)
                {
                    Task finishedTask = await Task.WhenAny(syncTasks);
                    //Console.WriteLine("Sync completed");
                    await finishedTask;
                    syncTasks.Remove(finishedTask);
                }
            }

            return true;
        }

        public void Log(string message)
        {
            progress.Report(message);
            //Console.Write(message);
        }

    }
}
