namespace AdditiveShader
{
    using System;
    using ColossalFramework;
    using JetBrains.Annotations;
    using UnityEngine;

    public class RainShaders : MonoBehaviour
    {
        private Guid rainGroup;

        private AdditiveShaderAPI api;

        private bool initialised;

        private bool rainState; // true when raining
        private bool rainStateChanged;

        /// <summary>
        /// Called by the game when the manager starts.
        /// </summary>
        [UsedImplicitly]
        protected void Start()
        {
            rainGroup = Guid.NewGuid();

            api = new AdditiveShaderAPI();

            enabled = api.Connect();
        }

        /// <summary>
        /// Called by the game every update, unelss <c>.enabled = false</c>.
        /// </summary>
        [UsedImplicitly]
        protected void Update()
        {
            if (!initialised && !CreateGroups())
                return;

            if (rainStateChanged)
                api.SetGroupVisibility(rainGroup, rainState);
        }

        /// <summary>
        /// Check the weather.
        /// </summary>
        protected void CheckRainState()
        {
            bool currentState = Singleton<WeatherManager>.instance.m_currentRain > 0.1;

            Singleton<WeatherManager>.instance.m_currentCloud
            Singleton<WeatherManager>.instance.m_currentFog
            Singleton<WeatherManager>.instance.m_currentNorthernLights
            Singleton<WeatherManager>.instance.m_currentRain
            Singleton<WeatherManager>.instance.m_currentRainbow
            Singleton<WeatherManager>.instance.m_currentTemperature
            Singleton<WeatherManager>.instance.m_directionSpeed
            Singleton<WeatherManager>.instance.m_groundWetness
            Singleton<WeatherManager>.instance.m_lastLightningIntensity
            Singleton<WeatherManager>.instance.m_windDirection

            if (rainState != currentState)
            {
                rainState = currentState;
                rainStateChanged = true;
            }
        }

        /// <summary>
        /// Terminates the manager.
        /// </summary>
        protected void OnDestroy()
        {
            CancelInvoke();

            enabled = false;

            api.Disconnect();
            api = null;
        }

        /// <summary>
        /// Create the groups.
        /// </summary>
        /// <returns>Returns <c>true</c> if successful.</returns>
        private bool CreateGroups()
        {
            initialised = true;

            InvokeRepeating(nameof(CheckRainState), 1.0f, 5.0f);

            if (!api.NewGroup(rainGroup, "on-during-rain"))
                OnDestroy();

            return enabled;
        }
    }
}
