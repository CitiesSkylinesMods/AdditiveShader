namespace AdditiveShader
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Represents an asset which uses the additive shader.
    /// See <see cref="https://cslmodding.info/mod/additive-shader/"/> for details.
    /// </summary>
    public class AdditiveShader
    {

        /// <summary>
        /// Gets a value indicating what type of asset this instance represents.
        /// </summary>
        public AssetType m_assetType { get; private set; }

        // TODO: Surely there is cleaner way to do this?
        public PropInfo m_prop { get; private set; }
        public BuildingInfo m_building { get; private set; }
        public BuildingInfoSub m_buildingSubMesh { get; private set; }
        public VehicleInfoSub m_vehicleSubMesh { get; private set; }

        /// <summary>
        /// Gets a value containing shader parameters which were parsed from the mesh name.
        /// </summary>
        public ShaderParams Info { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the additive shader for the asset is currently visible.
        /// </summary>
        public bool IsVisible { get; set; }

        // TODO: Can these constructors be merged somehow? Most of what they do is identical
        // there are a few diffs based on asset type but not enough to merit 4 bloated constructors.

        /// <summary>
        /// Initialises an instance of the <see cref="AdditiveShader"/> class
        /// for a <see cref="PropInfo"/> asset.
        /// </summary>
        public AdditiveShader(PropInfo info)
        {
            Debug.Log($"[AdditiveShader] Initialising PropInfo: {info.name}");

            m_assetType = AssetType.Prop;
            m_prop = info;

            Info = new ShaderParams(info.m_mesh.name);

            info.m_lodHasDifferentShader = false;
            info.m_material.SetFloat("_InvFade", Info.Fade);
            info.m_lodRenderDistance = info.m_maxRenderDistance = GetRenderDistance(info.m_generatedInfo.m_size);

            SetVisible(info, Info.AlwaysOn);
        }

        /// <summary>
        /// Initialises an instance of the <see cref="AdditiveShader"/> class
        /// for a <see cref="BuildingInfo"/> asset.
        /// </summary>
        public AdditiveShader(BuildingInfo info)
        {
            Debug.Log($"[AdditiveShader] Initialising BuildingInfo: {info.name}");

            m_assetType = AssetType.Building;
            m_building = info;

            Info = new ShaderParams(info.m_mesh.name);

            info.m_lodHasDifferentShader = false;
            info.m_lodMissing = true;
            info.m_material.SetFloat("_InvFade", Info.Fade);
            info.m_mesh.colors = GetMeshColors(info.m_mesh.vertices.Length);
            info.m_maxLodDistance = info.m_minLodDistance = GetRenderDistance(info.m_generatedInfo.m_size);

            SetVisible(info, Info.AlwaysOn);
        }

        /// <summary>
        /// Initialises an instance of the <see cref="AdditiveShader"/> class
        /// for a <see cref="BuildingInfoSub"/> asset.
        /// </summary>
        public AdditiveShader(BuildingInfoSub info)
        {
            Debug.Log($"[AdditiveShader] Initialising BuildingInfoSub: {info.name}");

            m_assetType = AssetType.BuildingSubMesh;
            m_buildingSubMesh = info;

            Info = new ShaderParams(info.m_mesh.name);

            info.m_lodHasDifferentShader = false;
            info.m_material.SetFloat("_InvFade", Info.Fade);
            info.m_mesh.colors = GetMeshColors(info.m_mesh.vertices.Length);
            info.m_maxLodDistance = info.m_minLodDistance = GetRenderDistance(info.m_generatedInfo.m_size);

            SetVisible(info, Info.AlwaysOn);
        }

        /// <summary>
        /// Initialises an instance of the <see cref="AdditiveShader"/> class
        /// for a <see cref="VehicleInfoSub"/> asset.
        /// </summary>
        /// <param name="info"></param>
        public AdditiveShader(VehicleInfoSub info)
        {
            Debug.Log($"[AdditiveShader] Initialising VehicleInfoSub: {info.name}");

            m_assetType = AssetType.VehicleSubMesh;
            m_vehicleSubMesh = info;

            Info = new ShaderParams(info.m_mesh.name);

            info.m_material.SetFloat("_InvFade", Info.Fade);
            info.m_mesh.colors = GetMeshColors(info.m_mesh.vertices.Length);
            info.m_lodRenderDistance = info.m_maxRenderDistance = GetRenderDistance(info.m_generatedInfo.m_size);

            SetVisible(info, Info.AlwaysOn);
        }

        /// <summary>
        /// Returns an array filled with <see cref="Color.white"/>.
        /// </summary>
        /// 
        /// <param name="count">The size of the array (number of mesh vertices).</param>
        /// 
        /// <returns>An array of specified size filled with white color.</returns>
        private Color[] GetMeshColors(int count)
        {
            var colors = new Color[count];

            for (int i = 0; i < count; i++)
                colors[i] = Color.white;

            return colors;
        }

        /// <summary>
        /// Additive shader doesn't work on LODs, so the render distance of the
        /// asset is increased, based on its size, to keep the effect visible for longer.
        /// </summary>
        /// 
        /// <param name="size">The asset mesh size.</param>
        /// 
        /// <returns>Returns the render distance applicable to the asset.</returns>
        private float GetRenderDistance(Vector3 size) =>
            (size.x + 30) * (size.y + 30) * (size.z + 30) * 0.1f;

        public void Show() => SetVisible(true);

        public void Hide() => SetVisible(false);

        public void SetVisible(bool state)
        {
            if (IsVisible == state) return;

            IsVisible = state;

            switch (m_assetType)
            {
                case AssetType.Prop:
                    SetVisible(m_prop, state);
                    break;
                case AssetType.Building:
                    SetVisible(m_building, state);
                    break;
                case AssetType.BuildingSubMesh:
                    SetVisible(m_buildingSubMesh, state);
                    break;
                case AssetType.VehicleSubMesh:
                    SetVisible(m_vehicleSubMesh, state);
                    break;
                default:
                    break;
            }
        }

        private void SetVisible(PropInfo info, bool visible) =>
            info.m_material.SetFloat("_Intensity", visible ? Info.Intensity : 0f);

        private void SetVisible(BuildingInfo info, bool visible) =>
            info.m_material.SetFloat("_Intensity", visible ? Info.Intensity : 0f);

        private void SetVisible(BuildingInfoSub info, bool visible) =>
            info.m_material.SetFloat("_Intensity", visible ? Info.Intensity : 0f);

        private void SetVisible(VehicleInfoSub info, bool visible) =>
            info.m_material.SetFloat("_Intensity", visible ? Info.Intensity : 0f);
    }
}
