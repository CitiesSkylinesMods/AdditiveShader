namespace AdditiveShader.Manager
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// <para>This class is used to extract and parse parameters from a mesh name.</para>
    /// <para>
    /// It also derives whether the shader stays on across the midnight boundary,
    /// and whether it is always on.
    /// </para>
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore")]
    public class ShaderInfo
    {
        // Indexes of the parameters stored in tags array
        // Note: Tag 0 is always "AdditiveShader"
        private const int TIME_ON = 1;
        private const int TIME_OFF = 2;
        private const int FADE = 3;
        private const int INTENSITY = 4;

        /// <summary>
        /// Delimiter used for splitting the mesh name in to raw parameter array.
        /// </summary>
        private static readonly char[] DELIMITER = new[] { ' ' };

        /// <summary>
        /// The original, unaltered mesh name.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// The raw shader parameters split from the mesh name.
        /// </summary>
        private readonly string[] tags;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderInfo"/> class
        /// by parsing the shader settings from the mesh name.
        /// </summary>
        /// <param name="meshName"><para>The mesh name for the asset that contains the additive shader.</para>
        /// <para>
        /// Must be in the following format: <code>"AdditiveShader On Off Fade Intensity"</code>
        /// as defined by <see cref="https://cslmodding.info/mod/additive-shader/"/>.
        /// </para>
        /// <para>Tags can optionally be appeneded to the end, each preceded by a space and in lowercase text.</para>
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="meshName"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">Thrown if <paramref name="meshName"/> format is invalid.</exception>
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly")]
        public ShaderInfo(string meshName)
        {
            name = meshName ?? throw new ArgumentNullException(nameof(meshName));
            tags = meshName.Split(DELIMITER, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                OnTime    = float.Parse(tags[TIME_ON  ]);
                OffTime   = float.Parse(tags[TIME_OFF ]);
                Fade      = float.Parse(tags[FADE     ]);
                Intensity = float.Parse(tags[INTENSITY]);
            }
            catch (Exception e)
            {
                throw new FormatException("Invalid mesh name format", e);
            }

            AlwaysOn = OnTime == OffTime && OnTime >= 0f || OnTime == 0f && OffTime == 24f;

            Static = AlwaysOn || OnTime < 0f; // -1 denotes always off (manual control, eg. via other mods)

            OverlapsMidnight = OnTime > OffTime;
        }

        /// <summary>
        /// Gets a value which determines what game time the shader is made visible.
        /// </summary>
        public float OnTime { get; }

        /// <summary>
        /// Gets a value which determines what game time the shader is made invisible.
        /// </summary>
        public float OffTime { get; }

        /// <summary>
        /// Gets a value indicating whether the OnTime > OffTime (ie. the shader is visible across the midnight boundary).
        /// </summary>
        public bool OverlapsMidnight { get; }

        /// <summary>
        /// Gets a value indicating whether the OnTime == OffTime (ie. the shader is always visible).
        /// </summary>
        public bool AlwaysOn { get; }

        /// <summary>
        /// Gets a value indicating whether the shader is static (always on, or always off).
        /// </summary>
        public bool Static { get; }

        /// <summary>
        /// <para>Gets a value which controls fading of the additive shader.</para>
        /// <para>
        /// The additive shader decreases opacity as it gets closer to other objects.
        /// Higher values make the fading less noticeable.
        /// </para>
        /// </summary>
        public float Fade { get; }

        /// <summary>
        /// <para>Gets a value indicating the light intensity multiplier to apply to the additive shader.</para>
        /// <para>Values above 1 may start to bloom.</para>
        /// </summary>
        public float Intensity { get; }

        /// <summary>
        /// <para>Gets a value containing tags associated with the asset.</para>
        /// <para>This can be used by mods to toggle shaders for those assets on/off.</para>
        /// </summary>
        public string Tags { get; }

        /// <summary>
        /// Check if a tag is defined. Case sensitive. Tags should always be lower case.
        /// </summary>
        /// <param name="tag">The tag to check.</param>
        /// <returns>Returns <c>true</c> if tag found, otherwise <c>false</c>.</returns>
        public bool HasTag(string tag) =>
            tags.Contains(tag);

        /// <inheritdoc/>
        public override string ToString() =>
            $"ShaderInfo('{name}')";
    }
}
