namespace AdditiveShader.Manager
{
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// The <see cref="AssetReporter"/> is used by <see cref="AdditiveShaderManager"/> to log
    /// details of assets collated from the <see cref="AssetScanner"/>.
    /// </summary>
    public class AssetReporter
    {
        private readonly StringBuilder report;

        private readonly Stopwatch timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetReporter"/> class.
        /// </summary>
        internal AssetReporter()
        {
            timer = Stopwatch.StartNew();

            report = new StringBuilder(2048);

            report
                .AppendLine("[AdditiveShader] Assets using shader:")
                .AppendLine();
        }

        /// <summary>
        /// Adds details of an asset to the report.
        /// </summary>
        /// <param name="shader">The shader-using asset to report on.</param>
        internal void Append(ShaderAsset shader) =>
            report
                .Append(shader).AppendLine(":")
                .Append("- ").Append(shader.Info)
                .AppendLine()
                .Append("- Static: ").Append(shader.Info.IsStatic)
                .Append(", Always On: ").Append(shader.Info.IsAlwaysOn)
                .Append(", Remote Control: ").Append(shader.Info.IsRemotelyControlled)
                .Append(", Overlap Midnight: ").Append(shader.Info.OverlapsMidnight)
                .Append(", Day Time: ").Append(shader.Info.IsDayTimeOnly)
                .Append(", Night Time: ").Append(shader.Info.IsNightTimeOnly)
                .Append(", Twilight: ").Append(shader.Info.IsToggledByTwilight)
                .AppendLine()
                .AppendLine();

        /// <summary>
        /// Add a summary to the report.
        /// </summary>
        /// <param name="countAll">Total number of shader-usuing assets.</param>
        /// <param name="countRemoteControl">Number of remote-control shaders.</param>
        /// <param name="countTwilight">Number of twilight time-based shaders.</param>
        /// <param name="countGeneral">Number of general time-based shaders.</param>
        internal void Summary(int countAll, int countRemoteControl, int countTwilight, int countGeneral) =>
            report
                .Append("Scanned ").Append(AssetScanner.ItemsScanned).Append(" assets ")
                .Append("in ").Append(timer.ElapsedMilliseconds).Append("ms: ")
                .Append("Found ").Append(countAll).Append(" additive shaders")
                .Append(" (static: ").Append(countAll - countRemoteControl - countTwilight - countGeneral)
                .Append(", remote control: ").Append(countRemoteControl)
                .Append(", day/night: ").Append(countTwilight)
                .Append(", others: ").Append(countGeneral).Append(" ) ");

        /// <summary>
        /// Publish report to the game log file.
        /// </summary>
        internal void PublishToLogFile()
            => UnityEngine.Debug.Log(report.ToString());
    }
}
