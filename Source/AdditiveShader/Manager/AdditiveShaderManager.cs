namespace AdditiveShader.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using ColossalFramework;
    using ColossalFramework.UI;
    using JetBrains.Annotations;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

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
        private const bool FORCE = true;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore")]
        private const float TWILIGHT_CHECK_INTERVAL = 9.7f;

        private static AdditiveShaderManager instance;

        /// <summary>
        /// <para>Lookup of remote group <see cref="Guid"/> to associated assets.</para>
        /// <para>A group is only added to the dictionary if it contains one or more assets.</para>
        /// </summary>
        private Dictionary<Guid, List<ManagedAsset>> remoteGroups;

        /// <summary>
        /// A list containing all shader-using assets.
        /// </summary>
        private List<ManagedAsset> managedAssets;

        /// <summary>
        /// A list of time-based shader-using assets that toggle at twilight (dusk + dawn).
        /// </summary>
        private List<ManagedAsset> twilightAssets;

        /// <summary>
        /// A list of time-based shader-using assets (excluding <see cref="twilightAssets"/>).
        /// </summary>
        private List<ManagedAsset> timeBasedAssets;

        /// <summary>
        /// Tracks if it is currently night time.
        /// </summary>
        private bool isNightTime;

        /// <summary>
        /// <para>Indicates the active list for <see cref="Update()"/>.</para>
        /// <list type="bullet">
        /// <item><c>true</c> -- <see cref="twilightAssets"/></item>
        /// <item><c>false</c> -- <see cref="timeBasedAssets"/></item>
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
        public bool AddRemoteGroup(Guid group, HashSet<string> tags)
        {
            if (managedAssets == null || tags == null || managedAssets.Count == 0 || tags.Count == 0 || remoteGroups?.ContainsKey(group) != false)
                return false;

            var list = new List<ManagedAsset>(managedAssets.Count);

            foreach (var asset in managedAssets)
                if (!asset.IsContainer && tags.IsSubsetOf(asset.HashTags))
                    list.Add(asset);

            if (list.Count == 0)
                return false;

            list.TrimExcess();
            remoteGroups.Add(group, list);
            return true;
        }

        /// <summary>
        /// Removes one or more remote contol groups.
        /// </summary>
        /// <param name="groups">A list of one or more group <see cref="Guid"/>s.</param>
        public void RemoveRemoteGroups(ICollection<Guid> groups)
        {
            if (groups == null || remoteGroups == null || groups.Count == 0)
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
        public void SetRemoteGroupVisibility(Guid group, bool visible)
        {
            if (remoteGroups != null && remoteGroups.TryGetValue(group, out var list))
                foreach (var asset in list)
                    asset.SetVisible(visible);
        }

        /// <summary>
        /// Initialises the manager.
        /// </summary>
        [UsedImplicitly]
        [SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements")]
        protected void Start()
        {
            Debug.Log("[AdditiveShader] AdditiveShaderManager.Start()");

            AssetReporter.Timer = Stopwatch.StartNew();

            managedAssets = AssetScanner.CollateManagedAssets();

            if (managedAssets.Count == 0)
            {
                OnDestroy();
                return;
            }

            RefreshRenderGroups();

            BuildCategories();

            remoteGroups = new Dictionary<Guid, List<ManagedAsset>>();

            instance = this;

            if (twilightAssets.Count != 0)
            {
                CheckForTwilight(FORCE);
                InvokeRepeating(nameof(CheckForTwilight), TWILIGHT_CHECK_INTERVAL, TWILIGHT_CHECK_INTERVAL);
            }
            else
            {
                enabled = timeBasedAssets.Count != 0;
            }

            HookEvents(true);
        }

        /// <summary>
        /// If we've cross twilight (day/night) boundary, iterate the twilight assets.
        /// </summary>
        /// <param name="force">Only used by <c>Start()</c> to force a twilight iteration.</param>
        protected void CheckForTwilight(bool force = false)
        {
            bool isNightTimeNow = Singleton<SimulationManager>.instance.m_isNightTime;

            if (force || isNightTime != isNightTimeNow)
            {
                isNightTime = isNightTimeNow;

                iterateTwilight = true;

                count = twilightAssets.Count;
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
                twilightAssets[index++].SetVisibleByTwilight(isNightTime);
            }
            else
            {
                timeBasedAssets[index++].SetVisibleByTime(Singleton<SimulationManager>.instance.m_currentDayTimeHour);
            }
        }

        /// <summary>
        /// Terminates the manager.
        /// </summary>
        [UsedImplicitly]
        protected void OnDestroy()
        {
            Debug.Log("[AdditiveShader] Manager shutting down.");
            HookEvents(false);
            instance = null;
            CancelInvoke();
            enabled = false;
            remoteGroups = null;
            RestoreOriginalAssetSettings();
            DisposeAssetLists();
        }

        /// <summary>
        /// After finishing an iteration, this works out what to do next.
        /// </summary>
        private void DetermineWhatHappensNext()
        {
            iterateTwilight = false;

            count = timeBasedAssets.Count;
            index = 0;

            enabled = count > index;
        }

        /// <summary>
        /// Hook events so that assets can be refreshed when game options change.
        /// </summary>
        /// <param name="enable">Hooks when <c>true</c>, unhooks when <c>false</c>.</param>
        [SuppressMessage("Naming", "AV1738:Event handlers should be named according to the pattern '(InstanceName)On(EventName)'", Justification = "Overkill.")]
        private void HookEvents(bool enable)
        {
            var optionsPanel = UIView.library.Get<UIPanel>("OptionsPanel");
            var levelOfDetail = UIView.GetAView().FindUIComponent<UIDropDown>("LevelOfDetail");

            if (enable)
            {
                optionsPanel.eventVisibilityChanged     += OnOptionsPanelVisibilityChanged;
                levelOfDetail.eventSelectedIndexChanged += OnLevelOfDetailChanged;
            }
            else
            {
                optionsPanel.eventVisibilityChanged     += OnOptionsPanelVisibilityChanged;
                levelOfDetail.eventSelectedIndexChanged -= OnLevelOfDetailChanged;
            }
        }

        /// <summary>
        /// Many things can overwrite render distances,
        /// so reapply our own just in case.
        /// </summary>
        /// <param name="component">Ignored.</param>
        /// <param name="visible">The visibility of the options panel.</param>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Game API")]
        private void OnOptionsPanelVisibilityChanged(UIComponent component, bool visible)
        {
            if (!visible)
                instance.RefreshRenderDistances();
        }

        /// <summary>
        /// If 'Level Of Detail' game setting is changed,
        /// it resets the render distances so we need to
        /// reapply our own render distances.
        /// </summary>
        /// <param name="component">Ignored.</param>
        /// <param name="index">Ignored.</param>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Game API")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1625:Element documentation should not be copied and pasted")]
        private void OnLevelOfDetailChanged(UIComponent component, int index) =>
            RefreshRenderDistances();

        /// <summary>
        /// In situations where render distances might be overwritten, we have to reapply them.
        /// </summary>
        private void RefreshRenderDistances()
        {
            foreach (var asset in managedAssets)
                asset.ApplyCachedRenderDistance();

            RefreshRenderGroups();
        }

        /// <summary>
        /// After changing render distances, render groups need invalidating to push changes to renderer.
        /// </summary>
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Rarely invoked.")]
        private void RefreshRenderGroups()
        {
            int buildingLayer = LayerMask.NameToLayer("Buildings");
            int propsLayer    = LayerMask.NameToLayer("Props");

            foreach (var renderGroup in RenderManager.instance.m_groups)
            {
                if (renderGroup == null)
                    continue;

                renderGroup.SetLayerDataDirty(buildingLayer);
                renderGroup.SetLayerDataDirty(propsLayer);
                renderGroup.UpdateMeshData();
            }
        }

        /// <summary>
        /// Time-based assets are sorted in to groups for easier management of update loop.
        /// </summary>
        private void BuildCategories()
        {
            PrepareAssetCategories(managedAssets.Count);
            SortAssetsInToCategories(); // also generates report
            CompactAssetCategories();
        }

        /// <summary>
        /// Initialises category lists with plenty of capacity to minimise time-consuming
        /// memory allocations, which in turn speeds up <see cref="SortAssetsInToCategories"/>.
        /// </summary>
        /// <param name="capacity">The initial capacity for the lists.</param>
        private void PrepareAssetCategories(int capacity)
        {
            twilightAssets = new List<ManagedAsset>(capacity);
            timeBasedAssets = new List<ManagedAsset>(capacity);
        }

        /// <summary>
        /// There's no point iterating static assets, and there's no point iterating twilight
        /// assets outside dawn/dusk, so filter in to context-specific lists to make iteration
        /// more effective.
        /// </summary>
        /// <remarks>
        /// Report could be done separtely, but doing it here saves an extra loop.
        /// </remarks>
        private void SortAssetsInToCategories()
        {
            var report = new AssetReporter();

            foreach (var asset in managedAssets)
            {
                if (!asset.IsContainer && !asset.Info.IsStatic)
                    (asset.Info.IsToggledByTwilight ? twilightAssets : timeBasedAssets)
                        .Add(asset);

                report.Append(asset);
            }

            report.Summary(managedAssets.Count, twilightAssets.Count, timeBasedAssets.Count);
            report.PublishToLogFile();
        }

        /// <summary>
        /// Lists were created with excess capacity; this trims that capacity to free RAM.
        /// </summary>
        private void CompactAssetCategories()
        {
            twilightAssets.TrimExcess();
            timeBasedAssets.TrimExcess();
        }

        /// <summary>
        /// Remove references to lists so GC can start reclaiming RAM sooner.
        /// </summary>
        private void DisposeAssetLists()
        {
            managedAssets = null;
            twilightAssets = null;
            timeBasedAssets = null;
        }

        /// <summary>
        /// Restore assets to their published defaults when shutting down.
        /// </summary>
        /// <remarks>
        /// This will help avoid asset corruption if user quits to main menu
        /// then goes in to asset editor. But only for games which have
        /// not already saved the altered values (ie. used earlier versions
        /// of the mod). ProTip: Always exit to desktop before using editor.
        /// </remarks>
        private void RestoreOriginalAssetSettings()
        {
            foreach (var asset in managedAssets)
                asset.RestoreOriginalSettings();
        }

        /// <summary>
        /// Updates the light intensity for the asset to whatever it should normally be at this moment in time.
        /// </summary>
        /// <param name="asset">The <see cref="ManagedAsset"/> to refresh.</param>
        [SuppressMessage("Maintainability", "AV1537:If-else-if construct should end with an unconditional else clause", Justification = "Intentional.")]
        private void RefreshAssetVisibility(ManagedAsset asset)
        {
            if (asset.Info.IsToggledByTwilight)
            {
                Debug.Log($"[AdditiveShader] - {asset.TypeOfAsset} Refresh by twilight: {isNightTime}");
                asset.SetVisibleByTwilight(isNightTime);
            }
            else if (!asset.Info.IsStatic)
            {
                Debug.Log($"[AdditiveShader] - {asset.TypeOfAsset} Refresh by time: {Singleton<SimulationManager>.instance.m_currentDayTimeHour}");
                asset.SetVisibleByTime(Singleton<SimulationManager>.instance.m_currentDayTimeHour);
            }
            else if (asset.Info.IsAlwaysOn)
            {
                Debug.Log($"[AdditiveShader] - {asset.TypeOfAsset} Refresh as AlwaysOn");
                asset.SetVisible(true);
            }
        }
    }
}
