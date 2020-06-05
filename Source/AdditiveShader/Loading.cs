namespace AdditiveShader
{
    using AdditiveShader.Manager;
    using ICities;
    using JetBrains.Annotations;
    using UnityEngine;

    /// <summary>
    /// Loading extension for AdditiveShader mod.
    /// </summary>
    [UsedImplicitly]
    public class Loading : LoadingExtensionBase
    {
        private GameObject gameObject;

        /// <summary>
        /// If applicable <paramref name="mode"/>, creates a <see cref="GameObject"/> for the <see cref="AdditiveShaderManager"/>.
        /// </summary>
        /// <param name="mode">The <see cref="LoadMode"/> applicable to this loading extension.</param>
        [UsedImplicitly]
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            if (UserMod.IsEnabled && IsApplicable(mode))
            {
                Debug.Log($"[AdditiveShader] Initialising for LoadMode: {mode}");
                gameObject = new GameObject();
                gameObject.AddComponent<AdditiveShaderManager>();
            }
        }

        /// <summary>
        /// If it exists, destroys the <see cref="AdditiveShaderManager"/> and <see cref="GameObject"/>.
        /// </summary>
        [UsedImplicitly]
        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            if (!gameObject)
                return;

            Object.Destroy(gameObject);
            gameObject = null;
        }

        /// <summary>
        /// Determines whether the <paramref name="mode"/> is applicable to this mod.
        /// </summary>
        /// <param name="mode">The <see cref="LoadMode"/> applicable to this instance.</param>
        /// <returns>Returns <c>true</c> if applicable, otherwise <c>false</c>.</returns>
        private static bool IsApplicable(LoadMode mode) =>
            mode == LoadMode.NewGame ||
            mode == LoadMode.NewGameFromScenario ||
            mode == LoadMode.LoadGame;
    }
}
