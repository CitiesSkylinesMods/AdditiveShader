namespace AdditiveShader.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore")]
        private const float TWILIGHT_CHECK_INTERVAL = 9.7f;

        /// <summary>
        /// <para>Lookup of remote group <see cref="Guid"/> to associated assets.</para>
        /// <para>A group is only added to the dictionary if it contains one or more assets.</para>
        /// </summary>
        private Dictionary<Guid, IEnumerable<ShaderAsset>> remoteGroups;

        /// <summary>
        /// A list containing all shaders.
        /// </summary>
        private ICollection<ShaderAsset> allShaders;

        /// <summary>
        /// A list of time-based shaders that toggle at twilight (dusk + dawn).
        /// </summary>
        private List<ShaderAsset> twilightShaders;

        /// <summary>
        /// A list of time-based shaders (excluding <see cref="twilightShaders"/>).
        /// </summary>
        private List<ShaderAsset> timeBasedShaders;

        /// <summary>
        /// Tracks if it is currently night time.
        /// </summary>
        private bool isNightTime;

        /// <summary>
        /// <para>Indicates the active list for <see cref="Update()"/>.</para>
        /// <list type="bullet">
        /// <item><c>true</c> -- <see cref="twilightShaders"/></item>
        /// <item><c>false</c> -- <see cref="timeBasedShaders"/></item>
        /// </list>
        /// </summary>
        private bool iterateTwilight;

        /// <summary>
        /// Current item index in active list.
        /// </summary>
        private int index;

        /// <summary>
        /// Number of items in the active list.
        /// </summary>
        private int count;

        /// <summary>
        /// Create a remote control group listing shaders matching the specified tags.
        /// </summary>
        /// <param name="group">The group <see cref="Guid"/> (unique identifier).</param>
        /// <param name="tags">Shaders matching all tags will be added to the group.</param>
        /// <returns>Returns <c>true</c> if the group contains at least one shader, otherwise <c>false</c>.</returns>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        public bool AddRemoteGroup(Guid group, HashSet<string> tags)
        {
            if (allShaders == null || allShaders.Count == 0 || tags == null || tags.Count == 0 || remoteGroups?.ContainsKey(group) != false)
                return false;

            var shaders = new List<ShaderAsset>(allShaders.Count);

            foreach (var shader in allShaders)
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
            if (groups == null || groups.Count == 0 || remoteGroups == null)
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
            if (remoteGroups != null && remoteGroups.TryGetValue(group, out var shaders))
                foreach (var shader in shaders)
                    shader.SetVisible(visible);
        }

        /// <summary>
        /// Initialises the manager.
        /// </summary>
        [UsedImplicitly]
        [SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements")]
        protected void Start()
        {
            AssetReporter.Timer = Stopwatch.StartNew();

            allShaders = AssetScanner.ListShaderAssets();

            if (allShaders.Count == 0)
            {
                OnDestroy();
                return;
            }

            remoteGroups = new Dictionary<Guid, IEnumerable<ShaderAsset>>();

            PrepareShaderCategories(allShaders.Count);
            SortShadersInToCategories(); // also generates report
            CompactShaderCategories();

            enabled = timeBasedShaders.Count != 0;

            if (twilightShaders.Count != 0)
            {
                InvokeRepeating(nameof(CheckForTwilight), TWILIGHT_CHECK_INTERVAL, TWILIGHT_CHECK_INTERVAL);
                CheckForTwilight(true);
            }
        }

        /// <summary>
        /// Checks to see if we have crossed twilight boundary.
        /// </summary>
        /// <param name="force">On startup this is set true to ensure twilight buildings get set correctly.</param>
        protected void CheckForTwilight(bool force = false)
        {
            bool isNightTimeNow = Singleton<SimulationManager>.instance.m_isNightTime;

            if (force || isNightTime != isNightTimeNow)
            {
                isNightTime = isNightTimeNow;

                iterateTwilight = true;

                count = twilightShaders.Count;
                index = 0;

                enabled = true;
            }
        }

        /// <summary>
        /// Update loop. Performance critical.
        /// </summary>
        [UsedImplicitly]
        protected void Update()
        {
            if (index >= count)
            {
                DetermineWhatHappensNext();
            }
            else if (iterateTwilight)
            {
                twilightShaders[index++].SetVisibleByTwilight(isNightTime);
            }
            else
            {
                timeBasedShaders[index++].SetVisibleByTime(Singleton<SimulationManager>.instance.m_currentDayTimeHour);
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
        /// After a complete iteration of either <see cref="timeBasedShaders"/>
        /// or <see cref="twilightShaders"/>, work out what to do next.
        /// </summary>
        private void DetermineWhatHappensNext()
        {
            iterateTwilight = false;

            count = timeBasedShaders.Count;
            index = 0;

            enabled = count > index;
        }

        /// <summary>
        /// Updates the light intensity for the shader to whatever it should be at this moment in time.
        /// </summary>
        /// <param name="shader">The shader to refresh.</param>
        [SuppressMessage("Maintainability", "AV1537:If-else-if construct should end with an unconditional else clause", Justification = "Intentional.")]
        private void RefreshShader(ShaderAsset shader)
        {
            if (shader.Info.IsToggledByTwilight)
            {
                UnityEngine.Debug.Log($"[AdditiveShader] - {shader.TypeOfAsset} Refresh by twilight: {isNightTime}");
                shader.SetVisibleByTwilight(isNightTime);
            }
            else if (!shader.Info.IsStatic)
            {
                UnityEngine.Debug.Log($"[AdditiveShader] - {shader.TypeOfAsset} Refresh by time: {Singleton<SimulationManager>.instance.m_currentDayTimeHour}");
                shader.SetVisibleByTime(Singleton<SimulationManager>.instance.m_currentDayTimeHour);
            }
            else if (shader.Info.IsAlwaysOn)
            {
                UnityEngine.Debug.Log($"[AdditiveShader] - {shader.TypeOfAsset} AlwaysOn");
                shader.SetVisible(true);
            }
        }

        /// <summary>
        /// <para>Sorts shaders in to categories and publishes report to log file.</para>
        /// <para>Note: Report could be done separtely, but doing it here saves an extra loop.</para>
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1519:Braces should not be omitted from multi-line child statement")]
        private void SortShadersInToCategories()
        {
            var report = new AssetReporter();

            foreach (var shader in allShaders)
            {
                if (!shader.Info.IsStatic)
                    (shader.Info.IsToggledByTwilight ? twilightShaders : timeBasedShaders)
                        .Add(shader);

                report.Append(shader);
            }

            report.Summary(allShaders.Count, twilightShaders.Count, timeBasedShaders.Count);
            report.PublishToLogFile();
        }

        /// <summary>
        /// Initialises shader lists:
        /// <list type="bullet">
        /// <item><see cref="twilightShaders"/> -- toggled at twilight (dusk and dawn)</item>
        /// <item><see cref="timeBasedShaders"/> -- other time-based on/off</item>
        /// </list>
        /// </summary>
        /// <param name="capacity">The initial capacity for the lists.</param>
        private void PrepareShaderCategories(int capacity)
        {
            twilightShaders = new List<ShaderAsset>(capacity);
            timeBasedShaders = new List<ShaderAsset>(capacity);
        }

        /// <summary>
        /// Trims excess capacity from populated lists.
        /// </summary>
        private void CompactShaderCategories()
        {
            twilightShaders.TrimExcess();
            timeBasedShaders.TrimExcess();
        }

        /// <summary>
        /// Disposes the shader lists.
        /// </summary>
        private void DisposeShaderLists()
        {
            allShaders = null;
            twilightShaders = null;
            timeBasedShaders = null;
        }
    }
}
