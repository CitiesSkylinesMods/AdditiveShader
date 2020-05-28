namespace AdditiveShader
{
    using ColossalFramework;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    // TODO: As this is exposed via a game object, maybe provide some way for
    // other mods to define default on/off time (day/night transition time).
    // Eg. Real Time mod that changes sunrise/sunset.

    public class AdditiveShaderManager : MonoBehaviour
    {
        /// <summary>
        /// A list of assets which utilise the additive shader.
        /// </summary>
        public List<AdditiveShader> AdditiveShaders;

        /// <summary>
        /// If a mesh name contains this token, it uses the additive shader.
        ///
        /// Mesh name format is: "AdditiveShader EnableTime DisableTime Fade Intensity".
        /// </summary>
        public const string TOKEN = "AdditiveShader";

        /// <summary>
        /// Set to <c>true</c> once all the assets are initialised.
        /// </summary>
        private bool _loaded = false;

        public void Start()
        {
            AdditiveShaders = new List<AdditiveShader>();

            Add_Props();
            Add_Buildings();
            Add_SubBuildings();
            Add_Vehicles();

            _loaded = true;
        }

        /// <summary>
        /// Sets visibility of the additive shaders depending on game time.
        /// </summary>
        public void Update()
        {
            if (!_loaded)
                return;

            float time = Singleton<SimulationManager>.instance.m_currentDayTimeHour;

            // TODO: Need to spread iteration of the list over frames, but don't know how yet.

            foreach (var shader in AdditiveShaders)
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
        private void Add_Props()
        {
            foreach (var prop in Resources.FindObjectsOfTypeAll<PropInfo>())
                try
                {
                    if (HasShaderToken(prop?.m_mesh?.name))
                        AdditiveShaders.Add(new AdditiveShader(prop));
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
        private void Add_Buildings()
        {
            foreach (var building in Resources.FindObjectsOfTypeAll<BuildingInfo>())
                try
                {
                    if (HasShaderToken(building?.m_mesh?.name))
                        AdditiveShaders.Add(new AdditiveShader(building));

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
        private void Add_SubBuildings()
        {
            foreach (var subBuilding in Resources.FindObjectsOfTypeAll<BuildingInfoSub>())
                try
                {
                    if (HasShaderToken(subBuilding?.m_mesh?.name))
                        AdditiveShaders.Add(new AdditiveShader(subBuilding));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
        }

        /// <summary>
        /// Scans vehicle assets, adding any using the shader to the list.
        /// </summary>
        private void Add_Vehicles()
        {
            foreach (var vehicle in Resources.FindObjectsOfTypeAll<VehicleInfoSub>())
                try
                {
                    if (HasShaderToken(vehicle?.m_mesh?.name))
                        AdditiveShaders.Add(new AdditiveShader(vehicle));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
        }
    }
}
