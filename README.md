# GamesaveCloud

Todo:
- fix issue sync with google drive from manager and not logging exception and not working afer that
- make provider same as default in syncform
- make cloud service optional in cli
- manager total game count
- Use logger in updater
- remove Playnite scripts
- remove "My Project" folders
- change manager Database to real one
- add icon to manager
- test wildcards
- fix bug when moving credentials to another pc
- implement parallelism
- upload database to cloud
- delete local files, delete cloud files
- delete permanently from onedrive
- search igdb, get id directly from igdb
- admin check incompatibility between machine and filter

Done:
- Create local backup when synchronizing from cloud
- disable File Watcher when synchronizing from cloud and machine update
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
- updater does not delete existing files
- admin tool
- open savegame folder