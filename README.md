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
