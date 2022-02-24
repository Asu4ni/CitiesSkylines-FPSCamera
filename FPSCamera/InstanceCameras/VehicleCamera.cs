using System;
using UnityEngine;

namespace FPSCamera
{
    public class VehicleCamera : BaseCamera
    {
        private VehicleID vehicleID;
        private bool wasReversed;
        public float cameraRotationOffset = 0u; // TODO: remove

        public VehicleCamera(GameObject parentObject) : base(parentObject) { }

        protected override void SetInstanceToFollowPost()
        {
            vehicleID = followedID.Vehicle;
            wasReversed = FPSVehicle.Of(vehicleID).IsReversed();
        }
        protected override UUID GetIntentedInstance(UUID id)
        {
            if (Config.Global.alwaysFrontVehicle)
                id.Vehicle = FPSVehicle.Of(id.Vehicle).FrontVehicleID();
            return id;
        }

        protected override Vector3 GetVelocity() => FPSVehicle.Of(vehicleID).Velocity();

        protected override string GetDestinationStr()
        {
            // TODO: ensure that using firstVehicle is necessary
            var vehicle = FPSVehicle.Of(FPSVehicle.Of(vehicleID).FrontVehicleID());
            if (vehicle.IsOfService(Service.PublicTransport))
            {
                return vehicle.TransportLineName() ?? unknownStr;
            }
            else if (vehicle.IsOfType(VehicleType.Bicycle))
            {
                // TODO: ensure if alternatives are necessary
                var biker = FPSCitizen.Of(vehicle.OwnerID().Citizen);
                return GeneralUT.GetBuildingName(vehicle.TargetID().Building)
                       ?? GeneralUT.GetBuildingName(biker.TargetBuildingID())
                       ?? GeneralUT.GetBuildingName(biker.TargetID().Building)
                       ?? unknownStr;
            }
            else
            {
                return GeneralUT.GetBuildingName(vehicle.TargetID().Building)
                       ?? GeneralUT.GetBuildingName(vehicle.OwnerID().Building)
                       ?? unknownStr;
            }
        }
        protected override string GetDisplayInfoStr()
        {
            // TODO: ensure if using firstVehicle is necessary
            var vehicle = FPSVehicle.Of(vehicleID);
            if (Config.Global.showPassengerCount && vehicle.IsOfService(Service.PublicTransport))
            {
                vehicle.GetPassengerSizeCapacity(out int size, out int capacity);
                return String.Format("Passengers: {0}/{1}", size, capacity);
            }              
            else return GeneralUT.RaycastRoad(vehicle.Position()) ?? unknownStr;
        }

        protected override bool UpdateCam()
        {
            var vehicle = FPSVehicle.Of(vehicleID);
            if (!(vehicle.Exists() && vehicle.Spawned()))
            {
                StopFollowing();
                return false;
            }
            else if (Config.Global.alwaysFrontVehicle && vehicle.IsReversed() != wasReversed)
            {
                SetInstanceToFollow(vehicle.FrontVehicleID());
                return false;
            }
            else
            {
                vehicle.PositionRotation(out Vector3 position, out Quaternion rotation);

                var forward = rotation * Vector3.forward;
                var up = rotation * Vector3.up;
                var right = rotation * Vector3.right;

                var transform = camera.transform;

                // TODO: necessary?
                var vehicleOffset = forward * vehicle.AttachOffsetFront();
                if (!vehicle.IsLeading() && !vehicle.IsTrailing())
                {
                    vehicleOffset += up * 3.0f;
                    vehicleOffset -= forward * 2.0f;
                }

                // TODO: always look toward moving direction, hot key to change direction

                transform.position = CameraUT.CamPosition(position, forward, up)
                                     + vehicleOffset + userOffset
                                     + forward * Config.Global.vehicleCameraOffsetX
                                     + up * Config.Global.vehicleCameraOffsetY
                                     + right * Config.Global.vehicleCameraOffsetZ;
                var oldRotation = transform.rotation;
                transform.LookAt(transform.position + rotation * Vector3.forward, Vector3.up);
                transform.rotation = Quaternion.Slerp(oldRotation, transform.rotation,
                                                      Time.deltaTime); // TODO: original code: deltaTime * 1.5f

                float height = camera.transform.position.y - TerrainManager.instance.SampleDetailHeight(camera.transform.position);
                cameraController.m_targetPosition = camera.transform.position;

                return true;
            }      
        }
    }

}
