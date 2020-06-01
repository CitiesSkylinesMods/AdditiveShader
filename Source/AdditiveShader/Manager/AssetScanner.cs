namespace AdditiveShader.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// <para>The <see cref="AssetScanner"/> compiles a list of assets which use the additive shader.</para>
    /// <para>The scanner runs once, during <see cref="AdditiveShaderManager.Start()"/>, and as such is _not_ performance critical.</para>
    /// </summary>
    [SuppressMessage("Class Design", "AV1008:Class should not be static")]
    internal static class AssetScanner
    {
        /// <summary>
        /// If a mesh name contains this token, it uses the additive shader.
        /// </summary>
        private const string TOKEN = "AdditiveShader";

        /// <summary>
        /// If a building contains a prop which uses additive shader,
        /// the <see cref="BuildingInfo.m_maxPropDistance"/> must be
        /// increased to prevent its props using LOD.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore")]
        private const float EXTENDED_BUILDING_PROP_VISIBILITY_DISTANCE = 25000;

        /// <summary>
        /// Gets the total number of items scanned, regardless of whether they use additive shader.
        /// </summary>
        public static ulong ItemsScanned { get; private set; }

        /// <summary>
        /// Scans supported asset types, converting any that use the shader to
        /// <see cref="ShaderAsset"/> instances which are collated in the
        /// returned list.
        /// </summary>
        /// <returns>Returns a list of <see cref="ShaderAsset"/> instances.</returns>
        internal static ICollection<ShaderAsset> ListShaderAssets()
        {
            ItemsScanned = 0;

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
            !string.IsNullOrEmpty(meshName) && meshName.StartsWith(TOKEN);

        /// <summary>
        /// Scans prop assets, adding any using the shader to the list.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        [SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "m_mesh is not derived from MonoBehaviour")]
        private static void Add_Props(List<ShaderAsset> assetList)
        {
            foreach (var prop in Resources.FindObjectsOfTypeAll<PropInfo>())
                try
                {
                    ++ItemsScanned;

                    if (prop && HasShaderToken(prop.m_mesh?.name))
                        assetList.Add(new ShaderAsset(prop));
                }
                catch (Exception error)
                {
                    Debug.LogError($"[AdditiveShader] PropInfo error: {prop.name}\n{error}");
                }
        }

        /// <summary>
        /// Scans building assets, adding any using the shader to the list.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        [SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "m_mesh is not derived from MonoBehaviour")]
        private static void Add_Buildings(List<ShaderAsset> assetList)
        {
            foreach (var building in Resources.FindObjectsOfTypeAll<BuildingInfo>())
                try
                {
                    ++ItemsScanned;

                    if (building)
                    {
                        if (HasShaderToken(building.m_mesh?.name))
                            assetList.Add(new ShaderAsset(building));

                        if (building.m_props != null)
                            CheckBuildingForShaderProps(building);
                    }
                }
                catch (Exception error)
                {
                    Debug.LogError($"[AdditiveShader] BuildingInfo error: {building.name} \n{error}");
                }
        }

        /// <summary>
        /// Because LODs don't support additive shader, if there are any props in the building that use
        /// it we have to increase the <c>m_maxPropDistance</c> for the whole building, _in addition_ to
        /// the props themselves being updated (<see cref="Add_Props(List{ShaderAsset})"/>).
        /// </summary>
        /// <param name="building">The <see cref="BuildingInfo"/> asset to scan.</param>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        [SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "m_mesh is not derived from MonoBehaviour")]
        private static void CheckBuildingForShaderProps(BuildingInfo building)
        {
            if (building.m_props.Any(prop => prop.m_finalProp && HasShaderToken(prop.m_finalProp.m_mesh.name)))
                building.m_maxPropDistance = EXTENDED_BUILDING_PROP_VISIBILITY_DISTANCE;
        }

        /// <summary>
        /// Scans sub building assets, adding any using the shader to the list.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        [SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "m_mesh is not derived from MonoBehaviour")]
        private static void Add_SubBuildings(List<ShaderAsset> assetList)
        {
            foreach (var subBuilding in Resources.FindObjectsOfTypeAll<BuildingInfoSub>())
                try
                {
                    ++ItemsScanned;

                    if (subBuilding && HasShaderToken(subBuilding.m_mesh?.name))
                        assetList.Add(new ShaderAsset(subBuilding));
                }
                catch (Exception error)
                {
                    Debug.LogError($"[AdditiveShader] BuildingInfoSub error: {subBuilding.name} \n{error}");
                }
        }

        /// <summary>
        /// Scans vehicle assets, adding any using the shader to the list.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        [SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "m_mesh is not derived from MonoBehaviour")]
        private static void Add_Vehicles(List<ShaderAsset> assetList)
        {
            foreach (var vehicle in Resources.FindObjectsOfTypeAll<VehicleInfoSub>())
                try
                {
                    ++ItemsScanned;

                    if (vehicle && HasShaderToken(vehicle.m_mesh?.name))
                        assetList.Add(new ShaderAsset(vehicle));
                }
                catch (Exception error)
                {
                    Debug.LogError($"[AdditiveShader] VehicleInfoSub error: {vehicle.name} \n{error}");
                }
        }
    }
}
