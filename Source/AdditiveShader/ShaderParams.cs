namespace AdditiveShader
{
    using System;

    /// <summary>
    /// This class is used to extract and parse parameters from a mesh name.
    ///
    /// It also derives whether the shader stays on across the midnight boundary,
    /// and whether it is always on.
    /// </summary>
    public class ShaderParams
    {
        /// <summary>
        /// Delimiter used for splitting the mesh name in to raw parameter array.
        /// </summary>
        private static readonly char[] DELIMITER = new[] { ' ' };

        public ShaderParams(string meshName)
        {
            string[] raw = meshName.Split(DELIMITER, StringSplitOptions.RemoveEmptyEntries);

            OnTime    = float.Parse(raw[1]);
            OffTime   = float.Parse(raw[2]);
            Fade      = float.Parse(raw[3]);
            Intensity = float.Parse(raw[4]);

            // TODO: Would be nice if tags (folksonomy) could be added to allow other mods to toggle shaders on/off.

            AlwaysOn = OnTime == OffTime;

            OverlapsMidnight = OnTime > OffTime;
        }

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
