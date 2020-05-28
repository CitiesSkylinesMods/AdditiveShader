namespace AdditiveShader
{
    using ICities;
    using JetBrains.Annotations;

    public class Mod : IUserMod
    {
        [UsedImplicitly]
        public string Name => "Additive Shader";

        [UsedImplicitly]
        public string Description => "Allows time-of-day dependent use of additive shader on props and buildings.";

        /// <summary>
        /// Gets or sets a value indicating whether the mod is currently enabled.
        /// </summary>
        public static bool IsEnabled {get; set;}

        [UsedImplicitly]
        public void OnEnabled()
        {
            IsEnabled = true;
        }

        [UsedImplicitly]
        public void OnDisabled()
        {
            IsEnabled = false;
        }
    }
}
