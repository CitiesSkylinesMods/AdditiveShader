namespace AdditiveShader.Manager
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// The <see cref="AssetReporter"/> is used by <see cref="AdditiveShaderManager"/> to log
    /// details of assets collated from the <see cref="AssetScanner"/>.
    /// </summary>
    public class AssetReporter
    {
        private readonly StringBuilder report;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetReporter"/> class.
        /// </summary>
        internal AssetReporter()
        {
            report = new StringBuilder(2048);

            report
                .AppendLine("[AdditiveShader] Assets using shader:")
                .AppendLine()
                .AppendLine("Static AlwaysOn Remote OverMid DayOnly NightOnly Twilight Type            Name 'm_mesh.name'")
                .Append('-', 100)
                .AppendLine();
        }

        /// <summary>
        /// <para>Gets or sets the report timer.</para>
        /// <para>The timer is initialised by <see cref="AdditiveShaderManager.Start()"/>.</para>
        /// </summary>
        public static Stopwatch Timer { get; set; }

        /// <summary>
        /// Adds details of an asset to the report.
        /// </summary>
        /// <param name="shader">The shader-using asset to report on.</param>
        internal void Append(ShaderAsset shader) =>
            report
                .AppendFormat(
                    "{0} {1} {2} {3} {4} {5} {6} {7} '{8}'",
                    (shader.Info.IsStatic             ? "Yes" : "-").PadRight(6),
                    (shader.Info.IsAlwaysOn           ? "Yes" : "-").PadRight(8),
                    (shader.Info.IsRemotelyControlled ? "Yes" : "-").PadRight(6),
                    (shader.Info.OverlapsMidnight     ? "Yes" : "-").PadRight(7),
                    (shader.Info.IsDayTimeOnly        ? "Yes" : "-").PadRight(7),
                    (shader.Info.IsNightTimeOnly      ? "Yes" : "-").PadRight(9),
                    (shader.Info.IsToggledByTwilight  ? "Yes" : "-").PadRight(8),
                    shader,
                    shader.Info)
                .AppendLine();

        /// <summary>
        /// Add a summary to the report.
        /// </summary>
        /// <param name="countAll">Total number of shader-usuing assets.</param>
        /// <param name="countTwilight">Number of twilight time-based shaders.</param>
        /// <param name="countTimeBased">Number of general time-based shaders.</param>
        internal void Summary(int countAll, int countTwilight, int countTimeBased) =>
            report
                .AppendLine()
                .Append("Scanned ").Append(AssetScanner.ItemsScanned).Append(" assets ")
                .Append("in ").Append(Timer.ElapsedMilliseconds).Append("ms: ")
                .Append("Found ").Append(countAll).Append(" additive shaders (")
                .Append(countTwilight).Append(" twilight-toggled, ")
                .Append(countTimeBased).Append(" other time-based)");

        /// <summary>
        /// Publish report to the game log file.
        /// </summary>
        internal void PublishToLogFile()
            => UnityEngine.Debug.Log(report.ToString());
    }
}
