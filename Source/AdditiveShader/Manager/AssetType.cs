namespace AdditiveShader.Manager
{
    /// <summary>
    /// The type of asset associated with an <see cref="ShaderAsset"/> instance.
    /// </summary>
    public enum AssetType
    {
        /// <summary>
        /// Denotes an invalid asset type. Should never happen.
        /// </summary>
        None = 0,

        /// <summary>
        /// A <see cref="PropInfo"/> asset.
        /// </summary>
        Prop = 1,

        /// <summary>
        /// A <see cref="BuildingInfo"/> asset.
        /// </summary>
        Building = 1 << 1,

        /// <summary>
        /// A <see cref="BuildingInfoSub"/> asset.
        /// </summary>
        SubBuilding = 1 << 2,

        /// <summary>
        /// A <see cref="VehicleInfoSub"/> asset.
        /// </summary>
        Vehicle = 1 << 3,

        /// <summary>
        /// A <see cref="BuildingInfo"/> asset which contains
        /// a shader-using <see cref="PropInfo"/> asset.
        /// </summary>
        Container = 1 << 4,
    }
}
