namespace AdditiveShader
{
    using System.Diagnostics.CodeAnalysis;
    using ICities;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class Mod : IUserMod
    {
        /// <summary>
        /// Gets or sets a value indicating whether the mod is currently enabled.
        /// </summary>
        public static bool IsEnabled { get; set; }

        /// <summary>
        /// Gets name of the mod, which is shown in content manager and options.
        /// </summary>
        [UsedImplicitly]
        public string Name => "Additive Shader";

        /// <summary>
        /// Gets description of the mod, which is show in content manager.
        /// </summary>
        [UsedImplicitly]
        public string Description => "Allows time-of-day dependent use of additive shader on props and buildings.";

        /// <summary>
        /// Invoked by the game when the mod is enabled.
        /// </summary>
        [UsedImplicitly]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Game requirement.")]
        public void OnEnabled() => IsEnabled = true;

        /// <summary>
        /// Invoked by the game when the mod is disabled.
        /// </summary>
        [UsedImplicitly]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Game requirement.")]
        public void OnDisabled() => IsEnabled = false;
    }
}
