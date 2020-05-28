namespace AdditiveShader.Manager
{
    using System;

    /// <summary>
    /// <para>This class is used to extract and parse parameters from a mesh name.</para>
    /// <para>
    /// It also derives whether the shader stays on across the midnight boundary,
    /// and whether it is always on.
    /// </para>
    /// </summary>
    public class ShaderInfo
    {
        /// <summary>
        /// Delimiter used for splitting the mesh name in to raw parameter array.
        /// </summary>
        private static readonly char[] DELIMITER = new[] { ' ' };

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderInfo"/> class
        /// by parsing the shader settings from the mesh name.
        /// </summary>
        /// <param name="meshName"><para>The mesh name for the asset that contains the additive shader.</para>
        /// <para>
        /// Must be in the following format: <code>"AdditiveShader On Off Fade Intensity"</code>
        /// as defined by <see cref="https://cslmodding.info/mod/additive-shader/"/>.
        /// </para>
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="meshName"/> is <c>null</c>.</exception>
        public ShaderInfo(string meshName)
        {
            if (meshName == null)
                throw new ArgumentNullException(nameof(meshName));

            // TODO: Would be nice if tags (folksonomy) could be added to allow other mods to toggle shaders on/off.
            Raw = meshName.Split(DELIMITER, StringSplitOptions.RemoveEmptyEntries);

            OnTime    = float.Parse(Raw[1]);
            OffTime   = float.Parse(Raw[2]);
            Fade      = float.Parse(Raw[3]);
            Intensity = float.Parse(Raw[4]);

            AlwaysOn = OnTime == OffTime;

            OverlapsMidnight = OnTime > OffTime;
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"ShaderInfo('{Raw[0]} {Raw[1]} {Raw[2]} {Raw[3]} {Raw[4]}'), AlwaysOn: {AlwaysOn}, OverlapsMidnight: {OverlapsMidnight}";

        /// <summary>
        /// The raw shader parameters split from the mesh name.
        /// </summary>
        private readonly string[] Raw;

        /// <summary>
        /// Gets a value which determines what game time the shader is made visible.
        /// </summary>
        public float OnTime { get; private set; }

        /// <summary>
        /// Gets a value which determines what game time the shader is made invisible.
        /// </summary>
        public float OffTime { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the OnTime > OffTime (ie. the shader is visible across the midnight boundary).
        /// </summary>
        public bool OverlapsMidnight { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the OnTime == OffTime (ie. the shader is always visible).
        /// </summary>
        public bool AlwaysOn { get; private set; }

        /// <summary>
        /// Gets a value which controls fading of the additive shader.
        ///
        /// The additive shader decreases opacity as it gets closer to other objects.
        /// Higher values make the fading less noticeable.
        /// </summary>
        public float Fade { get; private set; }

        /// <summary>
        /// Gets a value indicating the light intensity multiplier to apply to the additive shader.
        ///
        /// Values above 1 may start to bloom.
        /// </summary>
        public float Intensity { get; private set; }
    }
}
