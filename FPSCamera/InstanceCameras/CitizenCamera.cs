using FPSCamera.Utils;
using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using static ToolBase;

namespace FPSCamera
{

    public class CitizenCamera : MonoBehaviour, InstanceCamera
    {
        private uint followInstance;
        public bool following = false;
        public bool inVehicle = false;

        private CameraController cameraController;
        public Camera camera;
        public DepthOfField effect;

        private CitizenManager cManager;

        public Vector3 userOffset = Vector3.zero;
        public VehicleCamera vehicleCamera;

        public void SetFollowInstance(uint instance)
        {
            FPSCamera.instance.SetMode(false);
            followInstance = instance;
            following = true;
            CameraUtils.SetCamera(cameraController, camera);
            if (FPSCamera.instance.config.displaySpeed)
            {
                FPSCameraSpeedUI.Instance.enabled = true;
            }
            FPSCamera.onCameraModeChanged(true);
        }

        public void StopFollowing()
        {
            following = false;
            CameraUtils.StopCamera(cameraController, camera);
            userOffset = Vector3.zero;
            camera.fieldOfView = FPSCamera.instance.originalFieldOfView;
            FPSCameraSpeedUI.Instance.enabled = false;
            FPSCamera.onCameraModeChanged(false);
            if(!inVehicle)
            {
                vehicleCamera.StopFollowing();
            }
        }

        public void GetInstanceSpeed(Vector3 position)
        {
            var citizen = CitizenManager.instance.m_citizens.m_buffer[followInstance];

            CitizenInstance citizenInstance = CitizenManager.instance.m_instances.m_buffer[citizen.m_instance];
            uint targetFrame = SimulationManager.instance.m_referenceFrameIndex - ((uint)citizen.m_instance << 4) / 65536U;
            Vector3 velocity = Vector3.Lerp(citizenInstance.GetFrameData(targetFrame - 32U).m_velocity,
                citizenInstance.GetFrameData(targetFrame - 16U).m_velocity,
                (float)(((targetFrame & 15U) + SimulationManager.instance.m_referenceTimer) * (1.0 / 16.0))) * 3.75f;

            FPSCameraSpeedUI.Instance.speed = velocity.magnitude;
            FPSCameraSpeedUI.Instance.destinationName = GetDestination();
            FPSCameraSpeedUI.Instance.passengerOrStreet = RaycastRoad(position);
        }

        private string GetDestination()
        {
            var citizen = CitizenManager.instance.m_citizens.m_buffer[followInstance];
            CitizenInstance citizenInstance = CitizenManager.instance.m_instances.m_buffer[citizen.m_instance];
            CitizenInfo info = citizenInstance.Info;
            InstanceID instanceID2 = default(InstanceID);

            info.m_citizenAI.GetLocalizedStatus(followInstance, ref citizen, out instanceID2);
            String buildingName = BuildingManager.instance.GetBuildingName(citizenInstance.m_targetBuilding, default(InstanceID)) ?? "?";
            String altBuildingName = BuildingManager.instance.GetBuildingName(instanceID2.Building, default(InstanceID)) ?? "?";

            if (buildingName != null) {
                return buildingName;
            } else if(altBuildingName != null)
            {
                return altBuildingName;
            }
            else
            {
                return "?";
            }
        }

