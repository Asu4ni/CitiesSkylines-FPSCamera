using System;
using ColossalFramework;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using static ToolBase;

namespace FPSCamera
{

    public class VehicleCamera : MonoBehaviour, InstanceCamera
    {
        private ushort followInstance;

        public bool following = false;

        private CameraController cameraController;
        public Camera camera;
        public DepthOfField effect;

        private VehicleManager vManager;
        private bool isReversed;
        public Vector3 userOffset = Vector3.zero;

        private Vector3 vehicleVelocity = Vector3.zero;

        public int cameraRotationOffset = 0;

        void Awake()
        {
            cameraController = GetComponent<CameraController>();
            camera = GetComponent<Camera>();
            effect = cameraController.GetComponent<DepthOfField>();
            vManager = VehicleManager.instance;
        }

        void LateUpdate()
        {
            if (following)
            {

                if ((vManager.m_vehicles.m_buffer[followInstance].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) != Vehicle.Flags.Created)
                {
                    StopFollowing();
                    return;
                }

                if ((vManager.m_vehicles.m_buffer[followInstance].m_flags & Vehicle.Flags.Spawned) == 0)
                {
                    StopFollowing();
                    return;
                }

                bool currentReversedStatus = GameUtils.GetReversedStatus(vManager, followInstance);

                if (FPSCamera.instance.config.alwaysFrontVehicle)
                {
                    if (currentReversedStatus != isReversed)
                    {
                        followInstance = currentReversedStatus ?
                            vManager.m_vehicles.m_buffer[followInstance].GetLastVehicle(followInstance) :
                            vManager.m_vehicles.m_buffer[followInstance].GetFirstVehicle(followInstance);
                    }
                    isReversed = currentReversedStatus;
                }

                Vehicle v = vManager.m_vehicles.m_buffer[followInstance];

                Vector3 position = Vector3.zero;
                Quaternion orientation = Quaternion.identity;
                v.GetSmoothPosition((ushort)followInstance, out position, out orientation);
                Vector3 forward = orientation * Vector3.forward;
                Vector3 up = orientation * Vector3.up;
                Vector3 right = orientation * Vector3.right;

                Vector3 vehicleOffset = forward * v.Info.m_attachOffsetFront;
                if (v.m_leadingVehicle != 0 && v.m_trailingVehicle != 0)
                {
                    vehicleOffset += up * 3.0f;
                    vehicleOffset -= forward * 2.0f;
                }

                var pos = GetOffset(position, forward, up) + vehicleOffset +
                          forward * FPSCamera.instance.config.vehicleCameraOffsetX +
                          up * FPSCamera.instance.config.vehicleCameraOffsetY +
                          right * FPSCamera.instance.config.vehicleCameraOffsetZ;
                camera.transform.position = pos + this.userOffset;

                Vector3 offset = v.GetSmoothVelocity(followInstance);
                Vector3 direction = Vector3.forward;

                if ((v.Info.m_vehicleType == VehicleInfo.VehicleType.Tram ||
                    v.Info.m_vehicleType == VehicleInfo.VehicleType.Metro ||
                    v.Info.m_vehicleType == VehicleInfo.VehicleType.Train ||
                    v.Info.m_vehicleType == VehicleInfo.VehicleType.Monorail))
                {
                    bool isLastCarInTrain = (currentReversedStatus && v.m_trailingVehicle != 0 && v.m_leadingVehicle == 0) ||
                        (!currentReversedStatus && v.m_trailingVehicle == 0 && v.m_leadingVehicle != 0);
                    if (isLastCarInTrain)
                    {
                        offset *= -1;
                    }
                }
                if (offset.magnitude > 1)
                {
                    vehicleVelocity = offset;
                }

                if (Vector3.Dot(orientation * Vector3.forward, vehicleVelocity) < 0)
                {
                    direction = Vector3.back;
                }


                Vector3 lookAt = pos + (orientation * direction) * 1f;

                var currentOrientation = camera.transform.rotation;
                camera.transform.LookAt(lookAt, Vector3.up);
                camera.transform.Rotate(new Vector3(0, cameraRotationOffset));
                camera.transform.rotation = Quaternion.Slerp(currentOrientation, camera.transform.rotation,
                    Time.deltaTime * 1.5f);

                float height = camera.transform.position.y - TerrainManager.instance.SampleDetailHeight(camera.transform.position);
                cameraController.m_targetPosition = camera.transform.position;

                if (effect)
                {
                    effect.enabled = FPSCamera.instance.config.enableDOF;
                }
                if (FPSCamera.instance.config.displaySpeed)
                {
                    GetInstanceSpeed(pos);
                }
            }
        }

        public void GetInstanceSpeed(Vector3 position)
        {

            Vehicle v = vManager.m_vehicles.m_buffer[followInstance];
            Vector3 velocity = v.GetSmoothVelocity(followInstance);
            FPSCameraSpeedUI.Instance.speed = velocity.magnitude;
            FPSCameraSpeedUI.Instance.destinationName = GetDestination();

            ushort firstVehicle = vManager.m_vehicles.m_buffer[(int)followInstance].GetFirstVehicle(followInstance);
            VehicleInfo info = vManager.m_vehicles.m_buffer[firstVehicle].Info;
            FPSCameraSpeedUI.Instance.passengersOrStreet =
                    (info.GetService() == ItemClass.Service.PublicTransport &&
                            FPSCamera.instance.config.showPassengerCount)
                    ? GetPassengerNumbers() : FPSCameraSpeedUI.Instance.passengersOrStreet = RaycastRoad(position);
        }

