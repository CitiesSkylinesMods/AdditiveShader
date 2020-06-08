# Asset Settings Guide

This guide explains the asset settings in detail.

The settings are applied to an asset using [Mod Tools Scripts](./SCRIPTS.md).

The **Settings block** in a script lets you specify:

* `keyword` -- an alternate way of specifying on/off times
* `timeOn` -- game time when shader becomes visible
* `timeOff` -- game time when it's turned off
* `fade` -- controls amount of shader fading near other objects
* `intensity` -- controls the light intensity of the shader
* `tags` (optional) -- one or more tags/labels

Vehicle and sub-building shaders have an extra setting:

* `submesh` - the submesh index (starts at `0` for buildings, `1` for vehicles)

## On and Off times

Use a `keyword` to use a pre-defiend on/off time:

* `"AlwaysOn"` -- always on, never goes off
* `"DayTime"` -- on during day (off at night)
* `"NightTime"` -- on at night (off during day)
* `""` -- use custom on/off times

For example:

```cs
var keyword = "DayTime";
```

Keywords allow Additive Shader to handle dynamic sunrise/sunset times,
making them compatible with mods such as [Real Time](https://steamcommunity.com/sharedfiles/filedetails/?id=1420955187).

Older shaders will be treated as `NightTime` if:

* Their `timeOn` is between `19.0f` and `21.0f`, (7-9 PM), _and_
* Their `timeOff` is between `4.0f` and `6.0f` (4-6 AM)

> If you want to prevent that happening, add a `not-twilight` tag.

To make a shader 'always on', use _one_ of the following:

* Set `keyword` to `"AlwaysOn"` (recommended), _or_
* Set `timeOn` and `timeoff` to same time, _or_
* Set `timeOn` to `0f` and `timeOff` to `24f`

To make a shader turn on and off at specific game day times:

* You must set `keyword` to `""`
* Set `timeOn` and `timeOff` to different times
* Valid values are be between `0.0f` and `24.0f`.
* Times are based on 24 hour clock:
    * `2` means 2 AM (02:00), `13` means 1 PM (13:00)
* A value of `0.1f` represents 6 minutes, `0.05f` is 3 minutes:
    * `13.2f` means 1:12 PM (13:12), `13.25f` means 1:15 PM (13:15)
* If `timeOn` is later than `timeOff`, the shader _overlaps midnight_:
   * It will turn on at `timeOn`, and remain on until the following day
   * On that following day, it will eventually turn off at `timeOff`
   * Rinse, wash, repeat.

> If your `timeOn` and `timeOff` times are extremely close together, it's
possible the shader won't appear, because it might reach 'off time' before
it's been turned on!

## Fade and Intensity

Additive shader fades near other objects. The `fade` setting controls that:

* `0.5f` is a strong fade effect
* `1.0f` is default amount
* `10.0f` or more basically disables fade effect

_Yes, it's backwards. Higher value = weaker effect. Meh._

The `intensity` setting sets the light multiplier:

* `1.0f` is default intensity
* Lower is darker, higher is brighter
* Values above `1.0f` may cause a bloom effect

Example:

```cs
var fade = 2.0f;
var intensity = 0.9f;
```

## Tags

Tags are arbitrary labels:

* They are optional, you can have none, one or more
* Each tag must be separated by a space
* They must be lowercase (to avoid conflict with KeyWords)
* Avoid number-only tags (alphanumeric is OK)

For example:

```cs
var tags = "foo bar";
```

The main purpose of tags is for interaction with other mods that can toggle
shaders with matching tags on/off. That will be explained more when version 2.0
is released.

## Diagnosing problems

If your shader is not behaving how you expect, search the game log for
`[AdditiveShader]` to find diagnostic and error reports.

Searching `[AdditiveShader] Assets` will take you to a list of all assets
that use additive shader with some debug info to help identify problems.

Remember that shaders on/off times might be delayed, especially if you have a
shitton of assets that use the shader, or if your game is lagging real bad.
