# Mod Tools Scripts

These scripts are for asset creators to use in [Mod Tools](https://steamcommunity.com/sharedfiles/filedetails/?id=450877484)
console (`F7`) to configure assets to use the Additive Shader.

There are currently four types of asset that can have the shader:

* Props
* Buildings
* Sub-buildings
* Vehicles

Make sure you use the right script for your asset!

For a guide on creating the actual assets, see [CSL Modding: Additive Shader](https://cslmodding.info/mod/additive-shader/).

## Settings

Each script contains a **settings** block and a **code** block. Edit the settings,
do _not_ edit the **code**.

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

## Props script

> Note: In C# programming language, numbers with `f` after them = floating point.

```cs
// Settings:
var keyword = "NightTime"; // or "DayTime" or "AlwaysOn" (or "" for custom times)
var timeOn = 22.0f; // 24 HR clock
var timeOff = 6.0f; // 24 HR clock
var fade = 20.0f; // 0.5 a lot, 1 default, 10+ basically none
var intensity = 1.0f; // below 1 darker, 1 default, 1+ may bloom
var tags = ""; // optional: add one or more tags, separated by spaces

// Code (do not change):
var shader = Shader.Find("Custom/Particles/Additive (Soft)");
var asset = ToolsModifierControl.toolController.m_editPrefabInfo as PropInfo;
if (asset.m_material is null) asset.m_material.shader = shader;
asset.m_material.SetFloat("_Intensity", intensity);
asset.m_material.SetFloat("_InvFade", fade);
var shaderSettings = string.IsNullOrEmpty(keyword)
    ? $"AdditiveShader {timeOn.ToString("R")} {timeOff.ToString("R")} {fade.ToString("R")} {intensity.ToString("R")} {tags}"
    : $"AdditiveShader {keyword} {fade.ToString("R")} {intensity.ToString("R")} {tags}";
asset.m_mesh.name = shaderSettings;
asset.m_lodMesh = null;
asset.m_lodMaterial = null;
asset.m_lodMaterialCombined = null;
asset.m_lodObject = null;
// End of script
```

## Buildings script

> Additive Shader is sometimes unreliable with buildings. If you can't get it
working, try using a prop instead, and then add that prop to your building.

```cs
// Settings:
var keyword = "NightTime"; // or "DayTime" or "AlwaysOn" (or "" for custom times)
var timeOn = 22.0f; // 24 HR clock
var timeOff = 6.0f; // 24 HR clock
var fade = 20.0f; // 0.5 a lot, 1 default, 10+ basically none
var intensity = 1.0f; // below 1 darker, 1 default, 1+ may bloom
var tags = ""; // optional: add one or more tags, separated by spaces

// Code (do not edit):
var shader = Shader.Find("Custom/Particles/Additive (Soft)");
var asset = ToolsModifierControl.toolController.m_editPrefabInfo as BuildingInfo;
if(asset.m_material != null) asset.m_material.shader = shader;
asset.m_material.SetFloat("_Intensity", intensity);
asset.m_material.SetFloat("_InvFade", fade);
var shaderSettings = string.IsNullOrEmpty(keyword)
    ? $"AdditiveShader {timeOn.ToString("R")} {timeOff.ToString("R")} {fade.ToString("R")} {intensity.ToString("R")} {tags}"
    : $"AdditiveShader {keyword} {fade.ToString("R")} {intensity.ToString("R")} {tags}";
asset.m_mesh.name = shaderSettings;
asset.m_lodMesh = null;
asset.m_lodMaterial = null;
asset.m_lodMaterialCombined = null;
asset.m_lodObject = null;
// End of script
```

## Sub-buildings script

> These can also be unreliable; if it doesn't work, use props instead.

```cs
// Settings:
var submesh = 0; // sub mesh id, order as in the UI, starting from _0_

var keyword = "NightTime"; // or "DayTime" or "AlwaysOn" (or "" for custom times)
var timeOn = 22.0f; // 24 HR clock
var timeOff = 6.0f; // 24 HR clock
var fade = 20.0f; // 0.5 a lot, 1 default, 10+ basically none
var intensity = 1.0f; // below 1 darker, 1 default, 1+ may bloom
var tags = ""; // optional: add one or more tags, separated by spaces

// Code (do not edit):
var shader = Shader.Find("Custom/Particles/Additive (Soft)");
var asset = ToolsModifierControl.toolController.m_editPrefabInfo as BuildingInfo;
var mesh = asset.m_subMeshes[submesh];
if (mesh.m_subInfo.m_material != null)
    mesh.m_subInfo.m_material.shader = shader;
mesh.m_subInfo.m_material.SetFloat("_Intensity", intensity);
mesh.m_subInfo.m_material.SetFloat("_InvFade", fade);
var shaderSettings = string.IsNullOrEmpty(keyword)
    ? $"AdditiveShader {timeOn.ToString("R")} {timeOff.ToString("R")} {fade.ToString("R")} {intensity.ToString("R")} {tags}"
    : $"AdditiveShader {keyword} {fade.ToString("R")} {intensity.ToString("R")} {tags}";
mesh.m_subInfo.m_mesh.name = shaderSettings;
Vector3[] vertices = mesh.m_subInfo.m_mesh.vertices;
Color[] colors = new Color[vertices.Length];
for (int i = 0; i < vertices.Length; i++) colors[i] = Color.white;
mesh.m_subInfo.m_mesh.colors = colors;
mesh.m_subInfo.m_lodMesh = null;
mesh.m_subInfo.m_lodMaterial = null;
mesh.m_subInfo.m_lodMaterialCombined = null;
mesh.m_subInfo.m_lodObject = null;
// End of script
```

## Vehicles script

```cs
// Settings:
var submesh = 1; // sub mesh id, order as in the UI, starting from _1_

var keyword = "NightTime"; // or "DayTime" or "AlwaysOn" (or "" for custom times)
var timeOn = 22.0f; // 24 HR clock
var timeOff = 6.0f; // 24 HR clock
var fade = 20.0f; // 0.5 a lot, 1 default, 10+ basically none
var intensity = 1.0f; // below 1 darker, 1 default, 1+ may bloom
var tags = ""; // optional: add one or more tags, separated by spaces

// Code (do not edit):
var shader = Shader.Find("Custom/Particles/Additive (Soft)");
var asset = ToolsModifierControl.toolController.m_editPrefabInfo as VehicleInfo;
var mesh = asset.m_subMeshes[submesh];
if (mesh.m_subInfo.m_material != null) mesh.m_subInfo.m_material.shader = shader;
mesh.m_subInfo.m_material.SetFloat("_Intensity", intensity);
mesh.m_subInfo.m_material.SetFloat("_InvFade", fade);
var shaderSettings = string.IsNullOrEmpty(keyword)
    ? $"AdditiveShader {timeOn.ToString("R")} {timeOff.ToString("R")} {fade.ToString("R")} {intensity.ToString("R")} {tags}"
    : $"AdditiveShader {keyword} {fade.ToString("R")} {intensity.ToString("R")} {tags}";
mesh.m_subInfo.m_mesh.name = shaderSettings;
Vector3[] vertices = mesh.m_subInfo.m_mesh.vertices;
Color[] colors = new Color[vertices.Length];
for (int i = 0; i < vertices.Length; i++) colors[i] = Color.white;
mesh.m_subInfo.m_mesh.colors = colors;
mesh.m_subInfo.m_lodMesh = null;
mesh.m_subInfo.m_lodMaterial = null;
mesh.m_subInfo.m_lodMaterialCombined = null;
mesh.m_subInfo.m_lodObject = null;
// End of script
```
