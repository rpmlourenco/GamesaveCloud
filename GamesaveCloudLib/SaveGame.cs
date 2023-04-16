
namespace GamesaveCloudLib
{
    public class SaveGame
    {
        public int game_id;
        public int savegame_id;
        public string title;
        public string path;
        public Google.Apis.Drive.v3.Data.File driveFolder;

        public SaveGame(int game_id, int savegame_id, string title, string path, Google.Apis.Drive.v3.Data.File driveFolder)
        {
            this.game_id = game_id;
            this.savegame_id = savegame_id;
            this.title = title;
            this.path = path;
            this.driveFolder = driveFolder;
        }
    }
}