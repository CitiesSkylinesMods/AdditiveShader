> NOTE: Work in progress, likely to change.

## Asset creation

Additive shader assets are created in much the same way as any other asset, for
more detail, see [CSL Modding: Additive Shader](https://cslmodding.info/mod/additive-shader/).

You'll also need [Mod Tools](https://steamcommunity.com/sharedfiles/filedetails/?id=450877484)
and [these scripts](https://gist.github.com/ronyx69/97a8efae47d6828f01d7d0ab8189fd73).

> The scripts don't currently support Keywords or tags.

## Performance

LODs don't support additive shader. To work around that limitation the mod will
disable LODs and/or increase render distance of anything that uses the shader.

Buildings, in particular, can be a problem. If they contain a one prop that
uses the shader, the max _prop_ render distance _for that building_ will be
increased. That setting applies to all props so it can have an impact on fps.
So if you use a shader prop in a building, try and limit how many other props
that building has (and ideally keep them low tri with small textures).

To reduce CPU workload to basically zero, the Additive Shader mod (from v1.5
onwards) spreads shader visibility updates across frames, so there might be some
delay between on/off time and the shader actually being made visible/hidden in
the game.

## Settings

To make the mod recognise your mesh, the `m_mesh.name` must be in one of the
following formats:

```
AdditiveShader On Off Fade Intensity tags
```

or

```
AdditiveShader Keyword Fade Intensity tags
```

Where:

* **On** -- game time when shader becomes visible
* **Off** -- game time when it's turned off
* **Keyword** -- an alternate way of specifying on/off times
* **Fade** -- controls amount of shader fading near other objects
* **Intensity** -- controls the light intensity of the shader
* **tags** (optional) -- one or more tags/labels

### Examples:

```
AdditiveShader 9 17 20 2
```

That results in:

* **On** = `9 AM` (`09:00`)
* **Off** = `5 PM` (`17:00`)
* **Fade** = `20`
* **Intensity** = `2`

```
AdditiveShader AlwaysOn 20 2 foo bar
```

That will be always on, and is tagged `foo` and `bar`.

### Times

There are three broad categories of visibility control for your shaders:

* **Static** -- The shader is always on, or off
* **Timed** -- The shader turns on and off at specific times
* **Twilight** -- The shader turns on or off at twilight

#### Static shaders

To make a shader 'always on', use _one_ of the following:

* Use **Keyword** `AlwaysOn`
* Set **On** and **Off** to the same value (eg. `24 24`)
* Set **On** to `0` and **Off** to `24`

To make a shader 'always off' (useful for mod controlled shaders):

* Use **Keyword** `AlwaysOff`

Static shaders are updated once, when your city loads. As such they do not put
any load on the CPU.

#### Timed shaders

To make a shader turn on and off at specific game day times:

* Set **On** and **Off** to the desired times
* Valid values are be between `0` and `24`.
* Times are based on 24 hour clock:
    * `2` means 2 AM (02:00), `13` means 1 PM (13:00)
* A value of `0.1` represents 6 minutes:
    * `13.2` means 1:12 PM (13:12), `13.25` means 1:15 PM (13:15)
* If **On** is later than **Off**, the shader _overlaps midnight_:
   * It will turn on at **On** time, and remain on until the following day
   * On that following day, it will eventually turn off, at **Off time**
   * Rinse, wash, repeat.

Time-based shaders are updated every few frames, so the actual on/off time
in-game might be delayed by a few seconds, because reasons.

If your **On** and **Off** times are very close together, it's possible the
shader won't even appear, because it might reach 'off time' before it's been
turned on!

#### Twilight shaders

These are the most common, where the shader turns on at dusk, and off at dawn.

* Use **Keyword** `DayTime` to make the shader visible during daylight hours
* Use **Keyword** `NightTime` to make the shader visible during night hours

If mods, such as Real Time, alter the sunrise/sunset times, twilight shaders
will automatically adapt to the changed times.

Example:

```
AdditiveShader DayTime 3.5 0.9
```

To enable backwards-compatibility with older assets, shaders are also treated
as `NightTime` if:

* Their **On** time is between `19` and `21`, (7-9 PM) _and..._
* Their **Off** time is between `4` and `6` (4-6 AM)

If you want to disable backwards-compatibility, add a `not-twilight` **tag**:

```
AdditiveShader 20 5 3.5 0.9 not-twilight
```

Twilight shaders are processed as a batch, over several frames, so you'll see
them turning on/off in a similar way to street lights. Updates to timed shaders
are paused while the twilight shaders are being processed.

### Fade/Intensity

Additive shader fades near other objects. The **Fade** value controls that:

* `0.5` is a strong fade effect
* `1` is default amount
* `10+` basically disables fade effect

_Yes, it's backwards. Higher value = weaker effect. Meh._

The **Intensity** value sets the light multiplier:

* `1` is default intensity
* Lower is darker, higher is brighter
* Values above `1` may cause a bloom effect

### Tags

Tags are arbitrary keywords that you can add to the end of the mesh name:

* They are optional, you can have none, one or more
* They must be preceded by a space
* They must be lowercase
* Avoid number-only tags (alphanumeric is OK)

The main purpose of tags is for interaction with other mods that can toggle
shaders with matching tags on/off.

## Diagnosing problems

If your shader is not behaving how you expect, search the game log for
`[AdditiveShader]` to find diagnostic and error reports.

Searching `[AdditiveShader] Assets` will take you to a list of all assets
that use additive shader; any errors will usually appear just before that.

Remember that shaders on/off times might be delayed, especially if you have a
shitton of assets that use the shader, or if your game is lagging real bad.
