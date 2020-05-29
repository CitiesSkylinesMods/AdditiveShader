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
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderAsset"/> class
        /// for a <see cref="PropInfo"/> asset.
        /// </summary>
        /// <param name="asset">The <see cref="PropInfo"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="asset"/> is <c>null</c>.</exception>
        public ShaderAsset(PropInfo asset)
        {
            TypeOfAsset = AssetType.Prop;
            Prop = asset ?? throw new ArgumentNullException(nameof(asset));

            Info = new ShaderInfo(asset.m_mesh.name);

            asset.m_lodHasDifferentShader = false;
            asset.m_material.SetFloat("_InvFade", Info.Fade);
            asset.m_lodRenderDistance = asset.m_maxRenderDistance = GetRenderDistance(asset.m_generatedInfo.m_size);

            SetVisible(Info.AlwaysOn);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderAsset"/> class
        /// for a <see cref="BuildingInfo"/> asset.
        /// </summary>
        /// <param name="asset">The <see cref="BuildingInfo"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="asset"/> is <c>null</c>.</exception>
        public ShaderAsset(BuildingInfo asset)
        {
            TypeOfAsset = AssetType.Building;
            Building = asset ?? throw new ArgumentNullException(nameof(asset));

            Info = new ShaderInfo(asset.m_mesh.name);

            asset.m_lodHasDifferentShader = false;
            asset.m_lodMissing = true;
            asset.m_material.SetFloat("_InvFade", Info.Fade);
            asset.m_mesh.colors = GetMeshColors(asset.m_mesh.vertices.Length);
            asset.m_maxLodDistance = asset.m_minLodDistance = GetRenderDistance(asset.m_generatedInfo.m_size);

            SetVisible(Info.AlwaysOn);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderAsset"/> class
        /// for a <see cref="BuildingInfoSub"/> asset.
        /// </summary>
        /// <param name="asset">The <see cref="BuildingInfoSub"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="asset"/> is <c>null</c>.</exception>
        public ShaderAsset(BuildingInfoSub asset)
        {
            TypeOfAsset = AssetType.SubBuilding;
            SubBuilding = asset ?? throw new ArgumentNullException(nameof(asset));

            Info = new ShaderInfo(asset.m_mesh.name);

            asset.m_lodHasDifferentShader = false;
            asset.m_material.SetFloat("_InvFade", Info.Fade);
            asset.m_mesh.colors = GetMeshColors(asset.m_mesh.vertices.Length);
            asset.m_maxLodDistance = asset.m_minLodDistance = GetRenderDistance(asset.m_generatedInfo.m_size);

            SetVisible(Info.AlwaysOn);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderAsset"/> class
        /// for a <see cref="VehicleInfoSub"/> asset.
        /// </summary>
        /// <param name="asset">The <see cref="VehicleInfoSub"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="asset"/> is <c>null</c>.</exception>
        public ShaderAsset(VehicleInfoSub asset)
        {
            TypeOfAsset = AssetType.Vehicle;
            Vehicle = asset ?? throw new ArgumentNullException(nameof(asset));

            Info = new ShaderInfo(asset.m_mesh.name);

            asset.m_material.SetFloat("_InvFade", Info.Fade);
            asset.m_mesh.colors = GetMeshColors(asset.m_mesh.vertices.Length);
            asset.m_lodRenderDistance = asset.m_maxRenderDistance = GetRenderDistance(asset.m_generatedInfo.m_size);

            SetVisible(Info.AlwaysOn);
        }

        /// <summary>
        /// <para>Gets a value indicating whether the additive shader for the asset is currently visible.</para>
        /// <para>Use <see cref="Show()"/>, <see cref="Hide()"/> or <see cref="SetVisible(bool)"/> to change visibility.</para>
        /// </summary>
        public bool IsVisible { get; private set; }

        /// <summary>
        /// Gets a value containing shader parameters which were parsed from the mesh name.
        /// </summary>
        public ShaderInfo Info { get; }

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
        private AssetType TypeOfAsset { get; }

        /// <summary>
        /// Gets the <see cref="PropInfo"/> associated with this instance, if applicable.
        /// </summary>
        private PropInfo Prop { get; }

        /// <summary>
        /// Gets the <see cref="BuildingInfo"/> associated with this instance, if applicable.
        /// </summary>
        private BuildingInfo Building { get; }

        /// <summary>
        /// Gets the <see cref="BuildingInfoSub"/> associated with this instance, if applicalbe.
        /// </summary>
        private BuildingInfoSub SubBuilding { get; }

        /// <summary>
        /// Gets the <see cref="VehicleInfoSub"/> associated with this instance, if applicable.
        /// </summary>
        private VehicleInfoSub Vehicle { get; }

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
        /// <param name="visible">If <c>true</c>, the shader will be shown, otherwise it will be hidden.</param>
        public void SetVisible(bool visible)
        {
            if (IsVisible == visible)
                return;

            IsVisible = visible;

            switch (TypeOfAsset)
            {
                case AssetType.Prop:
                    Prop.m_material.SetFloat("_Intensity", visible ? Info.Intensity : 0f);
                    break;
                case AssetType.Building:
                    Building.m_material.SetFloat("_Intensity", visible ? Info.Intensity : 0f);
                    break;
                case AssetType.SubBuilding:
                    SubBuilding.m_material.SetFloat("_Intensity", visible ? Info.Intensity : 0f);
                    break;
                case AssetType.Vehicle:
                    Vehicle.m_material.SetFloat("_Intensity", visible ? Info.Intensity : 0f);
                    break;
            }
        }

        /// <summary>
        /// Check if a tag is defined. Case sensitive. Tags should always be lower case.
        /// </summary>
        /// <param name="tag">The tag to check.</param>
        /// <returns>Returns <c>true</c> if tag found, otherwise <c>false</c>.</returns>
        public bool HasTag(string tag) =>
            Info.HasTag(tag);

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