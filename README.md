# AdditiveShader

Source code for [Additive Shader mod](https://steamcommunity.com/sharedfiles/filedetails/?id=1410003347) developed by **Ronyx69** and **Simon Ryr**, moved to this repository with permission from Ronyx69.

## Asset creators

To make the mod recognise your mesh as using the additive shader, the `m_mesh.name` value must be in a specific format as follows:

```
AdditiveShader On Off Fade Intensity
```

Where:

* **On** -- game time when shader becomes visible
* **Off** -- game time when it's turned off
* **Fade** -- controls amount of shader fading near other objects
* **Intensity** -- controls the light intensity of the shader

Use [these scripts](https://gist.github.com/ronyx69/97a8efae47d6828f01d7d0ab8189fd73) in [Mod Tools](https://steamcommunity.com/sharedfiles/filedetails/?id=450877484) console (`F7`) to apply valid names to your meshes.

### Example:

```
AdditiveShader 21 6 20 2
```

That results in:

* On at `9 PM` (`21:00`)
* Off at `6 AM` (`06:00`) _the following day_ (because On is after Off)
* Fade of `20`
* Light intensity of `2`

See [CSL Modding - Additive Shader](https://cslmodding.info/mod/additive-shader/) for details of how to create the assets.

### On/Off times

* Times are based on 24 hour clock, so `2` would be 2 AM (02:00), `13` would be 1 PM (13:00), etc.
* A value of `0.1` is 6 minutes. So `13.2` would be 1:12 PM (13:12), `13.25` would be 1:15 PM (13:15), etc.
* If **On** is later than **Off**, the shader will remain visible across the midnight boundary
    * It will turn off, _the next day_, at **Off** time, and then later that day turn on again, at **On** time
* If **On** is same as **Off**, the shader will be "always on" (always shown)
* Likewise, if **On** is `0` _and_ **Off** is `24`, the shader will be "always on".
* If **On** is negative (eg. `-1`), the shader will be "always off" (that's for a planned future feature)

### Fade/Intensity values

Additive shader fades near other objects. The **Fade** value controlls that effect:

* `0.5` is a strong fade effect
* `1` is default amount
* `10+` basically disables fade effect

The **Intensity** value sets the light multiplier:

* `1` is default intensity
* Lower is darker, higher is brighter.
* Values above `1` may cause a bloom effect

### Diagnosing problems

If you run in to problems, you can check what values the Additive Shader mod sees by looking at the game log file.

Search for `[AdditiveShader] Assets` and you'll see something like this:

```diff
ShaderAsset(BuildingInfoSub lacantine-add):
- ShaderInfo('AdditiveShader 7 1 20 1.5')
- AlwaysOn: False, Static: False, OverlapsMidnight: True
ShaderAsset(VehicleInfoSub 345-reverse-1):
- ShaderInfo('AdditiveShader 0 24 20 3')
- AlwaysOn: True, Static: True, OverlapsMidnight: False
```

`ShaderAsset()` shows the asset type and name. `ShaderInfo()` shows the mesh name. There's also some derived values, `AlwaysOn`, `Static` (does not change with time), and `OverlapsMidnight`.

## Developers

First, clone the repository locally using a [GitHUb client](https://github.com/CitiesSkylinesMods/TMPE/wiki/GitHub-Clients).

You'll need an IDE (see [this list](https://github.com/CitiesSkylinesMods/TMPE/wiki/Dev-Tools)) to compile the code in to a dll file. Open the **AdditiveShader.sln** solution file in the IDE and then build.

The folders for managed dlls and publish dir are defined in [Directory.Build.props](https://github.com/CitiesSkylinesMods/AdditiveShader/blob/master/Source/Directory.Build.props) - you might need to change them depending on where you installed the game.
