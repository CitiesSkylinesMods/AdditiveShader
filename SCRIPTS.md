# Mod Tools Scripts

These scripts are for asset creators to use in [Mod Tools](https://steamcommunity.com/sharedfiles/filedetails/?id=450877484)
console (`F7`) to configure assets to use the Additive Shader.

> **Note for C# developers:**
> Mod Tools script console uses the inbuilt Mono compiler that comes with the
game, which uses [C# language version 3.0](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-30).
It does not support `is` operator, string interpolation, etc. In addition,
members such as `m_material` are derived from `UnityEngine.Object` which silently
casts `null` to boolean `false`. Hence the somewhat cumbersome code in the scripts.

There are currently four types of asset that can have the shader:

* Props
* Buildings
* Building submeshes
* Vehicle submeshes

Make sure you use the right script for your asset!

For a guide on creating the actual assets, see [CSL Modding: Additive Shader](https://cslmodding.info/mod/additive-shader/).

## Settings

Each script contains a **settings** block and a **code** block.

Edit the **settings**, do _not_ edit the **code**.

If a `keyword` is specified, the `timeOn` and `timeOff` will be ignored. To use
custom on/off times, set `keyword` value to `""`. For example:

```cs
var keyword = "NightTime"; // this will be used
var timeOn = 22.0f; // ignored
var timeOff = 6.0f; // ignored

// or ...

var keyword = ""; // ignored
var timeOn = 22.0f; // this will be used
var timeOff = 6.0f; // this will be used
```

> Important: **KeyWords** are case sensitive!

For more information on settings, see [Asset Settings Guide](./SETTINGS.md).

Once you've defined your settings, paste the whole thing (settings + code)
in to the Mod Tools script console.

## Props

Props are extremely versatile - they can be included in other assets, and users
can plop them manually anywhere they want.

> Numbers with `f` after them = floating point.

```cs
// Settings:
var keyWord = "NightTime"; // or "DayTime" or "AlwaysOn" (or "" for custom times)
var timeOn = 22.0f; // 24 HR clock
var timeOff = 6.0f; // 24 HR clock
var fade = 20.0f; // 0.5 a lot, 1 default, 10+ basically none
var intensity = 1.0f; // below 1 darker, 1 default, 1+ may bloom
var tags = ""; // optional: add one or more tags, separated by spaces

// Code (do not change):
var shader = Shader.Find("Custom/Particles/Additive (Soft)");
var asset = ToolsModifierControl.toolController.m_editPrefabInfo as PropInfo;
if ((object)asset.m_material != null) {
    asset.m_material.shader = shader;
    asset.m_material.SetFloat("_Intensity", intensity);
    asset.m_material.SetFloat("_InvFade", fade);
}
if (string.IsNullOrEmpty(keyWord)) {
    asset.m_mesh.name = "AdditiveShader "+timeOn.ToString("R")+" "+timeOff.ToString("R")+" "+fade.ToString("R")+" "+intensity.ToString("R")+" "+tags;
} else {
    asset.m_mesh.name = "AdditiveShader "+keyWord+" "+fade.ToString("R")+" "+intensity.ToString("R")+" "+tags;
}
asset.m_lodMesh = null;
asset.m_lodMaterial = null;
asset.m_lodMaterialCombined = null;
asset.m_lodObject = null;
// End of script
```

## Buildings

This applies a shader directly to the building.

```cs
// Settings:
var keyWord = "NightTime"; // or "DayTime" or "AlwaysOn" (or "" for custom times)
var timeOn = 22.0f; // 24 HR clock
var timeOff = 6.0f; // 24 HR clock
var fade = 20.0f; // 0.5 a lot, 1 default, 10+ basically none
var intensity = 1.0f; // below 1 darker, 1 default, 1+ may bloom
var tags = ""; // optional: add one or more tags, separated by spaces

// Code (do not edit):
var shader = Shader.Find("Custom/Particles/Additive (Soft)");
var asset = ToolsModifierControl.toolController.m_editPrefabInfo as BuildingInfo;
if ((object)asset.m_material != null) {
    asset.m_material.shader = shader;
    asset.m_material.SetFloat("_Intensity", intensity);
    asset.m_material.SetFloat("_InvFade", fade);
}
if (string.IsNullOrEmpty(keyWord)) {
    asset.m_mesh.name = "AdditiveShader "+timeOn.ToString("R")+" "+timeOff.ToString("R")+" "+fade.ToString("R")+" "+intensity.ToString("R")+" "+tags;
} else {
    asset.m_mesh.name = "AdditiveShader "+keyWord+" "+fade.ToString("R")+" "+intensity.ToString("R")+" "+tags;
}
asset.m_lodMesh = null;
asset.m_lodMaterial = null;
asset.m_lodMaterialCombined = null;
asset.m_lodObject = null;
// End of script
```

## Building submeshes

This applies a shader to a building submesh. This can be useful if you want
multiple shaders for a single building, each with its own settings.
Alternatively you could use props.

```cs
// Settings:
var submesh = 0; // sub mesh id, order as in the UI, starting from _0_

var keyWord = "NightTime"; // or "DayTime" or "AlwaysOn" (or "" for custom times)
var timeOn = 22.0f; // 24 HR clock
var timeOff = 6.0f; // 24 HR clock
var fade = 20.0f; // 0.5 a lot, 1 default, 10+ basically none
var intensity = 1.0f; // below 1 darker, 1 default, 1+ may bloom
var tags = ""; // optional: add one or more tags, separated by spaces

// Code (do not edit):
var shader = Shader.Find("Custom/Particles/Additive (Soft)");
var asset = ToolsModifierControl.toolController.m_editPrefabInfo as BuildingInfo;
var mesh = asset.m_subMeshes[submesh];
if ((object)mesh.m_subInfo.m_material != null) {
    mesh.m_subInfo.m_material.shader = shader;
    mesh.m_subInfo.m_material.SetFloat("_Intensity", intensity);
    mesh.m_subInfo.m_material.SetFloat("_InvFade", fade);
}
if (string.IsNullOrEmpty(keyWord)) {
    mesh.m_subInfo.m_mesh.name = "AdditiveShader "+timeOn.ToString("R")+" "+timeOff.ToString("R")+" "+fade.ToString("R")+" "+intensity.ToString("R")+" "+tags;
} else {
    mesh.m_subInfo.m_mesh.name = "AdditiveShader "+keyWord+" "+fade.ToString("R")+" "+intensity.ToString("R")+" "+tags;
}
int vertexCount = mesh.m_subInfo.m_mesh.vertexCount;
var colors = new Color[vertexCount];
for (int i = 0; i < vertexCount; i++) colors[i] = Color.white;
mesh.m_subInfo.m_mesh.colors = colors;
mesh.m_subInfo.m_lodMesh = null;
mesh.m_subInfo.m_lodMaterial = null;
mesh.m_subInfo.m_lodMaterialCombined = null;
mesh.m_subInfo.m_lodObject = null;
// End of script
```

## Vehicle submeshes

This applies a shader to a vehicle submesh. Since v1.5.0, it _should_ be possible
to apply blinking effects.

```cs
// Settings:
var submesh = 1; // sub mesh id, order as in the UI, starting from _1_

var keyWord = "NightTime"; // or "DayTime" or "AlwaysOn" (or "" for custom times)
var timeOn = 22.0f; // 24 HR clock
var timeOff = 6.0f; // 24 HR clock
var fade = 20.0f; // 0.5 a lot, 1 default, 10+ basically none
var intensity = 1.0f; // below 1 darker, 1 default, 1+ may bloom
var tags = ""; // optional: add one or more tags, separated by spaces

// Code (do not edit):
var shader = Shader.Find("Custom/Particles/Additive (Soft)");
var asset = ToolsModifierControl.toolController.m_editPrefabInfo as VehicleInfo;
var mesh = asset.m_subMeshes[submesh];
if ((object)mesh.m_subInfo.m_material != null) {
    mesh.m_subInfo.m_material.shader = shader;
    mesh.m_subInfo.m_material.SetFloat("_Intensity", intensity);
    mesh.m_subInfo.m_material.SetFloat("_InvFade", fade);
}
if (string.IsNullOrEmpty(keyWord)) {
    mesh.m_subInfo.m_mesh.name = "AdditiveShader "+timeOn.ToString("R")+" "+timeOff.ToString("R")+" "+fade.ToString("R")+" "+intensity.ToString("R")+" "+tags;
} else {
    mesh.m_subInfo.m_mesh.name = "AdditiveShader "+keyWord+" "+fade.ToString("R")+" "+intensity.ToString("R")+" "+tags;
}
int vertexCount = mesh.m_subInfo.m_mesh.vertexCount;
var colors = new Color[vertexCount];
for (int i = 0; i < vertexCount; i++) colors[i] = Color.white;
mesh.m_subInfo.m_mesh.colors = colors;
mesh.m_subInfo.m_lodMesh = null;
mesh.m_subInfo.m_lodMaterial = null;
mesh.m_subInfo.m_lodMaterialCombined = null;
mesh.m_subInfo.m_lodObject = null;
// End of script
```
