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
        /// <param name="meshName">The mesh name for the asset that contains the additive shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="meshName"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">Thrown if <paramref name="meshName"/> format is invalid.</exception>
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly")]
        [SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "Parsing tags.")]
        public ShaderInfo(string meshName)
        {
            try
            {
                name = meshName ?? throw new ArgumentNullException(nameof(meshName));
                tags = meshName.Split(DELIMITER, StringSplitOptions.RemoveEmptyEntries);

                // AdditiveShader on off fade intensity [tags ...]
                // AdditiveShader Keyword fade intensity [tags ...]
                switch (tags[1])
                {
                    case "AlwaysOn":
                        OnTime = 0f;
                        OffTime = 24f;

                        IsAlwaysOn = true;
                        IsStatic = true;
                        OverlapsMidnight = true;

                        Fade = float.Parse(tags[2]);
                        Intensity = float.Parse(tags[3]);
                        break;

                    case "AlwaysOff":
                        OnTime = -1f;
                        OffTime = -1f;

                        IsStatic = true;

                        Fade = float.Parse(tags[2]);
                        Intensity = float.Parse(tags[3]);
                        break;

                    case "DayTime":
                        OnTime = SUNRISE;
                        OffTime = SUNSET;

                        IsToggledByTwilight = true;
                        IsDayTimeOnly = true;

                        Fade = float.Parse(tags[2]);
                        Intensity = float.Parse(tags[3]);
                        break;

                    case "NightTime":
                        OnTime = SUNSET;
                        OffTime = SUNRISE;

                        IsToggledByTwilight = true;
                        IsNightTimeOnly = true;
                        OverlapsMidnight = true;

                        Fade = float.Parse(tags[2]);
                        Intensity = float.Parse(tags[3]);
                        break;

                    default:
                        OnTime = float.Parse(tags[1]);
                        OffTime = float.Parse(tags[2]);

                        IsAlwaysOn = OnTime == OffTime && OnTime >= 0f
                                  || OnTime == 0f && OffTime == 24f;

                        IsStatic = IsAlwaysOn || OnTime < 0f;

                        OverlapsMidnight    = OnTime > OffTime;
                        IsToggledByTwilight = OverlapsMidnight && OnAtDuskOffAtDawn();
                        IsNightTimeOnly     = IsToggledByTwilight;

                        Fade = float.Parse(tags[3]);
                        Intensity = float.Parse(tags[4]);
                        break;
                }
            }
            catch (Exception error)
            {
                throw new FormatException($"Invalid mesh name format: {meshName}", error);
            }
        }

        /// <summary>
        /// <para>Gets a value defining the game time at which shader is shown.</para>
        /// <para>Note: Will be negative if <c>AlwaysOff</c> keyword was used.</para>
        /// </summary>
        public float OnTime { get; }

        /// <summary>
        /// Gets a value defining the game time at which shader is hidden.
        /// </summary>
        public float OffTime { get; }

        /// <summary>
        /// Gets a value indicating whether OnTime > OffTime (ie. the shader is visible across the midnight boundary).
        /// </summary>
        public bool OverlapsMidnight { get; }

        /// <summary>
        /// <para>Gets a value indicating whether the shader is toggled by day/night boundary.</para>
        /// <para>One of <see cref="IsDayTimeOnly"/> or <see cref="IsNightTimeOnly"/> will be <c>true</c>.</para>
        /// </summary>
        public bool IsToggledByTwilight { get; }

        /// <summary>
        /// <para>Gets a value indicating whether the shader is on all day _and_ off all night.</para>
        /// <para>Note: This is determined by the <c>DayTime</c> keyword, not on/off times.</para>
        /// </summary>
        public bool IsDayTimeOnly { get; }

        /// <summary>
        /// <para>Gets a value indicating whether the shader is on all night _and_ off all day.</para>
        /// <para>Note: This is determined by either the <c>NightTime</c> keyword, or on/off times which occur during twilight.</para>
        /// </summary>
        public bool IsNightTimeOnly { get; }

        /// <summary>
        /// Gets a value indicating whether the OnTime == OffTime (ie. the shader is always visible).
        /// </summary>
        public bool IsAlwaysOn { get; }

        /// <summary>
        /// <para>Gets a value indicating whether the shader is static (always on, or always off).</para>
        /// <para>Note: If <c>true</c>, and <see cref="IsAlwaysOn"/> is <c>false</c>, it means "always off".</para>
        /// </summary>
        public bool IsStatic { get; }

        /// <summary>
        /// <para>Gets a value which controls fading of the additive shader.</para>
        /// <para>
        /// The additive shader decreases opacity as it gets closer to other objects.
        /// Higher value means less fading, because reasons.
        /// </para>
        /// </summary>
        public float Fade { get; }

        /// <summary>
        /// <para>Gets a value indicating the light intensity multiplier to apply to the additive shader.</para>
        /// <para>Values above 1 may start to bloom.</para>
        /// </summary>
        public float Intensity { get; }

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
        /// Given that the shader <see cref="OverlapsMidnight"/>, this checks
        /// whether it turns on at dusk and off at dawn (based on vanilla times).
        /// </summary>
        /// <returns>Returns <c>true</c> if on at dusk off at dawn, otherwise <c>false</c>.</returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1131:Use readable conditions", Justification = "Common pattern.")]
        private bool OnAtDuskOffAtDawn() =>
            SUNRISE_START < OffTime && OffTime < SUNRISE_END &&
            SUNSET_START  < OnTime  && OnTime  < SUNSET_END  &&
            !tags.Contains("not-twilight");
    }
}
