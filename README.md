# AdditiveShader

Source code for [Additive Shader mod](https://steamcommunity.com/sharedfiles/filedetails/?id=1410003347) developed by **Ronyx69** and **Simon Ryr**, moved to this repository with permission from Ronyx69.

See also:

* [CSL Modding guide](https://cslmodding.info/mod/additive-shader/)
* [Asset settings guide](./ASSETS.md)
* [Scripts](https://gist.github.com/ronyx69/97a8efae47d6828f01d7d0ab8189fd73) for [Mod Tools](https://steamcommunity.com/sharedfiles/filedetails/?id=450877484) console (`F7`)

## Contributing

Code improvements always welcome!

* Clone the repository locally using a [GitHUb client](https://github.com/CitiesSkylinesMods/TMPE/wiki/GitHub-Clients).
* You'll need an IDE (see [this list](https://github.com/CitiesSkylinesMods/TMPE/wiki/Dev-Tools)) to compile the code in to a dll file.
* Set folders for managed dlls and publish dir are defined in [Directory.Build.props](https://github.com/CitiesSkylinesMods/AdditiveShader/blob/master/Source/Directory.Build.props) - you might need to change them depending on where you installed the game.
    * Please do NOT push changes to this file back to the repo.
* Open the **AdditiveShader.sln** solution file in the IDE and then build.
