namespace AdditiveShader.Manager
{
    using System.Diagnostics;
    using System.Text;
    using AdditiveShader.ExtensionMethods;

    /// <summary>
    /// The <see cref="AssetReporter"/> is used by <see cref="AdditiveShaderManager"/> to log
    /// details of assets collated from the <see cref="AssetScanner"/>.
    /// </summary>
    public class AssetReporter
    {
        private const string YES = "Yes";

        private const string NO  = "-";

        private readonly StringBuilder report;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetReporter"/> class.
        /// </summary>
        internal AssetReporter()
        {
            report = new StringBuilder(2048);

            report
                .AppendLine("[AdditiveShader] Assets Report:")
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
                .AppendRightPad(shader.Info.IsStatic             ? YES : NO, 7)
                .AppendRightPad(shader.Info.IsAlwaysOn           ? YES : NO, 9)
                .AppendRightPad(shader.Info.IsRemotelyControlled ? YES : NO, 7)
                .AppendRightPad(shader.Info.OverlapsMidnight     ? YES : NO, 8)
                .AppendRightPad(shader.Info.IsDayTimeOnly        ? YES : NO, 8)
                .AppendRightPad(shader.Info.IsNightTimeOnly      ? YES : NO, 10)
                .AppendRightPad(shader.Info.IsToggledByTwilight  ? YES : NO, 9)
                .Append(shader)
                .Append(" '").Append(shader.Info).AppendLine("'");

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
