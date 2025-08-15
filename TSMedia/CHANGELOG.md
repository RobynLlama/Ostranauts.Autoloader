# Changelog

For the full changelog please read the project's [commit history](https://github.com/RobynLlama/Ostranauts.Autoloader/commits/main/)

## Version 0.1.10

- Will now fail gracefully if the user provided mod string doesn't exist

## Version 0.1.9

- Will now fail gracefully if the user provided mod string is not properly formatted

## Version 0.1.8

- Adds the ability to define soft dependencies in a mod's Autoload.Meta.toml file. This is fully backwards compatible and will not break existing meta files
- Adds a config file to handle a few settings
  - **BackupNeeded**: Managed automatically by Autoloader to determine if it needs to backup the existing load_order.json file. Will backup your load order the first time it runs if a backup doesn't already exist.
  - **UninstallMode**: Will clean up all changes made to the game and restore the backed up load_order.json file if it can. Once this flag is set Autoloader will no longer function until it is unset. Set this flag then run the game again prior to removal to ensure a completely clean uninstall process.

## Version 0.1.7

- Now respects the user defined mods directory
- Stores mods as fully qualified paths in the load_order.json file
- No longer uses Path.Combine patch for slight performance improvement during loading
- Outputs loaded mod names to game console

## Version 0.1.6

- Modified Dependencies in TS package

## Version 0.1.5

- Added output for mod loading order

## Version 0.1.4

- Uses TOML files for Autoload discovery rather than text

## Version 0.1.3

Initial Thunderstore Release!
