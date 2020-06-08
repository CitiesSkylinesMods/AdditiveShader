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
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore")]
    public class ManagedAsset
    {
        /// <summary>
        /// Used to force visibility update during instantiation.
        /// </summary>
        private const bool FORCE_UPDATE = true;

        /// <summary>
        /// Fake <c>m_mesh.name</c> for <see cref="AssetType.Container"/> assets.
        /// </summary>
        /// <remarks>
        /// It is passed to constructor of <see cref="ShaderInfo"/> class
        /// which will treat it as 'Continer' profile.
        /// </remarks>
        private const string CONTAINER_BUILDING = "AdditiveShader Container 0 0 container-building";

        /// <summary>
        /// If a building contains a prop which uses additive shader,
        /// the <see cref="BuildingInfo.m_maxPropDistance"/> must be
        /// increased to prevent its props using LOD.
        /// </summary>
        private const float CONTAINER_MAX_PROP_DISTANCE = 25000;

        /// <summary>
        /// Asset mesh color (<c>m_mesh.colors[]</c>) - required to
        /// accurately display the additive shader. Should never change.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:Field names should begin with lower-case letter", Justification = "Treat like a constant.")]
        private readonly Color MESH_COLOR = Color.white;

        // Backup original values (in constructors) so they can be restored on exit.
        // Each AssetType uses as _subset_ of these backups.
        private readonly bool    backup_lodHasDifferentShader; // PropInfo, BuildingInfo, BuildingInfoSub
        private readonly bool    backup_lodMissing;            // BuildingInfo
        private readonly Color[] backup_meshColors;            // BuildingInfo, BuildingInfoSub, VehicleInfoSub
        private readonly float   backup_InvFade;               // PropInfo, BuildingInfo, BuildingInfoSub, VehicleInfoSub
        private readonly float   backup_lodRenderDistance;     // PropInfo, VehicleInfoSub
        private readonly float   backup_maxRenderDistance;     // PropInfo, VehicleInfoSub
        private readonly float   backup_maxLodDistance;        // BuildingInfo, BuildingInfoSub
        private readonly float   backup_minLodDistance;        // BuildingInfo, BuildingInfoSub
        private readonly float   backup_maxPropDistance;       // Container (BuildingInfo)

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedAsset"/> class
        /// for a <see cref="PropInfo"/> asset.
        /// </summary>
        /// <param name="asset">The <see cref="PropInfo"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="asset"/> is <c>null</c>.</exception>
        internal ManagedAsset(PropInfo asset)
        {
            TypeOfAsset = AssetType.Prop;
            Prop = asset;

            Info = new ShaderInfo(asset.m_mesh.name, nameof(PropInfo), asset.name);

            backup_lodHasDifferentShader = asset.m_lodHasDifferentShader;
            backup_InvFade = asset.m_material.GetFloat("_InvFade");

            backup_lodRenderDistance = asset.m_lodRenderDistance;
            backup_maxRenderDistance = asset.m_maxRenderDistance;

            asset.m_lodHasDifferentShader = false;
            asset.m_material.SetFloat("_InvFade", Info.Fade);

            CachedRenderDistance = GetRenderDistance(asset.m_generatedInfo.m_size);
            ApplyCachedRenderDistance();

            SetVisible(Info.IsAlwaysOn, FORCE_UPDATE);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedAsset"/> class
        /// for a <see cref="BuildingInfo"/> asset.
        /// </summary>
        /// <param name="asset">The <see cref="BuildingInfo"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if asset <c>m_mesh.name</c> is <c>null</c>.</exception>
        /// <exception cref="FormatException">Thrown if asset <c>m_mesh.name</c> format is invalid.</exception>
        [SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements")]
        internal ManagedAsset(BuildingInfo asset)
        {
            TypeOfAsset = AssetType.Building;
            Building = asset;

            Info = new ShaderInfo(asset.m_mesh.name, nameof(BuildingInfo), asset.name);

            backup_lodHasDifferentShader = asset.m_lodHasDifferentShader;
            backup_lodMissing = asset.m_lodMissing;
            backup_InvFade = asset.m_material.GetFloat("_InvFade");
            backup_meshColors = asset.m_mesh.colors;

            backup_maxLodDistance = asset.m_maxLodDistance;
            backup_minLodDistance = asset.m_minLodDistance;

            asset.m_lodHasDifferentShader = false;
            asset.m_lodMissing = true;
            asset.m_material.SetFloat("_InvFade", Info.Fade);
            asset.m_mesh.colors = GetMeshColors(asset.m_mesh.vertices.Length);

            CachedRenderDistance = GetRenderDistance(asset.m_generatedInfo.m_size);
            ApplyCachedRenderDistance();

            SetVisible(Info.IsAlwaysOn, FORCE_UPDATE);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedAsset"/> class
        /// for a <see cref="BuildingInfo"/> asset which contains a shader-using
        /// <see cref="PropInfo"/> asset.
        /// </summary>
        /// <remarks>
        /// This is distinct from the other ShaderAsset types in that the building
        /// itself is not usually directly shader-using (if it is, a separate
        /// ShaderAsset will be created for it).
        /// </remarks>
        /// <param name="asset">The <see cref="BuildingInfo"/> which uses the shader.</param>
        /// <param name="isContainer">Ignored - just there to differentiate the overload.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="isContainer"/> is not <c>true</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if asset <c>m_mesh.name</c> is <c>null</c>.</exception>
        /// <exception cref="FormatException">Thrown if asset <c>m_mesh.name</c> format is invalid.</exception>
        internal ManagedAsset(BuildingInfo asset, bool isContainer)
        {
            if (!isContainer)
                throw new ArgumentOutOfRangeException(nameof(isContainer), "This constructor can only be used for buildings that contain shader-using props.");

            TypeOfAsset = AssetType.Container;
            Building = asset;

            IsContainer = true;

            Info = new ShaderInfo(CONTAINER_BUILDING, nameof(BuildingInfo), asset.name);

            backup_maxPropDistance = asset.m_maxPropDistance;

            CachedRenderDistance = CONTAINER_MAX_PROP_DISTANCE;
            ApplyCachedRenderDistance();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedAsset"/> class
        /// for a <see cref="BuildingInfoSub"/> asset.
        /// </summary>
        /// <param name="asset">The <see cref="BuildingInfoSub"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if asset <c>m_mesh.name</c> is <c>null</c>.</exception>
        /// <exception cref="FormatException">Thrown if asset <c>m_mesh.name</c> format is invalid.</exception>
        [SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements")]
        internal ManagedAsset(BuildingInfoSub asset)
        {
            TypeOfAsset = AssetType.SubBuilding;
            SubBuilding = asset;

            Info = new ShaderInfo(asset.m_mesh.name, nameof(BuildingInfoSub), asset.name);

            backup_lodHasDifferentShader = asset.m_lodHasDifferentShader;
            backup_InvFade = asset.m_material.GetFloat("_InvFade");
            backup_meshColors = asset.m_mesh.colors;

            backup_maxLodDistance = asset.m_maxLodDistance;
            backup_minLodDistance = asset.m_minLodDistance;

            asset.m_lodHasDifferentShader = false;
            asset.m_material.SetFloat("_InvFade", Info.Fade);
            asset.m_mesh.colors = GetMeshColors(asset.m_mesh.vertices.Length);

            CachedRenderDistance = GetRenderDistance(asset.m_generatedInfo.m_size);
            ApplyCachedRenderDistance();

            SetVisible(Info.IsAlwaysOn, FORCE_UPDATE);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedAsset"/> class
        /// for a <see cref="VehicleInfoSub"/> asset.
        /// </summary>
        /// <param name="asset">The <see cref="VehicleInfoSub"/> which uses the shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if asset <c>m_mesh.name</c> is <c>null</c>.</exception>
        /// <exception cref="FormatException">Thrown if asset <c>m_mesh.name</c> format is invalid.</exception>
        internal ManagedAsset(VehicleInfoSub asset)
        {
            TypeOfAsset = AssetType.Vehicle;
            Vehicle = asset;

            Info = new ShaderInfo(asset.m_mesh.name, nameof(VehicleInfoSub), asset.name);

            backup_InvFade = asset.m_material.GetFloat("_InvFade");
            backup_meshColors = asset.m_mesh.colors;

            backup_lodRenderDistance = asset.m_lodRenderDistance;
            backup_maxRenderDistance = asset.m_maxRenderDistance;

            asset.m_material.SetFloat("_InvFade", Info.Fade);
            asset.m_mesh.colors = GetMeshColors(asset.m_mesh.vertices.Length);

            CachedRenderDistance = GetRenderDistance(asset.m_generatedInfo.m_size);
            ApplyCachedRenderDistance();

            SetVisible(Info.IsAlwaysOn, FORCE_UPDATE);
        }

        /// <summary>
        /// Gets a value indicating whether this asset is just a container for another shader-using asset.
        /// </summary>
        public bool IsContainer { get; }

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
        /// <item><see cref="Prop"/></item> -- for <see cref="AssetType.Prop"/>
        /// <item><see cref="Building"/></item> -- for <see cref="AssetType.Building"/> or <see cref="AssetType.Container"/>
        /// <item><see cref="SubBuilding"/></item> -- for <see cref="AssetType.SubBuilding"/>
        /// <item><see cref="Vehicle"/></item> -- for <see cref="AssetType.Vehicle"/>
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
        /// Gets a cached render distance applicable to this asset.
        /// </summary>
        private float CachedRenderDistance { get; }

        /// <inheritdoc/>
        public override string ToString() => TypeOfAsset switch
        {
            AssetType.Prop        => $"PropInfo        {Prop.name}",
            AssetType.Building    => $"BuildingInfo    {Building.name}",
            AssetType.Container   => $"BuildingInfo[C] {Building.name}",
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
        public void SetVisible(bool visible, bool force = false)
        {
            if (IsContainer || !force && IsVisible == visible)
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
        /// Rstores the original values from backups.
        /// </summary>
        [SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements")]
        [SuppressMessage("Maintainability", "AV1536:Non-exhaustive switch statement requires a default case clause", Justification = "Intentional>")]
        public void RestoreOriginalSettings()
        {
            switch (TypeOfAsset)
            {
                case AssetType.Prop:
                    Prop.m_lodHasDifferentShader = backup_lodHasDifferentShader;
                    Prop.m_material.SetFloat("_InvFade", backup_InvFade);
                    Prop.m_lodRenderDistance = backup_lodRenderDistance;
                    Prop.m_maxRenderDistance = backup_maxRenderDistance;
                    return;

                case AssetType.Building:
                    Building.m_lodHasDifferentShader = backup_lodHasDifferentShader;
                    Building.m_lodMissing = backup_lodMissing;
                    Building.m_material.SetFloat("_InvFade", backup_InvFade);
                    Building.m_mesh.colors = backup_meshColors;
                    Building.m_maxLodDistance = backup_maxLodDistance;
                    Building.m_minLodDistance = backup_minLodDistance;
                    return;

                case AssetType.Container:
                    Building.m_maxPropDistance = backup_maxPropDistance;
                    return;

                case AssetType.SubBuilding:
                    SubBuilding.m_lodHasDifferentShader = backup_lodHasDifferentShader;
                    SubBuilding.m_material.SetFloat("_InvFade", backup_InvFade);
                    SubBuilding.m_mesh.colors = backup_meshColors;
                    SubBuilding.m_maxLodDistance = backup_maxLodDistance;
                    SubBuilding.m_minLodDistance = backup_maxRenderDistance;
                    return;

                case AssetType.Vehicle:
                    Vehicle.m_lodRenderDistance = backup_lodRenderDistance;
                    Vehicle.m_maxRenderDistance = backup_maxRenderDistance;
                    return;
            }
        }

        /// <summary>
        /// Applies cached render distance(s) as applicable to the asset type,
        /// unless the asset already defines larger value.
        /// </summary>
        /// <remarks>
        /// Lights are visible from a long distance, so it would be
        /// obvious if they suddenly disappear.
        /// </remarks>
        [SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements")]
        [SuppressMessage("Maintainability", "AV1536:Non-exhaustive switch statement requires a default case clause", Justification = "Intentional.")]
        public void ApplyCachedRenderDistance()
        {
            switch (TypeOfAsset)
            {
                case AssetType.Prop:
                    Prop.m_lodRenderDistance = Mathf.Max(Prop.m_lodRenderDistance, CachedRenderDistance);
                    Prop.m_maxRenderDistance = Mathf.Max(Prop.m_maxRenderDistance, CachedRenderDistance);
                    return;

                case AssetType.Building:
                    Building.m_maxLodDistance = Mathf.Max(Building.m_maxLodDistance, CachedRenderDistance);
                    Building.m_minLodDistance = Mathf.Max(Building.m_minLodDistance, CachedRenderDistance);
                    return;

                case AssetType.Container:
                    Building.m_maxPropDistance = Mathf.Max(Building.m_maxPropDistance, CachedRenderDistance);
                    return;

                case AssetType.SubBuilding:
                    SubBuilding.m_maxLodDistance = Mathf.Max(SubBuilding.m_maxLodDistance, CachedRenderDistance);
                    SubBuilding.m_minLodDistance = Mathf.Max(SubBuilding.m_minLodDistance, CachedRenderDistance);
                    return;

                case AssetType.Vehicle:
                    Vehicle.m_lodRenderDistance = Mathf.Max(Vehicle.m_lodRenderDistance, CachedRenderDistance);
                    Vehicle.m_maxRenderDistance = Mathf.Max(Vehicle.m_maxRenderDistance, CachedRenderDistance);
                    return;
            }
        }

        /// <summary>
        /// Returns an array filled with <see cref="Color.white"/>.
        /// </summary>
        /// <param name="count">The size of the array (number of mesh vertices).</param>
        /// <returns>An array of specified size filled with white color.</returns>
        [SuppressMessage("Member Design", "AV1130:Return type in method signature should be a collection interface instead of a concrete type", Justification = "Game API.")]
        private Color[] GetMeshColors(int count)
        {
            var colors = new Color[count];

            for (int index = 0; index < count; index++)
                colors[index] = MESH_COLOR;

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
