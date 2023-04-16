using System;

namespace GamesaveCloudLib
{
    public class ICloudFile
    {
        public string Id;
        public DateTime ModifiedTime;
        public DateTime CreatedTime;
        public long Size;
        public string Name;
    }
}
