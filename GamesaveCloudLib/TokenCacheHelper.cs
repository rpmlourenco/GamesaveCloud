﻿//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using Microsoft.Identity.Client;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace GamesaveCloudLib
{
    public static class TokenCacheHelper
    {
        //public string workingPath;
        private static string workingPath { get; set; }

        public static void Initialize(string aWorkingPath)
        {
            workingPath = aWorkingPath;
            //var pathCurrent = Path.GetDirectoryName(workingPath);
            string pathCredential = Path.Combine(workingPath, "credential");
            Directory.CreateDirectory(pathCredential);
            CacheFilePath = Path.Combine(pathCredential, "OneDrive.msalcache.bin3");
        }

        static TokenCacheHelper()
        {
            if (workingPath != null)
            {
                //var workingPath = Environment.ProcessPath;
                //string pathCurrent = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                //string pathCurrent = Path.GetDirectoryName(System.AppContext.BaseDirectory);
                //var pathCurrent = Path.GetDirectoryName(workingPath);
                string pathCredential = Path.Combine(workingPath, "credential");
                Directory.CreateDirectory(pathCredential);

                //string exe = Path.GetFileNameWithoutExtension(System.Environment.ProcessPath);
                CacheFilePath = Path.Combine(pathCredential, "OneDrive.msalcache.bin3");
            }
        }

        /// <summary>
        /// sPath to the token cache
        /// </summary>
        public static string CacheFilePath
        {
            get; private set;
        }

        private static readonly object FileLock = new();

        [SupportedOSPlatform("windows")]
        public static void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                try
                {
                    args.TokenCache.DeserializeMsalV3(File.Exists(CacheFilePath)
                            ? ProtectedData.Unprotect(File.ReadAllBytes(CacheFilePath),
                                                     null,
                                                     DataProtectionScope.CurrentUser)
                            : null);
                }
                catch
                {
                    args.TokenCache.DeserializeMsalV3(null);
                }
            }
        }

        [SupportedOSPlatform("windows")]
        public static void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                lock (FileLock)
                {
                    // reflect changes in the persistent store
                    File.WriteAllBytes(CacheFilePath,
                                       ProtectedData.Protect(args.TokenCache.SerializeMsalV3(),
                                                             null,
                                                             DataProtectionScope.CurrentUser)
                                      );
                }
            }
        }

        [SupportedOSPlatform("windows")]
        internal static void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
        }
    }
}
