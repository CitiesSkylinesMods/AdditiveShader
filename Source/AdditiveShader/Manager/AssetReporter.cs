namespace AdditiveShader.Manager
{
    using System.Text;
    using UnityEngine;

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
                .Append(", Alwyas Off: ").Append(shader.Info.IsStatic && !shader.Info.IsAlwaysOn)
                .Append(", Overlaps Midnight: ").Append(shader.Info.OverlapsMidnight)
                .Append(", Twilight: ").Append(shader.Info.IsToggledByTwilight)
                .Append(", Day Time: ").Append(shader.Info.IsDayTimeOnly)
                .Append(", Night Time: ").Append(shader.Info.IsNightTimeOnly)
                .AppendLine()
                .AppendLine();

        /// <summary>
        /// Add a summary to the report.
        /// </summary>
        /// <param name="countAll">Total number of shader-usuing assets.</param>
        /// <param name="countTwilight">Number of twilight time-based shaders.</param>
        /// <param name="countGeneral">Number of general time-based shaders.</param>
        internal void Summary(int countAll, int countTwilight, int countGeneral) =>
            report
                .AppendLine()
                .Append("Found ").Append(countAll).Append(" assets")
                .Append(" (static: ").Append(countAll - countTwilight - countGeneral)
                .Append(", twilight: ").Append(countTwilight)
                .Append(", others: ").Append(countGeneral).Append(" ).");

        /// <summary>
        /// Publish report to the game log file.
        /// </summary>
        internal void PublishToLogFile()
            => Debug.Log(report.ToString());
    }
}
