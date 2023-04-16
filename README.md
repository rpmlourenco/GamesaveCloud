# GamesaveCloud

Todo:
- updater does not delete existing files
- test wildcards
- fix bug when moving credentials to another pc
- implement parallelism
- Admin tool
- open savegame folder
- delete local files, delete cloud files
- open backup folder
- delete backup folder
- status bar, initializing, full sync, monitoring, partial sync
- delete permanently from onedrive

Done:
- Create local backup when synchronizing from cloud
- Disable File Watcher when synchronizing from cloud and machine update
- Scroll to bottom
- Indication synch from or to cloud
- avoid using one drive query by name since it takes long, search in memory instead and compare performance
- test one drive implementation
- handle and log exceptions
- log database sync status
- include option to sync all, arguments and help
- sync by name with unique constraint
- Check for new version download from cloud
- implement filters and recursiveness
- build script to update playnite library and config on startup