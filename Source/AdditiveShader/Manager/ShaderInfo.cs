namespace AdditiveShader.Manager
{
    using System;
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
        private const int TIME_ON   = 1;
        private const int TIME_OFF  = 2;
        private const int FADE      = 3;
        private const int INTENSITY = 4;

        // Hard-coded values derived from SimulationManager's `SUNRISE_HOUR` and `SUNSET_HOUR` members.
        // This is done because mods such as Real Time can alter those vanilla values. We need the
        // vanilla values as that's what most asset authors base their twilight shader on/off times on.
        private const float SUNRISE = 5f;
        private const float SUNSET = 20f;

        // A 1-hour boundary around the sunrise/sunset times. This is because asset authors tend to use
        // +/- 1 hour (of sunrise/sunset) for their on/off times. We can bucket all the 'twilight'
        // shader assets and spread them around the day/night transitions.
        private const float ONE_HOUR = 1.1f;
        private const float SUNRISE_START = SUNRISE - ONE_HOUR;
        private const float SUNRISE_END   = SUNRISE + ONE_HOUR;
        private const float SUNSET_START  = SUNSET  - ONE_HOUR;
        private const float SUNSET_END    = SUNSET  + ONE_HOUR;

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
            catch (Exception error)
            {
                throw new FormatException($"Invalid mesh name format: {meshName}", error);
            }

            IsAlwaysOn = OnTime == OffTime && OnTime >= 0f || OnTime == 0f && OffTime == 24f;

            IsStatic = IsAlwaysOn || OnTime < 0f; // -1 denotes always off (manual control, eg. via other mods)

            OverlapsMidnight = OnTime > OffTime;

            IsTwilight = OverlapsMidnight && TransitionsAtTwilight();
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
        /// <para>Gets a value indicating whether the shader turns on at dusk and off at dawn.</para>
        /// <para>
        /// In this situation, the shader is moved to a different list to allow batch toggling
        /// when day transitions to night and vice versa.
        /// </para>
        /// <para>
        /// As an added advantage, this also makes the shader synchronise with variable day
        /// lengths (and thus sun set/rise times) associated with the Real Time mod by dymanoid.
        /// </para>
        /// </summary>
        public bool IsTwilight { get; }

        /// <summary>
        /// Gets a value indicating whether the OnTime == OffTime (ie. the shader is always visible).
        /// </summary>
        public bool IsAlwaysOn { get; }

        /// <summary>
        /// Gets a value indicating whether the shader is static (always on, or always off).
        /// </summary>
        public bool IsStatic { get; }

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

        /// <summary>
        /// Given that the shader <see cref="OverlapsMidnight"/>, this does additional
        /// checks to see if the shader appears to be turning on at sunset and off at
        /// sunrise (based on vanilla times for those events).
        /// </summary>
        /// <returns>Returns <c>true</c> if the shader is probably a nightlight, otherwise <c>false</c>.</returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1131:Use readable conditions", Justification = "Common pattern.")]
        private bool TransitionsAtTwilight() =>
            SUNRISE_START < OnTime && OnTime < SUNRISE_END &&
            SUNSET_START < OffTime && OffTime < SUNSET_END &&
            !tags.Contains("not-twilight");
    }
}
