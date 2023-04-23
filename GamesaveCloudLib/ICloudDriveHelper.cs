using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GamesaveCloudLib
{
    public abstract class ICloudDriveHelper
    {
        public string _username;

        public abstract ICloudFile GetFolder(string parentId, string name);
        public abstract IList<ICloudFile> GetFolders(string parentId);
        public abstract ICloudFile GetFile(string parentId, string name);
        public abstract IList<ICloudFile> GetFiles(string parentId);
        public abstract bool DownloadFile(ICloudFile driveFile, string outputPath);
        public abstract ICloudFile UploadFile(string filePath, string folderId, bool checkExists);
        public abstract void DeleteFile(string itemId);
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

        public static IList<ICloudFile> FilterFiles(string expression, IList<ICloudFile> files)
        {
            IList<ICloudFile> result = new List<ICloudFile>();

            if (files is not null && files.Count > 0)
            {
                // If you want to implement both "*" and "?"
                static String WildCardToRegular(String value)
                {
                    return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
                }
                foreach (var file in files)
                {
                    if (Regex.IsMatch(file.Name, WildCardToRegular(expression)))
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

        public static string[] FilterFiles(string expression, string[] files)
        {
            List<string> result = new();

            if (files is not null && files.Length > 0)
            {
                // If you want to implement both "*" and "?"
                static String WildCardToRegular(String value)
                {
                    return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
                }
                foreach (var file in files)
                {
                    if (Regex.IsMatch(Path.GetFileName(file), WildCardToRegular(expression)))
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


        public DateTime LastModifiedDate(string folderId, ref int totalFiles, bool recursive, string filter)
        {
            DateTime lastModified = default;
            IList<ICloudFile> files;

            if (!String.IsNullOrEmpty(filter))
            {
                var allfiles = GetFiles(folderId);
                files = FilterFiles(filter, allfiles);
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

            if (recursive)
            {
                var folders = GetFolders(folderId);
                totalFiles += folders.Count;
                if (folders is not null && folders.Count > 0)
                {
                    foreach (var folder in folders)
                    {

                        var folderLastModified = LastModifiedDate(folder.Id, ref totalFiles, recursive, filter);
                        if (lastModified == default || folderLastModified > lastModified)
                        {
                            lastModified = folderLastModified;
                        }

                    }
                }
            }

            return lastModified;
        }
        public DateTime LocalLastModifiedDate(string folderPath, ref int totalFiles, bool recursive, string filter)
        {

            DateTime lastModified = default;

            //string[] fileEntries = Directory.GetFiles(folderPath);
            string[] fileEntries;
            if (!String.IsNullOrEmpty(filter))
            {
                var allFileEntries = Directory.GetFiles(folderPath);
                fileEntries = FilterFiles(filter, allFileEntries);
            }
            else
            {
                fileEntries = Directory.GetFiles(folderPath);
            }

            totalFiles += fileEntries.Length;
            foreach (var fileName in fileEntries)
            {
                var fileLastModified = File.GetLastWriteTime(fileName);
                if (lastModified == default || fileLastModified > lastModified)
                {
                    lastModified = fileLastModified;
                }
            }

            if (recursive)
            {
                string[] folderEntries = Directory.GetDirectories(folderPath);
                totalFiles += folderEntries.Length;
                foreach (var folderEntry in folderEntries)
                {
                    var folderLastModified = LocalLastModifiedDate(folderEntry, ref totalFiles, recursive, filter);
                    if (lastModified == default || folderLastModified > lastModified)
                    {
                        lastModified = folderLastModified;
                    }
                }
            }

            return lastModified;
        }

        public void SyncFromLocal(string folderPath, string folderId, bool recursive, string filter)
        {

            // Delete files in drive which do not exist locally
            // var files = GetFiles(folderId);
            IList<ICloudFile> files;
            if (!String.IsNullOrEmpty(filter))
            {
                var allfiles = GetFiles(folderId);
                files = FilterFiles(filter, allfiles);
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
                        DeleteFile(file.Id);
                    }
                }
            }

            IList<ICloudFile> folders = null;
            if (recursive)
                folders = GetFolders(folderId);

            if (folders is not null && folders.Count > 0)
            {
                foreach (var folder in folders)
                {
                    string checkFolder = Path.Combine(folderPath, folder.Name);
                    if (!Directory.Exists(checkFolder))
                    {
                        DeleteFile(folder.Id);
                    }
                }
            }

            // Uploads non existing or different files
            //string[] fileEntries = Directory.GetFiles(folderPath);
            string[] fileEntries;
            if (!String.IsNullOrEmpty(filter))
            {
                var allFileEntries = Directory.GetFiles(folderPath);
                fileEntries = FilterFiles(filter, allFileEntries);
            }
            else
            {
                fileEntries = Directory.GetFiles(folderPath);
            }

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
                        DeleteFile(driveFile.Id);
                        UploadFile(fileEntry, folderId, false);
                    }
                }
            }

            if (recursive)
            {
                string[] folderEntries = Directory.GetDirectories(folderPath);
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

                    SyncFromLocal(folderEntry, driveFolderId, recursive, filter);
                }
            }
        }

        public void SyncFromDrive(string folderPath, string folderId, bool recursive, string filter)
        {
            IList<ICloudFile> files;
            if (!String.IsNullOrEmpty(filter))
            {
                var allfiles = GetFiles(folderId);
                files = FilterFiles(filter, allfiles);
            }
            else
            {
                files = GetFiles(folderId);
            }

            IList<ICloudFile> folders = null;
            if (recursive)
                folders = GetFolders(folderId);

            //string[] fileEntries = Directory.GetFiles(folderPath);
            string[] fileEntries;
            if (!String.IsNullOrEmpty(filter))
            {
                var allFileEntries = Directory.GetFiles(folderPath);
                fileEntries = FilterFiles(filter, allFileEntries);
            }
            else
            {
                fileEntries = Directory.GetFiles(folderPath);
            }
            foreach (var fileEntry in fileEntries)
            {

                var driveFile = FindFile(Path.GetFileName(fileEntry), files);
                if (driveFile is null)
                {
                    File.Delete(fileEntry);
                }
            }

            if (recursive && folders != null)
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

                    SyncFromDrive(checkFolder, folder.Id, recursive, filter);
                }
            }

        }

        public static void Log(string message)
        {
            Console.Write(message);
        }

    }
}
