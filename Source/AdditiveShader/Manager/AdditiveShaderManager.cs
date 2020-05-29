namespace AdditiveShader.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
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
        /// A list of shader assets that are static.
        /// </summary>
        private List<ShaderAsset> staticShaders;

        /// <summary>
        /// A list of shader assets that are dynamic (time-based).
        /// </summary>
        private List<ShaderAsset> dynamicShaders;

        private bool flipflop;

        private int index;

        private int count;

        /// <summary>
        /// Collates list of assets that use the additive shader.
        /// </summary>
        [UsedImplicitly]
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        public void Start()
        {
            var report = new StringBuilder(2048);

            report
                .AppendLine("[AdditiveShader] Assets using shader:")
                .AppendLine();

            var tempList = new List<ShaderAsset>();

            staticShaders = new List<ShaderAsset>();
            dynamicShaders = new List<ShaderAsset>();

            Add_Props(tempList);
            Add_Buildings(tempList);
            Add_SubBuildings(tempList);
            Add_Vehicles(tempList);

            foreach (var shader in tempList)
            {
                report
                    .Append(shader).AppendLine(":")
                    .Append(" - ").Append(shader.Info)
                    .AppendLine()
                    .Append(" - AlwaysOn: ").Append(shader.Info.AlwaysOn)
                    .Append(", Static: ").Append(shader.Info.Static)
                    .Append(", OverlapsMidnight: ").Append(shader.Info.OverlapsMidnight)
                    .AppendLine();

                (shader.Info.Static ? staticShaders : dynamicShaders).Add(shader);
            }

            report
                .AppendLine()
                .Append("Found ").Append(tempList.Count).Append(" assets");

            Debug.Log(report.ToString());

            staticShaders.TrimExcess();
            dynamicShaders.TrimExcess();

            index = -1;
            count = dynamicShaders.Count;

            // if dynamicShaders list is empty, disable Update() completely
            enabled = count > 0;
        }

        /// <summary>
        /// <para>Sets visibility of the additive shaders depending on game time.</para>
        /// <para>PERFORMANCE CRITICAL.</para>
        /// </summary>
        [UsedImplicitly]
        public void Update()
        {
            if (flipflop = !flipflop)
                return;

            if (++index >= count)
                index = 0;

            var shader = dynamicShaders[index];

            float time = Singleton<SimulationManager>.instance.m_currentDayTimeHour;

            shader.SetVisible(
                shader.Info.OverlapsMidnight
                    ? time < shader.Info.OffTime || shader.Info.OnTime <= time
                    : shader.Info.OnTime <= time && time < shader.Info.OffTime);
        }

        /// <summary>
        /// Check if a mesh name contains the additive shader token.
        /// </summary>
        /// <param name="meshName">The <c>m_mesh.name</c> to investigate.</param>
        /// <returns>Returns <c>true</c> if the token is found, otherwise <c>false</c>.</returns>
        private static bool HasShaderToken(string meshName) =>
            !string.IsNullOrEmpty(meshName) && meshName.Contains(TOKEN);

        /// <summary>
        /// Scans prop assets, adding any using the shader to the list.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        private static void Add_Props(List<ShaderAsset> list)
        {
            foreach (var prop in Resources.FindObjectsOfTypeAll<PropInfo>())
                try
                {
                    if (HasShaderToken(prop?.m_mesh?.name))
                        list.Add(new ShaderAsset(prop));
                }
                catch (Exception e)
                {
                    Debug.LogError($"[AdditiveShader] PropInfo error: {prop?.name}\n{e}");
                }
        }

        /// <summary>
        /// <para>Scans building assets, adding any using the shader to the list.</para>
        /// <para>
        /// Also checks the building props; if any use the shader the building
        /// prop distance is greatly increased to keep them visible longer (that's
        /// required _in addition_ to setting the distance on the props themselves).
        /// </para>
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        private static void Add_Buildings(List<ShaderAsset> list)
        {
            foreach (var building in Resources.FindObjectsOfTypeAll<BuildingInfo>())
                try
                {
                    if (HasShaderToken(building?.m_mesh?.name))
                        list.Add(new ShaderAsset(building));

                    if (building?.m_props == null || building.m_props.Length == 0)
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
                    Debug.LogError($"[AdditiveShader] BuildingInfo error: {building?.name}\n{e}");
                }
        }

        /// <summary>
        /// Scans sub building assets, adding any using the shader to the list.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        private static void Add_SubBuildings(List<ShaderAsset> list)
        {
            foreach (var subBuilding in Resources.FindObjectsOfTypeAll<BuildingInfoSub>())
                try
                {
                    if (HasShaderToken(subBuilding?.m_mesh?.name))
                        list.Add(new ShaderAsset(subBuilding));
                }
                catch (Exception e)
                {
                    Debug.LogError($"[AdditiveShader] BuildingInfoSub error: {subBuilding?.name}\n{e}");
                }
        }

        /// <summary>
        /// Scans vehicle assets, adding any using the shader to the list.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        private static void Add_Vehicles(List<ShaderAsset> list)
        {
            foreach (var vehicle in Resources.FindObjectsOfTypeAll<VehicleInfoSub>())
                try
                {
                    if (HasShaderToken(vehicle?.m_mesh?.name))
                        list.Add(new ShaderAsset(vehicle));
                }
                catch (Exception e)
                {
                    Debug.LogError($"[AdditiveShader] VehicleInfoSub error: {vehicle?.name}\n{e}");
                }
        }
    }
}
