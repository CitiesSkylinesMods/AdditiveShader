namespace AdditiveShader.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

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
        // original values as that's what most asset authors base their twilight shader on/off times on.
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
        /// Named shader profiles.
        /// </summary>
        private static readonly Dictionary<string, ShaderInfo> Profiles;

        /// <summary>
        /// The original, unaltered mesh name.
        /// </summary>
        private readonly string meshName;

        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1001:Commas should be spaced correctly")]
        [SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline")]
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly")]
        static ShaderInfo()
        {
            Profiles = new Dictionary<string, ShaderInfo>
            {
                // Profile                        On       Off      RC     Always Static Mid    Twi    Day    Night
                { "AlwaysOn"     , new ShaderInfo(0f     , 24f    , false, true , true , false, false, false, false) },
                { "RemoteControl", new ShaderInfo(-1     , -1     , true , false, true , false, false, false, false) },
                { "DayTime"      , new ShaderInfo(SUNRISE, SUNSET , false, false, false, false, true , true , false) },
                { "NightTime"    , new ShaderInfo(SUNSET , SUNRISE, false, false, false, true , true , false, true ) },
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderInfo"/> class
        /// by parsing the shader settings from the mesh name.
        /// </summary>
        /// <param name="rawMeshName">The mesh name for the asset that contains the additive shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="rawMeshName"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">Thrown if <paramref name="rawMeshName"/> format is invalid.</exception>
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly")]
        [SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "Parsing tags.")]
        [SuppressMessage("Style", "IDE0008:Use explicit type", Justification = "Can only ever be string[] (game api).")]
        public ShaderInfo(string rawMeshName, string assetType)
        {
            try
            {
                meshName = rawMeshName ?? throw new ArgumentNullException(nameof(rawMeshName));

                var tags = rawMeshName.Split(DELIMITER, StringSplitOptions.RemoveEmptyEntries);
                HashTags = new HashSet<string>(tags) { assetType };

                // AdditiveShader Keyword fade intensity [tags ...]
                if (Profiles.TryGetValue(tags[1], out var profile))
                {
                    OnTime  = profile.OnTime;
                    OffTime = profile.OffTime;

                    IsRemotelyControlled = profile.IsRemotelyControlled;

                    IsAlwaysOn = profile.IsAlwaysOn;
                    IsStatic   = profile.IsStatic;

                    OverlapsMidnight    = profile.OverlapsMidnight;
                    IsToggledByTwilight = profile.IsToggledByTwilight;

                    IsNightTimeOnly = profile.IsNightTimeOnly;
                    IsDayTimeOnly   = profile.IsDayTimeOnly;

                    Fade      = float.Parse(tags[2]);
                    Intensity = float.Parse(tags[3]);
                }

                // AdditiveShader on off fade intensity [tags ...]
                else
                {
                    OnTime = float.Parse(tags[1]);
                    OffTime = float.Parse(tags[2]);

                    IsRemotelyControlled = OnTime < 0f;

                    IsAlwaysOn = !IsRemotelyControlled &&
                                 (OnTime == OffTime || OnTime == 0f && OffTime == 24f);

                    IsStatic = IsAlwaysOn || IsRemotelyControlled;

                    OverlapsMidnight    = OnTime > OffTime;
                    IsToggledByTwilight = OverlapsMidnight && OnAtDuskOffAtDawn();

                    IsNightTimeOnly = IsToggledByTwilight;
                    IsDayTimeOnly   = false; // todo: calc

                    Fade      = float.Parse(tags[3]);
                    Intensity = float.Parse(tags[4]);
                }
            }
            catch (Exception error)
            {
                throw new FormatException($"Invalid mesh name format: {rawMeshName}", error);
            }
        }

        // Internal use only - used to build profiles to reduce cruft in main constructor
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:Parameters should be on same line or separate lines")]
        [SuppressMessage("Maintainability", "AV1561:Signature contains too many parameters")]
        private ShaderInfo(
            float onTime, float offTime,
            bool isRemote,
            bool isAlwaysOn, bool isStatic,
            bool overlapsMidnight, bool isTwilight,
            bool isDayOnly, bool isNightOnly)
        {
            OnTime  = onTime;
            OffTime = offTime;

            IsRemotelyControlled = isRemote;

            IsAlwaysOn = isAlwaysOn;
            IsStatic   = isStatic;

            OverlapsMidnight    = overlapsMidnight;
            IsToggledByTwilight = isTwilight;

            IsDayTimeOnly   = isDayOnly;
            IsNightTimeOnly = isNightOnly;
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
        /// Gets a value indicating whether the shader is permanently visible.
        /// </summary>
        public bool IsAlwaysOn { get; }

        /// <summary>
        /// Gets a value indicating whether the shader is remotely controlled by other mods.
        /// </summary>
        public bool IsRemotelyControlled { get; }

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
        /// Gets a list of tags.
        /// </summary>
        public HashSet<string> HashTags { get; }

        /// <inheritdoc/>
        public override string ToString() => $"ShaderInfo('{meshName}')";

        /// <summary>
        /// Given that the shader <see cref="OverlapsMidnight"/>, this checks
        /// whether it turns on at dusk and off at dawn (based on vanilla times).
        /// </summary>
        /// <returns>Returns <c>true</c> if on at dusk off at dawn, otherwise <c>false</c>.</returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1131:Use readable conditions", Justification = "Common pattern.")]
        private bool OnAtDuskOffAtDawn() =>
            SUNSET_START  <  OnTime && OnTime  <  SUNSET_END &&
            SUNRISE_START < OffTime && OffTime < SUNRISE_END &&
            !HashTags.Contains("not-twilight");
    }
}
