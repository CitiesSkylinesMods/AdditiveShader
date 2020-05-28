namespace AdditiveShader.Manager
{
    using System;
    using System.Collections.Generic;
    using ColossalFramework;
    using JetBrains.Annotations;
    using UnityEngine;

    // TODO: As this is exposed via a game object, maybe provide some way for
    // other mods to define default on/off time (day/night transition time).
    // Eg. Real Time mod that changes sunrise/sunset.

    /// <summary>
    /// <para>Attached to a game object, this manager will perform the following tasks:
    /// <list type="bullet">
    /// <item><term>Start</term> collate a list of all assets using the additive shader.</item>
    /// <item><term>Update</term> set visibility of the shaders based on their parameters.</item>
    /// </list>
    /// </para>
    /// </summary>
    public class AdditiveShaderManager : MonoBehaviour
    {
        /// <summary>
        /// If a mesh name contains this token, it uses the additive shader.
        /// </summary>
        public const string TOKEN = "AdditiveShader";

        /// <summary>
        /// A list of assets which utilise the additive shader.
        /// </summary>
        public List<ShaderAsset> Shaders;

        /// <summary>
        /// Is set to <c>true</c> once all the assets are initialised.
        /// </summary>
        private bool loaded;

        /// <summary>
        /// Collates list of assets that use the additive shader.
        /// </summary>
        [UsedImplicitly]
        public void Start()
        {
            Shaders = new List<ShaderAsset>();

            Add_Props();
            Add_Buildings();
            Add_SubBuildings();
            Add_Vehicles();

            loaded = true;
        }

        /// <summary>
        /// <para>Sets visibility of the additive shaders depending on game time.</para>
        /// <para>PERFORMANCE CRITICAL.</para>
        /// </summary>
        [UsedImplicitly]
        public void Update()
        {
            if (!loaded)
                return;

            float time = Singleton<SimulationManager>.instance.m_currentDayTimeHour;

            // TODO: Need to spread iteration of the list over frames, but don't know how yet.
            foreach (var shader in Shaders)
            {
                if (shader.Info.AlwaysOn)
                    continue;

                shader.SetVisible(
                    shader.Info.OverlapsMidnight
                        ? time < shader.Info.OffTime || shader.Info.OnTime <= time
                        : shader.Info.OnTime <= time && time < shader.Info.OffTime);
            }
        }

        private bool HasShaderToken(string meshName) =>
            !string.IsNullOrEmpty(meshName) && meshName.Contains(TOKEN);

        /// <summary>
        /// Scans prop assets, adding any using the shader to the list.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        private void Add_Props()
        {
            foreach (var prop in Resources.FindObjectsOfTypeAll<PropInfo>())
                try
                {
                    if (HasShaderToken(prop?.m_mesh?.name))
                        Shaders.Add(new ShaderAsset(prop));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
        }

        /// <summary>
        /// Scans building assets, adding any using the shader to the list.
        ///
        /// Also checks the building props; if any use the shader the building
        /// prop distance is greatly increased to keep them visible longer (that's
        /// required _in addition_ to setting the distance on the props themselves).
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        private void Add_Buildings()
        {
            foreach (var building in Resources.FindObjectsOfTypeAll<BuildingInfo>())
                try
                {
                    if (HasShaderToken(building?.m_mesh?.name))
                        Shaders.Add(new ShaderAsset(building));

                    if (building.m_props == null || building.m_props.Length == 0)
                        continue;

                    for (uint i = 0; i < building.m_props.Length; i++)
                        if (HasShaderToken(building.m_props[i].m_finalProp?.m_mesh?.name))
                        {
                            building.m_maxPropDistance = 25000;
                            break;
                        }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
        }

        /// <summary>
        /// Scans sub building assets, adding any using the shader to the list.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        private void Add_SubBuildings()
        {
            foreach (var subBuilding in Resources.FindObjectsOfTypeAll<BuildingInfoSub>())
                try
                {
                    if (HasShaderToken(subBuilding?.m_mesh?.name))
                        Shaders.Add(new ShaderAsset(subBuilding));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
        }

        /// <summary>
        /// Scans vehicle assets, adding any using the shader to the list.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        private void Add_Vehicles()
        {
            foreach (var vehicle in Resources.FindObjectsOfTypeAll<VehicleInfoSub>())
                try
                {
                    if (HasShaderToken(vehicle?.m_mesh?.name))
                        Shaders.Add(new ShaderAsset(vehicle));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
        }
    }
}
