namespace AdditiveShader.ExtensionMethods
{
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// Extensions for <see cref="StringBuilder"/> class.
    /// </summary>
    public static class StringBuilderExtensions
    {
        /*
        |       Method |     Mean |   Error |  StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
        |------------- |---------:|--------:|--------:|------:|-------:|------:|------:|----------:|
        |  Aubergine18 | 450.2 ns | 2.87 ns | 2.69 ns |  1.00 | 0.1197 |     - |     - |     376 B |
        | StringFormat | 384.8 ns | 1.72 ns | 1.61 ns |  0.85 | 0.0305 |     - |     - |      96 B |
        |       Manual | 141.7 ns | 0.64 ns | 0.60 ns |  0.31 |      - |     - |     - |         - |

        Aubergine18 approach:

        .AppendFormat(
                    "{0} {1} {2} {3} {4} {5} {6} {7} '{8}'",
                    (shader.Info.IsStatic ? "Yes" : "-").PadRight(6), ...

        StringFormat approach:

        AppendFormat("{0,-6} {1,-8} {2,-6} {3,-7} {4,-7} {5,-9} {6,-8} {7} '{8}'",  ... )

        Manual approach:
        */

        /// <summary>
        /// Adds an <c>.AppendRightPadded(text, pading)</c> extension to <see cref="StringBuilder"/>.
        /// </summary>
        /// <remarks>Code originally by dymanoid.</remarks>
        /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
        /// <param name="text">The text to append.</param>
        /// <param name="width">Right padding will be added if necessary to achieve the specified width.</param>
        /// <returns>Returns <paramref name="sb"/>.</returns>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public static StringBuilder AppendRightPad(this StringBuilder sb, string text, int width) =>
            sb.Append(text).Append(' ', width - text.Length);
    }
}
