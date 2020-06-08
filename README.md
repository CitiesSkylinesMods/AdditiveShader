# AdditiveShader

Additive Shader mod allows asset creators to add time-based light effects to
their props, buildings and vehicles.

Based on [Additive Shader mod](https://steamcommunity.com/sharedfiles/filedetails/?id=1410003347) by **Ronyx69** and **Simon Ryr**.

This repository was setup and code updated with permission of Ronyx69.

## Asset creators

* [Making Additive Shader Assets](https://cslmodding.info/mod/additive-shader/)
* [Mod Tools Scripts](./SCRIPTS.md)
* [Settings Guide](./SETTINGS.md) - detailed info on script settings

## Modders

Work is in progress to allow remote control of shaders.

* [DRAFT: Modding Guide](./MODS.md)

## Contributing

Code improvements always welcome!

* Clone the repository locally using a [GitHUb client](https://github.com/CitiesSkylinesMods/TMPE/wiki/GitHub-Clients).
* You'll need an IDE (see [this list](https://github.com/CitiesSkylinesMods/TMPE/wiki/Dev-Tools)) to compile the code in to a dll file.
* Set folders for managed dlls and publish dir are defined in [Directory.Build.props](https://github.com/CitiesSkylinesMods/AdditiveShader/blob/master/Source/Directory.Build.props) - you might need to change them depending on where you installed the game.
    * Please do NOT push changes to this file back to the repo.
* Open the **AdditiveShader.sln** solution file in the IDE and then build.

## Changelog

### Version 1.5.0

> A complete rewrite of the mod with a focus on maintainability, performance,
and compatibility with other mods. Depending on number of shader-using assets
you have subscribed, you could see an increase of 1-3 fps in-game.

- Added: Now works in new scenario games!
- Added: Keywords for common times (`DayTime`, `NightTime`, `AlwaysOn`)
- Added: Detection of legacy NightTime shaders (on at dusk, off at dawn)
- Added: Folksonomic shader tagging and filtering (getting ready for v2.0 release)
- Added: Code analyzers (Roslynator, StyleCop, Microsoft, Humanizer, Unity...)
- Added: Code documentation for essential members
- Added: Detailed (but super-fast!) asset report in log file
- Added: Extended error checking and logging
- Improved: Compatibility with **Real Time** dynamic sunset/sunrise times (`NightTime`/`DayTime`)!
- Improved: Compatibility with **Ploppable Asphalt +** building prop render distance
- Improved: Compatibility with **Ultimate Level Of Detail** mod (ULOD)
- Improved: Compatibility with **Adaptive Prop Visibility Distance** mod
- Improved: Eradicated grindingly slow CPU load in `Update()` method
- Improved: Hugely optimised token detection performance
- Improved: Hugely optimised asset scanner (3-4x faster than original mod)
- Improved: Optimised logging memalloc/gc footprint during startup
- Improved: Extension method makes asset report 3x faster (thanks dymanoid!)
- Improved: Batch shader assets report in to single `Debug.Log()` call
- Improved: Accurate detection of always-on shaders
- Improved: Always-on shaders removed from `Update()` loop
- Improved: Cache OverlapsMidnight check per shader
- Improved: Use `enum` for faster/better asset type switching
- Improved: Asset settings only set once, on `Start()`, not per frame.
- Improved: Disable `Update()` if no dynamic shaders found
- Fixed: Code not shutting down when level/game unloads (slowed game exit)
- Fixed: Render distances overwritten when graphics settings changed
- Fixed: Render distances would sometimes be lower than what asset defines
- Fixed: Building prop render distance conflict with Ploppable Asphalt+ mod
- Fixed: Superfluous batch applications of asset settings
- Fixed: Invalid `null` checking on `UnityEngine.Object`-derived classes
- Fixed: Render distances not working properly on game load
- Fixed: `RenderGroups` not updated when render distances change
- Updated: Heavy code refactoring; split in to separate files
- Updated: Changed `.csproj` to SDK format
- Updated: Split managed dll and publish folder locations in to `.props` file
- Updated: Tidy up internal data properly on level unloading
- Updated: Assets are returned to their original state on level unloading
- Removed: Redundant legacy code
- Meta: New scripts for asset creators (see source repository link below)
- Meta: New source repository https://github.com/CitiesSkylinesMods/AdditiveShader

### Version 1.4.0, 8 Dec 2018

- No change log provided
- Meta: Source on gist https://gist.github.com/ronyx69/41d2368485b4eea89958c368fab878b8/revisions

### Version 1.3.0, 30 Jul 2018

- Sub-building meshes code removed
- Only set building max prop distance if it contains shader-using props
- Meta: Source on gist https://gist.github.com/ronyx69/41d2368485b4eea89958c368fab878b8/revisions

### Version 1.2.0, 27 Jul, 2018

- Building max prop distance added (allows shader props in buildings)
- Meta: Source on gist https://gist.github.com/ronyx69/41d2368485b4eea89958c368fab878b8/revisions

### Version 1.1.0, 12 Jun 2018

- Initial release to workshop
- Meta: Source on gist https://gist.github.com/ronyx69/41d2368485b4eea89958c368fab878b8/revisions
