namespace AdditiveShader
{
    public enum AssetType : ushort
    {
        None            = 0,
        Prop            = 1,
        Building        = 1 << 1,
        BuildingSubMesh = 1 << 2,
        VehicleSubMesh  = 1 << 3,
    }
}
