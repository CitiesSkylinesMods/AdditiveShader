namespace AdditiveShader
{
    using AdditiveShader.Manager;
    using ICities;
    using JetBrains.Annotations;
    using UnityEngine;

    /// <summary>
    /// Loading extension for AdditiveShader mod:
    /// <list type="bullet">
    /// <item><term>OnLevelLoaded</term> if applicable <see cref="LoadMode"/>, creates a <see cref="GameObject"/> for the <see cref="AdditiveShaderManager"/>.</item>
    /// <item><term>OnLevelUnloading</term> destroys the <see cref="AdditiveShaderManager"/> <see cref="GameObject"/>, if it exists.</item>
    /// </list>
    /// </summary>
    [UsedImplicitly]
    public class Loading : LoadingExtensionBase
    {
        private GameObject gameobj;

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
                gameobj = new GameObject();
                gameobj.AddComponent<AdditiveShaderManager>();
            }
        }

        /// <summary>
        /// If it exists, destroys the <see cref="AdditiveShaderManager"/> <see cref="GameObject"/>.
        /// </summary>
        [UsedImplicitly]
        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            if (gameobj != null)
            {
                Object.Destroy(gameobj);
                gameobj = null;
            }
        }

        /// <summary>
        /// Determines whether the <paramref name="mode"/> is applicable to this mod.
        /// </summary>
        /// <param name="mode">The <see cref="LoadMode"/> applicable to this loading extension.</param>
        /// <returns>Returns <c>true</c> if the mod is applicable, otherwise <c>false</c>.</returns>
        private bool IsApplicable(LoadMode mode) =>
            mode == LoadMode.NewGame ||
            mode == LoadMode.LoadGame;
    }
}
