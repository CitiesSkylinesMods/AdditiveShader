## Overview

> :warning: NOTE: DRAFT DOCUMENTATION FOR FUTURE FEATURE (NOT RELEASED YET)

Traditionally, Additive Shader mod has only allowed shaders to be toggled based
on game time: On or Off at certain times of day.

From v2.0 onwards, shader visibility _is moddable_ based on _tags_ defined
in the `m_mesh.name` of the asset.

## Asset settings

[Mod Tools Scripts](./SCRIPTS.md) allow asset creators to set up the mesh name
easily via variables at start of script.

To expose a shader to external mods, there are two key requirements:

* The `keyword` must be set to `Moddable`, _and_
* The `tags` must be set based on the filtering requirements of the mod

The mod should specify clearly what tags it's looking for.

An asset such as a building or vehicle can have multiple shaders, and the asset
creator can choose for each shader whether it is `Moddable` or time-based. The
Additive Shader mod will deal with the time-based shaders, while external mods
will deal with the `Moddable` shaders.

If the external mod is not subscribed/enabled, any shaders that require it will
not be visible (Additive Shader mod sets them hidden by default).

## Modding

External mods instruct the Additive Shader mod to filter `Moddable` shaders
in to groups, based on their _tags_.

For example, a mod which toggles shaders when it's raining might use the
`on-during-rain` tag. Any shaders with that tag will be added to a group and then
the mod can toggle visibility of the whole group when it starts/stops raining.

A single mod can define one or more groups, and can toggle visibility of each
group independently. The API used by the mod handles communications between the
mod and Additive Shader mod and provides the functionality to create and manage
the groups.

### NuGet package

First you'll need to add the `AdditiveShaderAPI` NuGet package to your mod.

> TODO: it doesn't exist yet

### Create a manager

A `MonoBehaviour` is required, which manages your groups, making them visible
when it rains:

> This is a simplified version of the 'Additive Shader - Weather Extensions' mod.

```cs
public class RainShaders : MonoBehaviour
{
    // the api allows you to communicate with additive shader
    private AdditiveShaderAPI api;

    // group id
    private Guid rainGroup;

    // tracks if groups are created
    private bool initialised;

    private bool rainState; // true when raining
    private bool rainStageChanged; // true if rainState has changed

    [UsedImplicitly]
    protected void Start()
    {
        // get unique id for group
        rainGroup = Guid.NewGuid();

        // initialise api, passing in the name of your mod
        api = new AdditiveShaderAPI("RainShaders");

        // connect to the additive shader mod
        // if it fails, the RainShaders behaviour will be disabled
        enabled = api.Connect();
    }

    [UsedImplicitly]
    protected void Update()
    {
        // on first update, the groups need to be created
        if (!initialised && !CreateGroups())
            return;

        if (rainStateChanged)
        {
            api.SetGroupVisibility(rainGroup, rainState);
            rainStateChanged = false;
        }
    }

    [UsedImplicitly]
    protected void CheckRainState()
    {
        bool currentState =
            Singleton<WeatherManager>.instance.m_currentRain > 0.1;

        if (rainState != currentState)
        {
            rainState = currentState;
            rainStateChanged = true;
        }
    }

    [UsedImplicitly]
    protected void OnDestroy()
    {
        CancelInvoke(); // stop checking the weather

        enabled = false; // stop updating

        api.Disconnect();
        api = null;
    }

    private bool CreateGroups()
    {
        initialised = true;

        // monitor weather changes
        InvokeRepeating(nameof(CheckRainState), 1.0f, 5.0f);

        // if the group contains no shaders, terminate
        if (!api.NewGroup(rainGroup, "on-during-rain"))
            OnDestroy();

        return enabled;
    }
}
```

### Attach manager to game object

The manager on its own won't do anything. We need to attach it to a `GameObject`.

```cs
public class Loading : LoadingExtensionBase
{
    private GameObject gameObject;

    [UsedImplicitly]
    public override void OnLevelLoaded(LoadMode mode)
    {
        base.OnLevelLoaded(mode);

        if (IsApplicable(mode) && AdditiveShaderAPI.IsAvailable) {

            gameObject = new GameObject();
            gameObject.AddComponent<RainShaders>();
        }
    }

    public override void OnLevelUnloading()
    {
        base.OnLevelUnloading();

        if (!gameObject)
            return;

        UnityEngine.Object.Destroy(gameObject);
        gameObject = null;
    }

    private static bool IsApplicable(LoadMode mode) =>
        mode == LoadMode.NewGame ||
        mode == LoadMode.NewGameFromScenario ||
        mode == LoadMode.LoadGame;
}
```