        private String RaycastRoad(Vector3 position)
        {
            RaycastOutput output = new RaycastOutput();
            RaycastInput raycastInput = new RaycastInput(Camera.main.ScreenPointToRay(camera.transform.position), Camera.main.farClipPlane);
            raycastInput.m_netService.m_service = ItemClass.Service.Road;
            raycastInput.m_netService.m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels;
            raycastInput.m_ignoreSegmentFlags = NetSegment.Flags.None;
            raycastInput.m_ignoreNodeFlags = NetNode.Flags.None;
            raycastInput.m_ignoreTerrain = true;

            ColossalFramework.Math.Segment3 ray = new ColossalFramework.Math.Segment3(position, position + new Vector3(0, -1000, 0));
            bool blah = NetManager.instance.RayCast(raycastInput.m_buildObject as NetInfo, ray, raycastInput.m_netSnap, raycastInput.m_segmentNameOnly, raycastInput.m_netService.m_service, raycastInput.m_netService2.m_service, raycastInput.m_netService.m_subService, raycastInput.m_netService2.m_subService, raycastInput.m_netService.m_itemLayers, raycastInput.m_netService2.m_itemLayers, raycastInput.m_ignoreNodeFlags, raycastInput.m_ignoreSegmentFlags, out position, out output.m_netNode, out output.m_netSegment);
            if (blah)
            {
                return NetManager.instance.GetSegmentName(output.m_netSegment) ?? "?";
            }
            else
            {
                return "?";
            }
        }

        void Awake()
        {
            cameraController = GetComponent<CameraController>();
            camera = GetComponent<Camera>();
            effect = cameraController.GetComponent<DepthOfField>();
            cManager = CitizenManager.instance;
        }

        void Update()
        {
            if (following)
            {
                var citizen = cManager.m_citizens.m_buffer[followInstance];
                var i = citizen.m_instance;
                var flags = cManager.m_instances.m_buffer[i].m_flags;

                if ( inVehicle )
                {
                    if (citizen.m_vehicle == 0)
                    {
                        inVehicle = false;
                        vehicleCamera.StopFollowing();
                        SetFollowInstance(followInstance);
                    }
                    return;
                }
                
                if ((flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted)) != CitizenInstance.Flags.Created)
                {
                    inVehicle = false;
                    StopFollowing();
                    return;
                }

                if ((flags & CitizenInstance.Flags.EnteringVehicle) != 0)
                {                    
                    if ( citizen.m_vehicle != 0 )
                    {
                        ushort vehicleId = citizen.m_vehicle;
                        if((VehicleManager.instance.m_vehicles.m_buffer[vehicleId].Info.GetService() == ItemClass.Service.PublicTransport))
                        {
                            bool isReversed = AIUtils.GetReversedStatus(VehicleManager.instance, vehicleId);
                            vehicleId = isReversed ?
                                VehicleManager.instance.m_vehicles.m_buffer[vehicleId].GetLastVehicle(vehicleId) :
                                VehicleManager.instance.m_vehicles.m_buffer[vehicleId].GetFirstVehicle(vehicleId);
                            inVehicle = true;
                            vehicleCamera.SetFollowInstance(vehicleId);
                            return;
                        }

                    }
                    StopFollowing();
                    return;
                }

                CitizenInstance c = cManager.m_instances.m_buffer[i];
                Vector3 position = Vector3.zero;
                Quaternion orientation = Quaternion.identity;
                c.GetSmoothPosition((ushort)i, out position, out orientation);

                Vector3 forward = orientation * Vector3.forward;
                Vector3 up = orientation * Vector3.up;

                camera.transform.position = GetOffset(position, forward, up) + userOffset;
                Vector3 lookAt = camera.transform.position + (orientation * Vector3.forward) * 1.0f;
                var currentOrientation = camera.transform.rotation;
                camera.transform.LookAt(lookAt, Vector3.up);
                camera.transform.rotation = Quaternion.Slerp(currentOrientation, camera.transform.rotation,
                    Time.deltaTime);

                float height = camera.transform.position.y - TerrainManager.instance.SampleDetailHeight(camera.transform.position);
                cameraController.m_targetPosition = camera.transform.position;

                if(effect)
                {
                    effect.enabled = FPSCamera.instance.config.enableDOF;
                }

                if (FPSCamera.instance.config.displaySpeed)
                {
                    GetInstanceSpeed(camera.transform.position - userOffset);
                }
            }
        }

        public Vector3 GetOffset( Vector3 position, Vector3 forward, Vector3 up)
        {
            Vector3 retVal = position +
                          forward * CameraUtils.CAMERAOFFSETFORWARD +
                          up * CameraUtils.CAMERAOFFSETUP;
            return retVal;
        }
        
    }

}
