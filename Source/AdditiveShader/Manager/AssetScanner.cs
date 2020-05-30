namespace AdditiveShader.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using UnityEngine;

    /// <summary>
    /// The <see cref="AssetScanner"/> compiles a list of assets which use the additive shader.
    /// </summary>
    [SuppressMessage("Class Design", "AV1008:Class should not be static")]
    internal static class AssetScanner
    {
        /// <summary>
        /// If a mesh name contains this token, it uses the additive shader.
        /// </summary>
        private const string TOKEN = "AdditiveShader";

        /// <summary>
        /// Scans supported asset types, converting any that use the shader to
        /// <see cref="ShaderAsset"/> instances which are collated in the
        /// returned list.
        /// </summary>
        /// <returns>Returns a list of <see cref="ShaderAsset"/> instances.</returns>
        internal static ICollection<ShaderAsset> ListShaderAssets()
        {
            var assetList = new List<ShaderAsset>();

            Add_Props(assetList);
            Add_Buildings(assetList);
            Add_SubBuildings(assetList);
            Add_Vehicles(assetList);

            return assetList;
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
        private static void Add_Props(List<ShaderAsset> assetList)
        {
            foreach (var prop in Resources.FindObjectsOfTypeAll<PropInfo>())
                try
                {
                    if (HasShaderToken(prop?.m_mesh?.name))
                        assetList.Add(new ShaderAsset(prop));
                }
                catch (Exception error)
                {
                    Debug.LogError($"[AdditiveShader] PropInfo error: {prop?.name}\n{error}");
                }
        }

        /// <summary>
        /// Scans building assets, adding any using the shader to the list.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        private static void Add_Buildings(List<ShaderAsset> assetList)
        {
            foreach (var building in Resources.FindObjectsOfTypeAll<BuildingInfo>())
                try
                {
                    if (HasShaderToken(building?.m_mesh?.name))
                        assetList.Add(new ShaderAsset(building));

                    if (building?.m_props == null || building.m_props.Length == 0)
                        continue;

                    CheckBuildingForShaderProps(building);
                }
                catch (Exception error)
                {
                    Debug.LogError($"[AdditiveShader] BuildingInfo error: {building?.name} \n{error}");
                }
        }

        /// <summary>
        /// Because LODs don't support additive shader, if there are any props in the building that use
        /// it we have to increase the <c>m_maxPropDistance</c> for the whole building, _in addition_ to
        /// the props themselves being updated (<see cref="Add_Props(List{ShaderAsset})"/>).
        /// </summary>
        /// <param name="building">The <see cref="BuildingInfo"/> asset to scan.</param>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        private static void CheckBuildingForShaderProps(BuildingInfo building)
        {
            for (uint index = 0; index < building.m_props.Length; index++)
                if (HasShaderToken(building.m_props[index].m_finalProp?.m_mesh?.name))
                {
                    building.m_maxPropDistance = 25000;
                    break;
                }
        }

        /// <summary>
        /// Scans sub building assets, adding any using the shader to the list.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        private static void Add_SubBuildings(List<ShaderAsset> assetList)
        {
            foreach (var subBuilding in Resources.FindObjectsOfTypeAll<BuildingInfoSub>())
                try
                {
                    if (HasShaderToken(subBuilding?.m_mesh?.name))
                        assetList.Add(new ShaderAsset(subBuilding));
                }
                catch (Exception error)
                {
                    Debug.LogError($"[AdditiveShader] BuildingInfoSub error: {subBuilding?.name} \n{error}");
                }
        }

        /// <summary>
        /// Scans vehicle assets, adding any using the shader to the list.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        private static void Add_Vehicles(List<ShaderAsset> assetList)
        {
            foreach (var vehicle in Resources.FindObjectsOfTypeAll<VehicleInfoSub>())
                try
                {
                    if (HasShaderToken(vehicle?.m_mesh?.name))
                        assetList.Add(new ShaderAsset(vehicle));
                }
                catch (Exception error)
                {
                    Debug.LogError($"[AdditiveShader] VehicleInfoSub error: {vehicle?.name} \n{error}");
                }
        }
    }
}
