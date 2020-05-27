using ICities;
using JetBrains.Annotations;
using UnityEngine;

namespace AdditiveShader
{
    public class Loading : LoadingExtensionBase
    {
        private GameObject gameobj;

        private bool IsApplicable(LoadMode mode)
        {
            return mode == LoadMode.NewGame
                || mode == LoadMode.LoadGame;
        }

        [UsedImplicitly]
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
        
            if (Mod.IsEnabled && IsApplicable(mode))
            {
                gameobj = new GameObject();
                gameobj.AddComponent<AdditiveShaderLogic>();
            }
        }

        [UsedImplicitly]
        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            if (gameobj != null)
            {
                UnityEngine.Object.Destroy(gameobj);
                gameobj = null;
            }
        }
    }
}
