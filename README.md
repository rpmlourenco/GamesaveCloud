# GamesaveCloud

Todo:
- filter choose case sensitive
- delete local files, delete cloud files
- delete permanently from onedrive

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
- fix issue sync with google drive from manager and not logging exception and not working afer that
- remove Playnite scripts
- make provider same as default in syncform
- remove "My Project" folders
- change manager Database to real one
- upload database to cloud
- check if credentials are shared between cli and desktop
- fix bug when ini is not present
- cli does not read desktop tokens
- unhandled exception in cli
- make single file to avoid conflicts in shared dll
- fix bug when moving credentials to another pc
- test wildcards
- improve parameter parsing unknown direction
- make cloud service optional in cli
- separate two updates
- search igdb, get id directly from igdb
- implement parallelism
- add icon to manager
- manager total game count
- admin check incompatibility between machine and filter
- filter exclude, filters separated with ;