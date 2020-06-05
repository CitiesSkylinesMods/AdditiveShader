namespace AdditiveShader.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("Maintainability", "AV1522:Assign each property, field, parameter or variable in a separate statement", Justification = "Intentional.")]
        internal ShaderAsset(PropInfo asset)
        {
            TypeOfAsset = AssetType.Prop;
            Prop = asset;

            Info = new ShaderInfo(asset.m_mesh.name, nameof(PropInfo), asset.name);

            asset.m_lodHasDifferentShader = false;
            asset.m_material.SetFloat("_InvFade", Info.Fade);
            asset.m_lodRenderDistance = asset.m_maxRenderDistance = GetRenderDistance(asset.m_generatedInfo.m_size);

            SetVisible(Info.IsAlwaysOn, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderAsset"/> class
        /// for a <see cref="BuildingInfo"/> asset.
        /// </summary>
        /// <param name="asset">The <see cref="BuildingInfo"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="asset"/> is <c>null</c>.</exception>
        [SuppressMessage("Maintainability", "AV1522:Assign each property, field, parameter or variable in a separate statement", Justification = "Intentional.")]
        internal ShaderAsset(BuildingInfo asset)
        {
            TypeOfAsset = AssetType.Building;
            Building = asset;

            Info = new ShaderInfo(asset.m_mesh.name, nameof(BuildingInfo), asset.name);

            asset.m_lodHasDifferentShader = false;
            asset.m_lodMissing = true;
            asset.m_material.SetFloat("_InvFade", Info.Fade);
            asset.m_mesh.colors = GetMeshColors(asset.m_mesh.vertices.Length);
            asset.m_maxLodDistance = asset.m_minLodDistance = GetRenderDistance(asset.m_generatedInfo.m_size);

            SetVisible(Info.IsAlwaysOn, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderAsset"/> class
        /// for a <see cref="BuildingInfoSub"/> asset.
        /// </summary>
        /// <param name="asset">The <see cref="BuildingInfoSub"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="asset"/> is <c>null</c>.</exception>
        [SuppressMessage("Maintainability", "AV1522:Assign each property, field, parameter or variable in a separate statement", Justification = "Intentional.")]
        internal ShaderAsset(BuildingInfoSub asset)
        {
            TypeOfAsset = AssetType.SubBuilding;
            SubBuilding = asset;

            Info = new ShaderInfo(asset.m_mesh.name, nameof(BuildingInfoSub), asset.name);

            asset.m_lodHasDifferentShader = false;
            asset.m_material.SetFloat("_InvFade", Info.Fade);
            asset.m_mesh.colors = GetMeshColors(asset.m_mesh.vertices.Length);
            asset.m_maxLodDistance = asset.m_minLodDistance = GetRenderDistance(asset.m_generatedInfo.m_size);

            SetVisible(Info.IsAlwaysOn, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderAsset"/> class
        /// for a <see cref="VehicleInfoSub"/> asset.
        /// </summary>
        /// <param name="asset">The <see cref="VehicleInfoSub"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="asset"/> is <c>null</c>.</exception>
        [SuppressMessage("Maintainability", "AV1522:Assign each property, field, parameter or variable in a separate statement", Justification = "Intentional.")]
        internal ShaderAsset(VehicleInfoSub asset)
        {
            TypeOfAsset = AssetType.Vehicle;
            Vehicle = asset;

            Info = new ShaderInfo(asset.m_mesh.name, nameof(VehicleInfoSub), asset.name);

            asset.m_material.SetFloat("_InvFade", Info.Fade);
            asset.m_mesh.colors = GetMeshColors(asset.m_mesh.vertices.Length);
            asset.m_lodRenderDistance = asset.m_maxRenderDistance = GetRenderDistance(asset.m_generatedInfo.m_size);

            SetVisible(Info.IsAlwaysOn, true);
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
        /// Gets hashtags for the shader. Case sensitive. Tags should always be lower case.
        /// </summary>
        /// <returns>Returns a <see cref="HashSet{string}"/> represetnation of the shader tags.</returns>
        public HashSet<string> HashTags => Info.HashTags;

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

        /// <inheritdoc/>
        public override string ToString() => TypeOfAsset switch
        {
            AssetType.Prop        => $"PropInfo        {Prop.name}",
            AssetType.Building    => $"BuildingInfo    {Building.name}",
            AssetType.SubBuilding => $"BuildingInfoSub {SubBuilding.name}",
            AssetType.Vehicle     => $"VehicleInfoSub  {Vehicle.name}",

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
        /// Show or hide the additive shader for this asset based on game world time.
        /// </summary>
        /// <param name="time">The game time of day.</param>
        public void SetVisibleByTime(float time) =>
            SetVisible(
                Info.OverlapsMidnight
                    ? time < Info.OffTime || Info.OnTime <= time
                    : Info.OnTime <= time && time < Info.OffTime);

        /// <summary>
        /// Show or hide the additive shader for this asset based on night vs. day.
        /// </summary>
        /// <param name="currentlyNightTime">Set <c>true</c> if it is now night time in game world.</param>
        public void SetVisibleByTwilight(bool currentlyNightTime) =>
            SetVisible(currentlyNightTime == Info.IsNightTimeOnly);

        /// <summary>
        /// Show or hide the additive shader for this asset.
        /// </summary>
        /// <param name="visible">If <c>true</c>, the shader will be shown, otherwise it will be hidden.</param>
        /// <param name="force">If <c>true</c>, don't check current state. Defaults to <c>false</c>.</param>
        [SuppressMessage("Maintainability", "AV1536:Non-exhaustive switch statement requires a default case clause")]
        [SuppressMessage("Maintainability", "AV1535:Missing block in case or default clause of switch statement")]
        public void SetVisible(bool visible, bool force = false)
        {
            if (!force && IsVisible == visible)
                return;

            IsVisible = visible;

            var material = TypeOfAsset switch
            {
                AssetType.Prop        => Prop.m_material,
                AssetType.Building    => Building.m_material,
                AssetType.SubBuilding => SubBuilding.m_material,
                AssetType.Vehicle     => Vehicle.m_material,
                _ => throw new IndexOutOfRangeException("Invalid AssetType"),
            };

            material.SetFloat("_Intensity", visible ? Info.Intensity : 0f);
        }

        /// <summary>
        /// Returns an array filled with <see cref="Color.white"/>.
        /// </summary>
        /// <param name="count">The size of the array (number of mesh vertices).</param>
        /// <returns>An array of specified size filled with white color.</returns>
        [SuppressMessage("Member Design", "AV1130:Return type in method signature should be a collection interface instead of a concrete type", Justification = "Game requirement.")]
        private Color[] GetMeshColors(int count)
        {
            var colors = new Color[count];

            for (int index = 0; index < count; index++)
                colors[index] = Color.white;

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
