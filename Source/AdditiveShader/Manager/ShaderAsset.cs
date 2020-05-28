namespace AdditiveShader.Manager
{
    using System;
    using UnityEngine;

    /// <summary>
    /// <para>Represents an asset which uses the additive shader.</para>
    /// <para>See <see cref="https://cslmodding.info/mod/additive-shader/"/> for details.</para>
    /// </summary>
    public class ShaderAsset
    {
        // TODO: Can these constructors be merged somehow? Most of what they do is identical
        // there are a few diffs based on asset type but not enough to merit 4 bloated constructors.

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderAsset"/> class
        /// for a <see cref="PropInfo"/> asset.
        /// </summary>
        /// <param name="info">The <see cref="PropInfo"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="info"/> is <c>null</c>.</exception>
        public ShaderAsset(PropInfo info)
        {
            TypeOfAsset = AssetType.Prop;
            Prop = info ?? throw new ArgumentNullException(nameof(info));

            Info = new ShaderInfo(info.m_mesh.name);

            info.m_lodHasDifferentShader = false;
            info.m_material.SetFloat("_InvFade", Info.Fade);
            info.m_lodRenderDistance = info.m_maxRenderDistance = GetRenderDistance(info.m_generatedInfo.m_size);

            SetVisible(info, Info.AlwaysOn);

            Debug.Log($"[AdditiveShader] {this} {Info}");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderAsset"/> class
        /// for a <see cref="BuildingInfo"/> asset.
        /// </summary>
        /// <param name="info">The <see cref="BuildingInfo"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="info"/> is <c>null</c>.</exception>
        public ShaderAsset(BuildingInfo info)
        {
            TypeOfAsset = AssetType.Building;
            Building = info ?? throw new ArgumentNullException(nameof(info));

            Info = new ShaderInfo(info.m_mesh.name);

            info.m_lodHasDifferentShader = false;
            info.m_lodMissing = true;
            info.m_material.SetFloat("_InvFade", Info.Fade);
            info.m_mesh.colors = GetMeshColors(info.m_mesh.vertices.Length);
            info.m_maxLodDistance = info.m_minLodDistance = GetRenderDistance(info.m_generatedInfo.m_size);

            SetVisible(info, Info.AlwaysOn);

            Debug.Log($"[AdditiveShader] {this} {Info}");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderAsset"/> class
        /// for a <see cref="BuildingInfoSub"/> asset.
        /// </summary>
        /// <param name="info">The <see cref="BuildingInfoSub"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="info"/> is <c>null</c>.</exception>
        public ShaderAsset(BuildingInfoSub info)
        {
            TypeOfAsset = AssetType.SubBuilding;
            SubBuilding = info ?? throw new ArgumentNullException(nameof(info));

            Info = new ShaderInfo(info.m_mesh.name);

            info.m_lodHasDifferentShader = false;
            info.m_material.SetFloat("_InvFade", Info.Fade);
            info.m_mesh.colors = GetMeshColors(info.m_mesh.vertices.Length);
            info.m_maxLodDistance = info.m_minLodDistance = GetRenderDistance(info.m_generatedInfo.m_size);

            SetVisible(info, Info.AlwaysOn);

            Debug.Log($"[AdditiveShader] {this} {Info}");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderAsset"/> class
        /// for a <see cref="VehicleInfoSub"/> asset.
        /// </summary>
        /// <param name="info">The <see cref="VehicleInfoSub"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="info"/> is <c>null</c>.</exception>
        public ShaderAsset(VehicleInfoSub info)
        {
            TypeOfAsset = AssetType.Vehicle;
            Vehicle = info ?? throw new ArgumentNullException(nameof(info));

            Info = new ShaderInfo(info.m_mesh.name);

            info.m_material.SetFloat("_InvFade", Info.Fade);
            info.m_mesh.colors = GetMeshColors(info.m_mesh.vertices.Length);
            info.m_lodRenderDistance = info.m_maxRenderDistance = GetRenderDistance(info.m_generatedInfo.m_size);

            SetVisible(info, Info.AlwaysOn);

            Debug.Log($"[AdditiveShader] {this} {Info}");
        }

        /// <summary>
        /// Gets or sets a value indicating whether the additive shader for the asset is currently visible.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// <para>Gets a value indicating what type of asset this instance represents.</para>
        /// <para>
        /// Depending on the type, the asset will be stored in one of the following members:
        /// <list type="bullet">
        /// <item><see cref="Prop"/></item>
        /// <item><see cref="Building"/></item>
        /// <item><see cref="SubBuilding"/></item>
        /// <item><see cref="Vehicle"/></item>
        /// </list>
        /// </para>
        /// </summary>
        public AssetType TypeOfAsset { get; }

        /// <summary>
        /// Gets the <see cref="PropInfo"/> associated with this instance, if applicable.
        /// </summary>
        public PropInfo Prop { get; }

        /// <summary>
        /// Gets the <see cref="BuildingInfo"/> associated with this instance, if applicable.
        /// </summary>
        public BuildingInfo Building { get; }

        /// <summary>
        /// Gets the <see cref="BuildingInfoSub"/> associated with this instance, if applicalbe.
        /// </summary>
        public BuildingInfoSub SubBuilding { get; }

        /// <summary>
        /// Gets the <see cref="VehicleInfoSub"/> associated with this instance, if applicable.
        /// </summary>
        public VehicleInfoSub Vehicle { get; }

        /// <summary>
        /// Gets a value containing shader parameters which were parsed from the mesh name.
        /// </summary>
        public ShaderInfo Info { get; }

        /// <inheritdoc/>
        public override string ToString() => TypeOfAsset switch
        {
            AssetType.Prop        => $"ShaderAsset(PropInfo {Prop.name})",
            AssetType.Building    => $"ShaderAsset(BuildingInfo {Building.name})",
            AssetType.SubBuilding => $"ShaderAsset(BuildingInfoSub {SubBuilding.name})",
            AssetType.Vehicle     => $"ShaderAsset(VehicleInfoSub {Vehicle.name})",

            _ => "ShaderAsset(null) Error",
        };

        /// <summary>
        /// Shows the additive shader for this asset.
        /// </summary>
        public void Show() => SetVisible(true);

        /// <summary>
        /// Hides the additive shader for this asset.
        /// </summary>
        public void Hide() => SetVisible(false);

        /// <summary>
        /// Show or hide the additive shader for this asset.
        /// </summary>
        /// <param name="visibility">If <c>true</c>, the shader will be shown, otherwise it will be hidden.</param>
        public void SetVisible(bool visibility)
        {
            if (IsVisible == visibility)
                return;

            IsVisible = visibility;

            switch (TypeOfAsset)
            {
                case AssetType.Prop:
                    SetVisible(Prop, visibility);
                    break;
                case AssetType.Building:
                    SetVisible(Building, visibility);
                    break;
                case AssetType.SubBuilding:
                    SetVisible(SubBuilding, visibility);
                    break;
                case AssetType.Vehicle:
                    SetVisible(Vehicle, visibility);
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

        /// <summary>
        /// Returns an array filled with <see cref="Color.white"/>.
        /// </summary>
        /// <param name="count">The size of the array (number of mesh vertices).</param>
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
        /// <param name="size">The asset mesh size.</param>
        /// <returns>Returns the render distance applicable to the asset.</returns>
        private float GetRenderDistance(Vector3 size) =>
            (size.x + 30) * (size.y + 30) * (size.z + 30) * 0.1f;
    }
}
