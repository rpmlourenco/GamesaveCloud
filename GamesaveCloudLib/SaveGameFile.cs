
namespace GamesaveCloudLib
{
    public class SaveGameFile
    {
        public SaveGame savegame;
        public string path;
        public string oldPath;
        public string changeType;

        public SaveGameFile(SaveGame savegame, string path, string oldPath, string changeType)
        {
            this.savegame = savegame;
            this.path = path;
            this.oldPath = oldPath;
            this.changeType = changeType;
        }

    }
}