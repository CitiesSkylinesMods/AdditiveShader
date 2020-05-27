using ColossalFramework;
using ICities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdditiveShader
{

    public class AdditiveShaderLogic : MonoBehaviour
    {
        private float minimumUpdateTimeDelta = 0.1f; //6 minutes
        private float lastUpdateTime = 0f;

        public List<AdditiveShaderInfo> shaderInfos;
        public const string SHADER_IDENTIFIER = "AdditiveShader";
        public bool _loaded = false;

        void Start()
        {
            shaderInfos = new List<AdditiveShaderInfo>();
            foreach (PropInfo prop in Resources.FindObjectsOfTypeAll<PropInfo>())
            {
                if (prop == null)
                    continue;
                if (prop.m_mesh == null)
                    continue;
                if (prop.m_mesh.name == null)
                    continue;
                if (prop.m_mesh.name.Contains(SHADER_IDENTIFIER))
                {
                    string[] data = prop.m_mesh.name.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    var psi = new AdditiveShaderInfo();
                    psi.m_assetType = "prop";
                    psi.m_prop = prop;
                    psi.m_building = null;
                    psi.m_buildingSubMesh = null;
                    psi.m_vehicleSubMesh = null;
                    psi.m_enableTime = float.Parse(data[1]);
                    psi.m_disableTime = float.Parse(data[2]);
                    psi.m_fade = float.Parse(data[3]);
                    psi.m_intensity = float.Parse(data[4]);
                    psi.m_enabled = false;
                    shaderInfos.Add(psi);
                    Debug.Log("[AdditiveShader] : Loaded a prop marked as having the AdditiveShader");
                    PropOnLoaded(prop, psi);
                }
            }
            foreach (BuildingInfo building in Resources.FindObjectsOfTypeAll<BuildingInfo>())
            {
                if (building == null)
                    continue;
                if (building.m_mesh == null)
                    continue;
                if (building.m_mesh.name == null)
                    continue;
                if (building.m_mesh.name.Contains(SHADER_IDENTIFIER))
                {
                    string[] data = building.m_mesh.name.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    var psi = new AdditiveShaderInfo();
                    psi.m_assetType = "building";
                    psi.m_building = building;
                    psi.m_buildingSubMesh = null;
                    psi.m_vehicleSubMesh = null;
                    psi.m_prop = null;
                    psi.m_enableTime = float.Parse(data[1]);
                    psi.m_disableTime = float.Parse(data[2]);
                    psi.m_fade = float.Parse(data[3]);
                    psi.m_intensity = float.Parse(data[4]);
                    psi.m_enabled = false;
                    shaderInfos.Add(psi);
                    Debug.Log("[AdditiveShader] : Loaded a building marked as having the AdditiveShader");
                    BuildingOnLoaded(building, psi);
                }
                if (building.m_props == null || building.m_props.Length == 0) continue;
                for (uint j = 0; j < building.m_props.Length; j++)
                {
                    if (building.m_props[j].m_finalProp == null) continue;
                    if (building.m_props[j].m_finalProp.m_mesh == null) continue;
                    if (building.m_props[j].m_finalProp.m_mesh.name.Contains(SHADER_IDENTIFIER))
                        building.m_maxPropDistance = 25000;
                }
            }
            foreach (BuildingInfoSub buildingSubMesh in Resources.FindObjectsOfTypeAll<BuildingInfoSub>())
            {
                if (buildingSubMesh == null)
                    continue;
                if (buildingSubMesh.m_mesh == null)
                    continue;
                if (buildingSubMesh.m_mesh.name == null)
                    continue;
                if (buildingSubMesh.m_mesh.name.Contains(SHADER_IDENTIFIER))
                {
                    string[] data = buildingSubMesh.m_mesh.name.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    var psi = new AdditiveShaderInfo();
                    psi.m_assetType = "buildingSubMesh";
                    psi.m_building = null;
                    psi.m_buildingSubMesh = buildingSubMesh;
                    psi.m_vehicleSubMesh = null;
                    psi.m_prop = null;
                    psi.m_enableTime = float.Parse(data[1]);
                    psi.m_disableTime = float.Parse(data[2]);
                    psi.m_fade = float.Parse(data[3]);
                    psi.m_intensity = float.Parse(data[4]);
                    psi.m_enabled = false;
                    shaderInfos.Add(psi);
                    Debug.Log("[AdditiveShader] : Loaded a building sub mesh marked as having the AdditiveShader");
                    BuildingSubMeshOnLoaded(buildingSubMesh, psi);
                }
            }
            foreach (VehicleInfoSub vehicleSubMesh in Resources.FindObjectsOfTypeAll<VehicleInfoSub>())
            {
                if (vehicleSubMesh == null)
                    continue;
                if (vehicleSubMesh.m_mesh == null)
                    continue;
                if (vehicleSubMesh.m_mesh.name == null)
                    continue;
                if (vehicleSubMesh.m_mesh.name.Contains(SHADER_IDENTIFIER))
                {
                    string[] data = vehicleSubMesh.m_mesh.name.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    var psi = new AdditiveShaderInfo();
                    psi.m_assetType = "vehicleSubMesh";
                    psi.m_building = null;
                    psi.m_buildingSubMesh = null;
                    psi.m_vehicleSubMesh = vehicleSubMesh;
                    psi.m_prop = null;
                    psi.m_enableTime = float.Parse(data[1]);
                    psi.m_disableTime = float.Parse(data[2]);
                    psi.m_fade = float.Parse(data[3]);
                    psi.m_intensity = float.Parse(data[4]);
                    psi.m_enabled = false;
                    shaderInfos.Add(psi);
                    Debug.Log("[AdditiveShader] : Loaded a vehicle sub mesh marked as having the AdditiveShader");
                    VehicleSubMeshOnLoaded(vehicleSubMesh, psi);
                }
            }
            _loaded = true;
        }

        void Update()
        {
            if (!_loaded)
            {
                return;
            }

            float time = Singleton<SimulationManager>.instance.m_currentDayTimeHour;
            if (time - lastUpdateTime < minimumUpdateTimeDelta)
            {
                return;
            }

            for (int i = 0; i < shaderInfos.Count; i++)
            {
                AdditiveShaderInfo psInfo = shaderInfos[i];
                if (psInfo.m_enableTime < psInfo.m_disableTime)
                {
                    if ((psInfo.m_enableTime <= time) && (time < psInfo.m_disableTime))
                    {
                        Enable(psInfo);
                    }
                    else if ((time < psInfo.m_enableTime) || (time >= psInfo.m_disableTime))
                    {
                        Disable(psInfo);
                    }
                }
                else
                {
                    if ((psInfo.m_disableTime <= time) && (time < psInfo.m_enableTime))
                    {
                        Disable(psInfo);
                    }
                    else if ((time < psInfo.m_disableTime) || (time >= psInfo.m_enableTime))
                    {
                        Enable(psInfo);
                    }
                }
            }
        }

        public void PropOnLoaded(PropInfo info, AdditiveShaderInfo psInfo)
        {
            info.m_lodHasDifferentShader = false;
            info.m_material.SetFloat("_InvFade", psInfo.m_fade);
            DisableShaderProp(info, psInfo);
        }

        public void BuildingOnLoaded(BuildingInfo info, AdditiveShaderInfo psInfo)
        {
            info.m_lodHasDifferentShader = false;
            info.m_lodMissing = true;
            info.m_material.SetFloat("_InvFade", psInfo.m_fade);
            DisableShaderBuilding(info, psInfo);
            Vector3[] vertices = info.m_mesh.vertices;
            Color[] colors = new Color[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) colors[i] = Color.white;
            info.m_mesh.colors = colors;
        }

        public void BuildingSubMeshOnLoaded(BuildingInfoSub info, AdditiveShaderInfo psInfo)
        {
            info.m_lodHasDifferentShader = false;
            info.m_material.SetFloat("_InvFade", psInfo.m_fade);
            DisableShaderBuildingSubMesh(info, psInfo);
            Vector3[] vertices = info.m_mesh.vertices;
            Color[] colors = new Color[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) colors[i] = Color.white;
            info.m_mesh.colors = colors;
        }

        public void VehicleSubMeshOnLoaded(VehicleInfoSub info, AdditiveShaderInfo psInfo)
        {
            info.m_material.SetFloat("_InvFade", psInfo.m_fade);
            DisableShaderVehicleSubMesh(info, psInfo);
            Vector3[] vertices = info.m_mesh.vertices;
            Color[] colors = new Color[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) colors[i] = Color.white;
            info.m_mesh.colors = colors;
        }

        public void EnableShaderProp(PropInfo info, AdditiveShaderInfo psInfo)
        {
            info.m_lodRenderDistance = info.m_maxRenderDistance =
            (info.m_generatedInfo.m_size.x + 30) * (info.m_generatedInfo.m_size.y + 30) * (info.m_generatedInfo.m_size.z + 30) * 0.1f;

            info.m_material.SetFloat("_Intensity", psInfo.m_intensity);
        }

        public void DisableShaderProp(PropInfo info, AdditiveShaderInfo psInfo)
        {
            info.m_lodRenderDistance = info.m_maxRenderDistance =
            (info.m_generatedInfo.m_size.x + 30) * (info.m_generatedInfo.m_size.y + 30) * (info.m_generatedInfo.m_size.z + 30) * 0.1f;

            info.m_material.SetFloat("_Intensity", 0f);
        }

        public void EnableShaderBuilding(BuildingInfo info, AdditiveShaderInfo psInfo)
        {
            info.m_maxLodDistance = info.m_minLodDistance =
            (info.m_generatedInfo.m_max.x + 30) * (info.m_generatedInfo.m_max.y + 30) * (info.m_generatedInfo.m_max.z + 30) * 0.1f;

            info.m_material.SetFloat("_Intensity", psInfo.m_intensity);
        }

        public void DisableShaderBuilding(BuildingInfo info, AdditiveShaderInfo psInfo)
        {
            info.m_maxLodDistance = info.m_minLodDistance =
            (info.m_generatedInfo.m_max.x + 30) * (info.m_generatedInfo.m_max.y + 30) * (info.m_generatedInfo.m_max.z + 30) * 0.1f;

            info.m_material.SetFloat("_Intensity", 0f);
        }


        public void EnableShaderBuildingSubMesh(BuildingInfoSub info, AdditiveShaderInfo psInfo)
        {
            info.m_maxLodDistance = info.m_minLodDistance =
            (info.m_generatedInfo.m_max.x + 30) * (info.m_generatedInfo.m_max.y + 30) * (info.m_generatedInfo.m_max.z + 30) * 0.1f;

            info.m_material.SetFloat("_Intensity", psInfo.m_intensity);
        }

        public void DisableShaderBuildingSubMesh(BuildingInfoSub info, AdditiveShaderInfo psInfo)
        {
            info.m_maxLodDistance = info.m_minLodDistance =
            (info.m_generatedInfo.m_max.x + 30) * (info.m_generatedInfo.m_max.y + 30) * (info.m_generatedInfo.m_max.z + 30) * 0.1f;

            info.m_material.SetFloat("_Intensity", 0f);
        }


        public void EnableShaderVehicleSubMesh(VehicleInfoSub info, AdditiveShaderInfo psInfo)
        {
            info.m_lodRenderDistance = info.m_maxRenderDistance =
            (info.m_generatedInfo.m_size.x + 30) * (info.m_generatedInfo.m_size.y + 30) * (info.m_generatedInfo.m_size.z + 30) * 0.1f;

            info.m_material.SetFloat("_Intensity", psInfo.m_intensity);
        }

        public void DisableShaderVehicleSubMesh(VehicleInfoSub info, AdditiveShaderInfo psInfo)
        {
            info.m_lodRenderDistance = info.m_maxRenderDistance =
            (info.m_generatedInfo.m_size.x + 30) * (info.m_generatedInfo.m_size.y + 30) * (info.m_generatedInfo.m_size.z + 30) * 0.1f;

            info.m_material.SetFloat("_Intensity", 0f);
        }


        private void Enable(AdditiveShaderInfo info)
        {
            if (!info.m_enabled)
            {
                if (info.m_assetType == "prop")
                    EnableShaderProp(info.m_prop, info);
                else if (info.m_assetType == "building")
                    EnableShaderBuilding(info.m_building, info);
                else if (info.m_assetType == "buildingSubMesh")
                    EnableShaderBuildingSubMesh(info.m_buildingSubMesh, info);
                else if (info.m_assetType == "vehicleSubMesh")
                    EnableShaderVehicleSubMesh(info.m_vehicleSubMesh, info);
                info.m_enabled = true;
            }
        }
        private void Disable(AdditiveShaderInfo info)
        {
            if (info.m_enabled)
            {
                if (info.m_assetType == "prop")
                    DisableShaderProp(info.m_prop, info);
                else if (info.m_assetType == "building")
                    DisableShaderBuilding(info.m_building, info);
                else if (info.m_assetType == "buildingSubMesh")
                    DisableShaderBuildingSubMesh(info.m_buildingSubMesh, info);
                else if (info.m_assetType == "vehicleSubMesh")
                    DisableShaderVehicleSubMesh(info.m_vehicleSubMesh, info);
                info.m_enabled = false;
            }
        }
    }

    public class AdditiveShaderInfo
    {
        public AdditiveShaderInfo() { }

        public float m_enableTime, m_disableTime;
        public float m_intensity, m_fade;
        public PropInfo m_prop;
        public BuildingInfo m_building;
        public BuildingInfoSub m_buildingSubMesh;
        public VehicleInfoSub m_vehicleSubMesh;
        public string m_assetType;
        public bool m_enabled;

        public PrefabInfo Prefab
        {
            get
            {
                if (m_assetType == "prop")
                    return m_prop;
                else if (m_assetType == "building")
                    return m_building;
                else if (m_assetType == "buildingSubMesh")
                    return m_buildingSubMesh;
                else if (m_assetType == "vehicleSubMesh")
                    return m_vehicleSubMesh;
                else return null;
            }
        }

    }
}