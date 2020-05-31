namespace AdditiveShader.Manager
{
    using System.Collections.Generic;
    using ColossalFramework;
    using JetBrains.Annotations;
    using UnityEngine;

    /// <summary>
    /// <para>Attached to a game object, this manager will perform the following tasks:
    /// <list type="bullet">
    /// <item><term>Start</term> collate a list of all assets using the additive shader.</item>
    /// <item><term>Update</term> set visibility of the shaders based on their parameters.</item>
    /// </list>
    /// </para>
    /// </summary>
    public class AdditiveShaderManager : MonoBehaviour
    {
        /// <summary>
        /// A list of shader assets that are static (always on/off).
        /// </summary>
        private List<ShaderAsset> staticShaders;

        /// <summary>
        /// A list of shader assets that turn on at dusk, off at dawn.
        /// </summary>
        private List<ShaderAsset> twilightShaders;

        /// <summary>
        /// A list of other (non-twilight) time-based shader assets.
        /// </summary>
        private List<ShaderAsset> generalShaders;

        /// <summary>
        /// Used to spot twilight transitions.
        /// </summary>
        private bool isNightTime;

        /// <summary>
        /// <para>Indicates the active list for <see cref="Update()"/>.</para>
        /// <list type="bullet">
        /// <item><c>true</c> -- <see cref="twilightShaders"/></item>
        /// <item><c>false</c> -- <see cref="generalShaders"/></item>
        /// </list>
        /// </summary>
        private bool iterateTwilight;

        /// <summary>
        /// Current item index in active list.
        /// </summary>
        private int index = int.MaxValue - 1;

        /// <summary>
        /// Number of items in the active list.
        /// </summary>
        private int count;

        /// <summary>
        /// Collates list of assets that use the additive shader, sorts them in to lists.
        /// </summary>
        [UsedImplicitly]
        protected void Start()
        {
            enabled = false;

            var report    = new AssetReporter();
            var assetList = AssetScanner.ListShaderAssets();

            InitialiseShaderLists(assetList.Count);

            foreach (var shader in assetList)
            {
                (shader.Info.IsStatic ? staticShaders : shader.Info.IsTwilight ? twilightShaders : generalShaders).Add(shader);

                report.Append(shader);
            }

            TrimExcessCapacity();

            report.Summary(assetList.Count, twilightShaders.Count, generalShaders.Count);
            report.PublishToLogFile();

            enabled = twilightShaders.Count != 0 || generalShaders.Count != 0;
        }

        /// <summary>
        /// <para>Sets visibility of the additive shaders depending on game time.</para>
        /// <para>PERFORMANCE CRITICAL.</para>
        /// </summary>
        [UsedImplicitly]
        protected void Update()
        {
            if (++index >= count && !IsReadyForNextUpdate())
                return;

            if (iterateTwilight)
            {
                twilightShaders[index].SetVisible(isNightTime);
            }
            else
            {
                generalShaders[index].SetVisible(Singleton<SimulationManager>.instance.m_currentDayTimeHour);
            }
        }

        /// <summary>
        /// Resumes <see cref="Update()"/> cycle after being paused.
        /// </summary>
        /// <returns>Returns <c>true</c>.</returns>
        protected bool ResumeUpdates() => enabled = true;

        /// <summary>
        /// If the active list is empty, pause updates for a short time before trying again.
        /// </summary>
        /// <returns>Returns <c>false</c>.</returns>
        private bool PauseUpdates()
        {
            Invoke(nameof(ResumeUpdates), iterateTwilight ? 1.0f : 5.0f);
            return enabled = false;
        }

        /// <summary>
        /// Determines active list to use for <see cref="Update()"/>:
        /// <list type="bullet">
        /// <item><see cref="twilightShaders"/> - if we just transitioned twilight</item>
        /// <item><see cref="generalShaders"/> - at all other times</item>
        /// </list>
        /// </summary>
        /// <returns>Returns <c>true</c> if ready for next update, otherwise <c>false</c>.</returns>
        private bool IsReadyForNextUpdate()
        {
            if (iterateTwilight = Singleton<SimulationManager>.instance.m_isNightTime != isNightTime)
                isNightTime = !isNightTime;

            index = 0;
            count = (iterateTwilight ? twilightShaders : generalShaders).Count;

            return count == 0
                ? PauseUpdates()
                : ResumeUpdates();
        }

        /// <summary>
        /// Initialises shader lists:
        /// <list type="bullet">
        /// <item><see cref="staticShaders"/> -- always on/off</item>
        /// <item><see cref="twilightShaders"/> -- on at dusk, off at dawn</item>
        /// <item><see cref="generalShaders"/> -- other time-based on/off</item>
        /// </list>
        /// </summary>
        /// <param name="capacity">The initial capacity for the lists.</param>
        private void InitialiseShaderLists(int capacity)
        {
            staticShaders = new List<ShaderAsset>(capacity);
            twilightShaders = new List<ShaderAsset>(capacity);
            generalShaders = new List<ShaderAsset>(capacity);
        }

        /// <summary>
        /// Trims excess capacity from populated lists.
        /// </summary>
        private void TrimExcessCapacity()
        {
            staticShaders.TrimExcess();
            twilightShaders.TrimExcess();
            generalShaders.TrimExcess();
        }
     }
}