        public void SetFollowInstance(uint instance)
        {
            this.enabled = true;
            FPSCamera.instance.SetMode(false);

            followInstance = (ushort)instance;
            isReversed = GameUtils.GetReversedStatus(vManager, followInstance);
            if (FPSCamera.instance.config.alwaysFrontVehicle)
            {
                followInstance = isReversed ?
                    vManager.m_vehicles.m_buffer[followInstance].GetLastVehicle(followInstance) :
                    vManager.m_vehicles.m_buffer[followInstance].GetFirstVehicle(followInstance);
            }
            following = true;

            CameraUtils.SetCamera(cameraController, camera);
            if (FPSCamera.instance.config.displaySpeed)
            {
                FPSCameraSpeedUI.Instance.enabled = true;
            }
            FPSCamera.onCameraModeChanged(true);
            userOffset = Vector3.zero;
            vehicleVelocity = Vector3.zero;
            cameraRotationOffset = 0;
        }

        public void StopFollowing()
        {
            followInstance = 0;
            following = false;
            FPSCameraSpeedUI.Instance.enabled = false;
            CameraUtils.StopCamera(cameraController, camera);
            FPSCamera.onCameraModeChanged(false);
            this.enabled = false;
        }

        public Vector3 GetOffset(Vector3 position, Vector3 forward, Vector3 up)
        {
            Vector3 retVal = position +
                            forward * CameraUtils.CameraOffsetForward +
                            up * CameraUtils.CameraOffsetUp;

            return retVal;
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
            bool success = NetManager.instance.RayCast(raycastInput.m_buildObject as NetInfo, ray, raycastInput.m_netSnap, raycastInput.m_segmentNameOnly, raycastInput.m_netService.m_service, raycastInput.m_netService2.m_service, raycastInput.m_netService.m_subService, raycastInput.m_netService2.m_subService, raycastInput.m_netService.m_itemLayers, raycastInput.m_netService2.m_itemLayers, raycastInput.m_ignoreNodeFlags, raycastInput.m_ignoreSegmentFlags, out position, out output.m_netNode, out output.m_netSegment);
            if (success)
            {
                return NetManager.instance.GetSegmentName(output.m_netSegment) ?? "  ";
            }
            else
            {
                return "  ";
            }
        }

        private string GetDestination()
        {
            Vehicle v = vManager.m_vehicles.m_buffer[followInstance];
            ushort firstVehicle = vManager.m_vehicles.m_buffer[(int)followInstance].GetFirstVehicle(followInstance);
            v = vManager.m_vehicles.m_buffer[firstVehicle];
            InstanceID instanceID2 = default(InstanceID);

            VehicleInfo info = vManager.m_vehicles.m_buffer[firstVehicle].Info;

            info.m_vehicleAI.GetLocalizedStatus(firstVehicle, ref vManager.m_vehicles.m_buffer[firstVehicle], out instanceID2);
            if (info.GetService() == ItemClass.Service.PublicTransport)
            {
                return TransportManager.instance.GetLineName(v.m_transportLine) ?? "  ";
            }
            else if (info.m_vehicleType == VehicleInfo.VehicleType.Bicycle)
            {
                uint driverInstance = v.Info.m_vehicleAI.GetOwnerID(firstVehicle, ref v).Citizen;
                var citizen = CitizenManager.instance.m_citizens.m_buffer[driverInstance];
                CitizenInstance citizenInstance = CitizenManager.instance.m_instances.m_buffer[citizen.m_instance];
                CitizenInfo citizenInfo = citizenInstance.Info;
                InstanceID citizenInstanceId = default(InstanceID);

                citizenInfo.m_citizenAI.GetLocalizedStatus(driverInstance, ref citizen, out citizenInstanceId);
                String buildingName = BuildingManager.instance.GetBuildingName(citizenInstance.m_targetBuilding, default(InstanceID)) ?? "?";
                String altBuildingName = BuildingManager.instance.GetBuildingName(citizenInstanceId.Building, default(InstanceID)) ?? "?";

                if (buildingName != null)
                {
                    return buildingName;
                }
                else if (altBuildingName != null)
                {
                    return altBuildingName;
                }
                else
                {
                    return "?";
                }
            }
            else
            {
                info.m_vehicleAI.GetLocalizedStatus(firstVehicle, ref vManager.m_vehicles.m_buffer[firstVehicle], out instanceID2);
                String targetBuilding = BuildingManager.instance.GetBuildingName(instanceID2.Building, default(InstanceID));
                InstanceID ownerID = info.m_vehicleAI.GetOwnerID(firstVehicle, ref vManager.m_vehicles.m_buffer[firstVehicle]);
                String ownerBuilding = BuildingManager.instance.GetBuildingName(ownerID.Building, default(InstanceID));

                if (targetBuilding != null)
                {
                    return targetBuilding;
                }
                else if (ownerBuilding != null)
                {
                    return ownerBuilding;
                }
                else
                {
                    return "?";
                }

            }
        }

        private string GetPassengerNumbers()
        {
            Vehicle v = vManager.m_vehicles.m_buffer[followInstance];
            ushort firstVehicle = vManager.m_vehicles.m_buffer[(int)followInstance].GetFirstVehicle(followInstance);
            v = vManager.m_vehicles.m_buffer[firstVehicle];
            VehicleInfo info = vManager.m_vehicles.m_buffer[firstVehicle].Info;

            int fill = 0;
            int cap = 0;

            info.m_vehicleAI.GetBufferStatus(firstVehicle, ref v, out string text, out fill, out cap);

            return String.Format("Passengers: {0}/{1}", fill, cap);
        }

    }

}
