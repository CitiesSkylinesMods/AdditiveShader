namespace AdditiveShader
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using ColossalFramework;
    using ColossalFramework.PlatformServices;
    using ColossalFramework.Plugins;
    using UnityEngine;
    using static ColossalFramework.Plugins.PluginManager;


    // WORK IN PROGRESS, LIKELY NEEDS MOVING TO SEPARATE PROJECT


    /// <summary>
    /// API for remote-controllable Additive Shaders.
    /// </summary>
    public class AdditiveShaderAPI
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdditiveShaderAPI"/> class.
        /// </summary>
        public AdditiveShaderAPI()
        {
            GroupState = new Dictionary<Guid, bool>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the API is being debugged (extra validity checks, etc).
        /// </summary>
        public static bool Debugging { get; set; }

        /// <summary>
        /// <para>Gets a value indicating whether Additive Shader mod is subscribed and enabled.</para>
        /// <para>Use this to determine whether it's worth creating your <see cref="GameObject"/> / <see cref="MonoBehaviour"/>.</para>
        /// </summary>
        /// <returns>Returns <c>true</c> if mod is subscribed and enabled, otherwise <c>false</c>.</returns>
        public static bool IsAvailable =>
            IsModEnabled(1410003347uL, new HashSet<string> { "Additive Shader", "AdditiveShader" });

        /// <summary>
        /// Gets a value indicating whether the Steam Workshop is available.
        /// </summary>
        [SuppressMessage("Maintainability", "AV1502:Logical not operator is applied on a member which has a negation in its name", Justification = "Game API.")]
        public static bool IsWorkshopAvailable =>
            PlatformService.platformType == PlatformType.Steam && !PluginManager.noWorkshop;

        /// <summary>
        /// Gets a value indicating whether the API is connected to the Additive Shader mod.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// <para>Gets a dictionary keyed by group <see cref="Guid"/>.</para>
        /// <para>Values indicate if the group is active or not.</para>
        /// <para>A group will be inactive if it's empty (no shaders matched the tags).</para>
        /// </summary>
        public Dictionary<Guid, bool> GroupState { get; }

        /// <summary>
        /// Gets a value indicating whether the specified mod is enabled.
        /// </summary>
        /// <param name="workshopId">The workshop id of the mod.</param>
        /// <param name="modName">The mod name as it appearas in <see cref="PluginInfo.name"/>.</param>
        /// <returns>Returns <c>true</c> if the mod is enabled, otherwise <c>false</c>.</returns>
        public static bool IsModEnabled(ulong workshopId, HashSet<string> modName) =>
            Singleton<PluginManager>.instance.GetPluginsInfo()
                .Any(plugin => plugin.isEnabled && (plugin.publishedFileID.AsUInt64 == workshopId || modName.Contains(plugin.name)));

        /// <summary>
        /// <para>Connect the API to the Additive Shader mod.</para>
        /// <para>This should go in your <see cref="MonoBehaviour.Start()"/> event.</para>
        /// <para>To check if connected at any time, query <see cref="IsConnected"/>.</para>
        /// </summary>
        /// <returns>Returns <c>true</c> if successfully connected, otherwise <c>false</c>.</returns>
        public bool Connect()
        {
            if (!IsAvailable)
                return false;

            // todo: cache refs to AS methods

            IsConnected = true; // or false

            return IsConnected;
        }

        /// <summary>
        /// Disconnect the API from the Additive Shader mod.
        /// </summary>
        public void Disconnect()
        {
            if (!IsConnected)
                return;

            // todo: send GroupState.Keys to AS so it can remove them

            GroupState.Clear();

            // todo: dispose refs/lists

            IsConnected = false;
        }

        /// <summary>
        /// Creates a new group comtaining shaders that match the specified tag.
        /// </summary>
        /// <param name="group">The group <see cref="Guid"/>.</param>
        /// <param name="tag">One tag. Tags are case-sesitive, use lowercase.</param>
        /// <returns>Returns <c>true</c> if the group was created and has at least one shader, otherwise <c>false</c>.</returns>
        public bool NewGroup(Guid group, string tag) =>
            NewGroup(group, new HashSet<string> { tag });

        /// <summary>
        /// Creates a new group comtaining shaders that match all of the specified tags.
        /// </summary>
        /// <param name="group">The group <see cref="Guid"/>.</param>
        /// <param name="tags">One or more tags. Tags are case-sesitive, use lowercase.</param>
        /// <returns>Returns <c>true</c> if the group was created and has at least one shader, otherwise <c>false</c>.</returns>
        public bool NewGroup(Guid group, params string[] tags) =>
            NewGroup(group, new HashSet<string>(tags));

        /// <summary>
        /// Creates a new group comtaining shaders that match all of the specified tags.
        /// </summary>
        /// <param name="group">The group <see cref="Guid"/>.</param>
        /// <param name="tags">A <see cref="HashSet{string}"/> contianing one or more tags. Tags are case-sesitive, use lowercase.</param>
        /// <returns>Returns <c>true</c> if the group was created and has at least one shader, otherwise <c>false</c>.</returns>
        [SuppressMessage("Simplification", "RCS1104:Simplify conditional expression.", Justification = "Not valid in this context.")]
        public bool NewGroup(Guid group, HashSet<string> tags)
        {
            if (!IsConnected)
            {
                return Debugging
                    ? throw new MemberAccessException("[AdditiveShaderAPI] Unable to create groups if not connected.")
                    : false;
            }

            if (Debugging)
            {
                if (GroupState.ContainsKey(group))
                    throw new ArgumentException("[AdditiveShaderAPI] Group already defined.", nameof(group));

                if (tags == null)
                    throw new ArgumentNullException(nameof(tags), "[AdditiveShaderAPI] Tags missing.");

                if (tags.Count == 0)
                    throw new ArgumentOutOfRangeException(nameof(tags), "[AdditiveShaderAPI] Must specify at least one tag.");
            }

            // todo: relay to additive shader and return result

            return true; // or false
        }

        /// <summary>
        /// Sets the visibility of the specified filter list.
        /// </summary>
        /// <param name="group">The group <see cref="Guid"/>.</param>
        /// <param name="visible">If <c>true</c>, the associated shaders will be shown, otherwise they will be hidden.</param>
        public void SetGroupVisibility(Guid group, bool visible)
        {
            if (!GroupState.TryGetValue(group, out bool valid) || !valid || !IsConnected)
                return;

            // todo: relay to additive shader
        }
    }
}
