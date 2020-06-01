## Overview

> NOTE: DRAFT DOCUMENTATION FOR FUTURE FEATURE (NOT RELEASED YET)

Traditionally, Additive Shader mod has only allowed shaders to be toggled based
on game time: On or Off at certain times of day.

From v2.0 onwards, shader visibility _is moddable_ based on _tags_ defined
in the `m_mesh.name` of the asset.

## Asset settings

> See ASSETS guide if you haven't already done so.

The shader `m_mesh.name` needs to use this format:

```
AdditiveShader RemoteControl Fade Intensity tag1 tag2 ... tagN
```

The `RemoteControl` **Keyword** tells Additive Shader mod that something external
will be controlling visibility of the asset. The shader will be hidden by default
and Additive Shader mod won't change the visibility unless instructed to do so by
an external mod.

One or more **tags** allow the external mod to locate the asset and control its
visibility.

## Modding

External mods instruct the Additive Shader mod to filter `RemoteControl` shaders
in to groups based on their _tags_.

For example, a mod which toggles shaders based when it's raining might use the
`on-during-rain` tag. Any shaders with that tag will be added to a group and then
the mod can toggle visibility of the whole group when it starts/stops raining.

If you want to make a shader compatible with an existing mod, just add the tags.

_If you want to make your own mod, keep reading..._

### NuGet package

First you'll need to add the AdditiveShaderAPI NuGet package to your mod.

TODO: no freaking clue yet

### Create a manager

You'll need a `MonoBehaviour` to create and manage shader groups - here's an
example of a manager for toggling shaders when it rains:

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
    private bool rainStageChanged;

    [UsedImplicitly]
    protected void Start()
    {
        // get unique id for group
        rainGroup = Guid.NewGuid();

        // initialise api
        api = new AdditiveShaderAPI();

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
            api.SetGroupVisibility(rainGroup, rainState);
    }

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

In your `LoadingExtension`, create a game object and connect the manager...

```cs
public class Loading : LoadingExtensionBase
{
    private GameObject gameObject;

    [UsedImplicitly]
    public override void OnLevelLoaded(LoadMode mode)
    {
        base.OnLevelLoaded(mode);

        if (IsApplicable(mode) && AdditiveShaderAPI.isAvailable) {

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
        mode == LoadMode.LoadGame;
}
```
