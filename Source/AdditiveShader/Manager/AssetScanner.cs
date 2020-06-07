namespace AdditiveShader.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// The <see cref="AssetScanner"/> compiles a list of assets which Additive Shader mod will manage.
    /// </summary>
    [SuppressMessage("Class Design", "AV1008:Class should not be static")]
    internal static class AssetScanner
    {
        /// <summary>
        /// If a mesh name contains this token, it uses the additive shader.
        /// </summary>
        private const string TOKEN = "AdditiveShader";

        /// <summary>
        /// Gets total number of scanned assets; used by <see cref="AssetReporter"/>.
        /// </summary>
        public static int ItemsScanned { get; private set; }

        /// <summary>
        /// Scans supported asset types, converting any that use the shader to
        /// <see cref="ManagedAsset"/> instances which are collated in the
        /// returned list.
        /// </summary>
        /// <returns>Returns a list of <see cref="ManagedAsset"/> instances.</returns>
        [SuppressMessage("Member Design", "AV1130:Return type in method signature should be a collection interface instead of a concrete type", Justification = "Comprehensibility.")]
        internal static List<ManagedAsset> CollateManagedAssets()
        {
            ItemsScanned = 0;

            var list = new List<ManagedAsset>(150);

            Add_Props(list);
            Add_Buildings(list);
            Add_SubBuildings(list);
            Add_Vehicles(list);

            list.TrimExcess();

            return list;
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
        private static void Add_Props(List<ManagedAsset> list)
        {
            int count = PrefabCollection<PropInfo>.LoadedCount();

            ItemsScanned += count;

            for (uint index = 0; index < count; index++)
                try
                {
                    var prop = PrefabCollection<PropInfo>.GetLoaded(index);

                    if (prop && prop.m_isCustomContent && prop.m_mesh && HasShaderToken(prop.m_mesh.name))
                        list.Add(new ManagedAsset(prop));
                }
                catch (Exception error)
                {
                    Debug.LogError($"[AdditiveShader] PropInfo error: \n{error}");
                }
        }

        /// <summary>
        /// Scans building assets, adding any using the shader to the list.
        /// </summary>
        private static void Add_Buildings(List<ManagedAsset> list)
        {
            int count = PrefabCollection<BuildingInfo>.LoadedCount();

            ItemsScanned += count;

            for (uint index = 0; index < count; index++)
                try
                {
                    var building = PrefabCollection<BuildingInfo>.GetLoaded(index);

                    if (building && building.m_isCustomContent)
                    {
                        if (building.m_mesh && HasShaderToken(building.m_mesh.name))
                            list.Add(new ManagedAsset(building));

                        if (building.m_props != null && ContainsShaderProps(building))
                            list.Add(new ManagedAsset(building, true));
                    }
                }
                catch (Exception error)
                {
                    Debug.LogError($"[AdditiveShader] BuildingInfo error: \n{error}");
                }
        }

        /// <summary>
        /// Because LODs don't support additive shader, if there are any props in the building that use
        /// it we have to increase the <c>m_maxPropDistance</c> for the whole building, _in addition_ to
        /// the props themselves being updated (<see cref="Add_Props(List{ManagedAsset})"/>).
        /// </summary>
        /// <param name="building">The <see cref="BuildingInfo"/> to inspect.</param>
        /// <returns>Returns <c>true</c> if the building contains shader-using props, otherwise <c>false</c>.</returns>
        private static bool ContainsShaderProps(BuildingInfo building) =>
            building.m_props.Any(prop =>
                prop.m_finalProp &&
                prop.m_finalProp.m_isCustomContent &&
                HasShaderToken(prop.m_finalProp.m_mesh.name));

        /// <summary>
        /// Scans sub building assets, adding any using the shader to the list.
        /// </summary>
        private static void Add_SubBuildings(List<ManagedAsset> assetList)
        {
            // PrefabCollection (used in methods above) doesn't appear to contain BuildingInfoSub.
            // So we have to use FindObjectsOfTypeAll, which is 2x slower :(
            // Also, the m_isCustomContent is false for all subs.
            // Luckily there aren't many of them.
            foreach (var subBuilding in Resources.FindObjectsOfTypeAll<BuildingInfoSub>())
                try
                {
                    ++ItemsScanned;

                    if (subBuilding && subBuilding.m_mesh && HasShaderToken(subBuilding.m_mesh.name))
                        assetList.Add(new ManagedAsset(subBuilding));
                }
                catch (Exception error)
                {
                    Debug.LogError($"[AdditiveShader] BuildingInfoSub error: \n{error}");
                }
        }

        /// <summary>
        /// Scans vehicle assets, adding any using the shader to the list.
        /// </summary>
        private static void Add_Vehicles(List<ManagedAsset> assetList)
        {
            // Same situation as for BuildingInfoSub, we have to use FindObjectsOfTypeAll :(
            foreach (var vehicle in Resources.FindObjectsOfTypeAll<VehicleInfoSub>())
                try
                {
                    ++ItemsScanned;

                    if (vehicle && vehicle.m_mesh && HasShaderToken(vehicle.m_mesh.name))
                        assetList.Add(new ManagedAsset(vehicle));
                }
                catch (Exception error)
                {
                    Debug.LogError($"[AdditiveShader] VehicleInfoSub error: \n{error}");
                }
        }
    }
}
