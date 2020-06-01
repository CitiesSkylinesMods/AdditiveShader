namespace AdditiveShader.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using ColossalFramework;
    using JetBrains.Annotations;
    using UnityEngine;

    /// <summary>
    /// <para>Attached to a game object, this manager will perform the following tasks:
    /// <list type="bullet">
    /// <item><term>Start</term> collate a lists of assets using the additive shader.</item>
    /// <item><term>Update</term> set visibility of time-based shaders.</item>
    /// </list>
    /// </para>
    /// </summary>
    public class AdditiveShaderManager : MonoBehaviour
    {
        /// <summary>
        /// <para>Lookup of remote group <see cref="Guid"/> to associated assets.</para>
        /// <para>A group is only added to the dictionary if it contains one or more assets.</para>
        /// </summary>
        private Dictionary<Guid, IEnumerable<ShaderAsset>> remoteGroups;

        /// <summary>
        /// A list of shader assets that are remotely controlled by external mods
        /// using the <see cref="UserMod.SetVisibilityForTag(string, bool)"/> method.
        /// </summary>
        private List<ShaderAsset> remoteShaders;

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
        /// Tracks if it is currently night time.
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
        /// Adds a remote control group with shaders matching the specified tags.
        /// </summary>
        /// <param name="group">The group <see cref="Guid"/>.</param>
        /// <param name="tags">Shaders containing all tags will be added to the group.</param>
        /// <returns>Returns <c>true</c> if the group contains at leasts one shader, otherwise <c>false</c>.</returns>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        public bool AddRemoteGroup(Guid group, HashSet<string> tags)
        {
            if (remoteShaders.Count == 0 || remoteGroups.ContainsKey(group) || tags == null || tags.Count == 0)
                return false;

            var shaders = new List<ShaderAsset>();

            foreach (var shader in remoteShaders)
                if (tags.IsSubsetOf(shader.HashTags))
                    shaders.Add(shader);

            if (shaders.Count == 0)
                return false;

            shaders.TrimExcess();
            remoteGroups.Add(group, shaders);
            return true;
        }

        /// <summary>
        /// Removes one or more remote contol groups.
        /// </summary>
        /// <param name="groups">A list of one or more group <see cref="Guid"/>s.</param>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        public void RemoveRemoteGroups(ICollection<Guid> groups)
        {
            if (groups == null || groups.Count == 0)
                return;

            foreach (var group in groups)
                if (remoteGroups.ContainsKey(group))
                    remoteGroups.Remove(group);
        }

        /// <summary>
        /// Sets visibility for all shaders in a remote group.
        /// </summary>
        /// <param name="group">The group <see cref="Guid"/>.</param>
        /// <param name="visible">The desired visibility state of the shaders in the group.</param>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        public void SetRemoteGroupVisibility(Guid group, bool visible)
        {
            if (remoteGroups.TryGetValue(group, out var shaders))
                foreach (var shader in shaders)
                    shader.SetVisible(visible);
        }

        /// <summary>
        /// Initialises shader lists.
        /// </summary>
        [UsedImplicitly]
        protected void Start()
        {
            enabled = false;

            remoteGroups = new Dictionary<Guid, IEnumerable<ShaderAsset>>();

            var report    = new AssetReporter();
            var assetList = AssetScanner.ListShaderAssets();

            InitialiseShaderLists(assetList.Count);

            foreach (var shader in assetList)
            {
                (shader.Info.IsStatic
                    ? shader.Info.IsRemotelyControlled
                        ? remoteShaders
                        : staticShaders
                    : shader.Info.IsToggledByTwilight
                        ? twilightShaders
                        : generalShaders)
                .Add(shader);

                report.Append(shader);
            }

            TrimShaderLists();

            report.Summary(assetList.Count, remoteShaders.Count, twilightShaders.Count, generalShaders.Count);
            report.PublishToLogFile();

            enabled = twilightShaders.Count != 0 || generalShaders.Count != 0;
        }

        /// <summary>
        /// Update loop. Performance critical!
        /// </summary>
        [UsedImplicitly]
        protected void Update()
        {
            if (++index >= count && !IsReadyForNextUpdate())
                return;

            if (iterateTwilight)
            {
                twilightShaders[index].SetVisibleByTwilight(isNightTime);
            }
            else
            {
                generalShaders[index].SetVisibleByTime(Singleton<SimulationManager>.instance.m_currentDayTimeHour);
            }
        }

        /// <summary>
        /// Terminates the manager.
        /// </summary>
        [UsedImplicitly]
        protected void OnDestroy()
        {
            CancelInvoke();

            enabled = false;

            remoteGroups = null;

            DisposeShaderLists();
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
            remoteShaders = new List<ShaderAsset>(capacity);
            twilightShaders = new List<ShaderAsset>(capacity);
            generalShaders = new List<ShaderAsset>(capacity);
        }

        /// <summary>
        /// Trims excess capacity from populated lists.
        /// </summary>
        private void TrimShaderLists()
        {
            staticShaders.TrimExcess();
            remoteShaders.TrimExcess();
            twilightShaders.TrimExcess();
            generalShaders.TrimExcess();
        }

        /// <summary>
        /// Disposes the shader lists.
        /// </summary>
        private void DisposeShaderLists()
        {
            staticShaders = null;
            remoteShaders = null;
            twilightShaders = null;
            generalShaders = null;
        }
    }
}
