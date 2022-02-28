using System;
using UnityEngine;

namespace FPSCamMod
{
    internal class VehicleCam : FPSCam
    {
        // TODO: move to config
        private static readonly Vector3 middleVehicelOffset
                = Vector3.up * 3.0f + Vector3.forward * 2.0f;
        private readonly Vector3 camOffset = new Vector3(0f, 3f, 0f);

        private VehicleID vehicleID;
        private bool wasReversed;

        public VehicleCam(UUID idToFollow) : base()
        {
            vehicleID = idToFollow.Vehicle;
            if (Config.Global.alwaysFrontVehicle)
                vehicleID = FPSVehicle.Of(vehicleID).FrontVehicleID();

            if (FPSVehicle.Of(vehicleID).exists)
            {
                Log.Msg($"start following vehicle(ID:{vehicleID})");
                wasReversed = FPSVehicle.Of(vehicleID).isReversed;
            }
            else
            {
                Log.Warn($"vehicle(ID:{vehicleID}) to follow does not exist");
                state = State.stopped;
            }
        }

        public override Vector3 GetVelocity() => FPSVehicle.Of(vehicleID).Velocity();
        public override string GetDestinationStr()
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
                       ?? GeneralUT.GetBuildingName(biker.targetBuildingID)
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
        public override string GetDisplayInfoStr()
        {
            var vehicle = FPSVehicle.Of(FPSVehicle.Of(vehicleID).FrontVehicleID());
            if (Config.Global.showPassengerCount && vehicle.IsOfService(Service.PublicTransport))
            {
                vehicle.GetPassengerSizeCapacity(out int size, out int capacity);
                return String.Format("Passengers:{0,4} /{1,4}", size, capacity);
            }
            // TODO: investigae: not working
            else return GeneralUT.RaycastRoad(vehicle.Position()) ?? unknownStr;
        }

        public override CamSetting GetNextCamSetting()
        {
            var vehicle = FPSVehicle.Of(vehicleID);

            if (!(vehicle.exists && vehicle.spawned))
            {
                Log.Msg($"vehicle(ID:{vehicleID}) disappears");
                state = State.stopped;
                return CamSetting.Identity;
            }

            if (Config.Global.alwaysFrontVehicle && vehicle.isReversed != wasReversed)
            {
                Log.Msg($"vehicle(ID:{vehicleID}) changes direction");
                vehicleID = vehicle.FrontVehicleID();
            }

            // TODO: detect changing direction

            vehicle.PositionRotation(out Vector3 position, out Quaternion rotation);

            // TODO: necessary?
            var vehicleOffset = CamUT.GetOffset(rotation, vehicle.AttachOffsetFront(), .0f, .0f)
                                + CamUT.GetOffset(rotation, Config.Global.vehicleCameraOffsetX,
                                                            Config.Global.vehicleCameraOffsetY,
                                                            Config.Global.vehicleCameraOffsetZ);
            if (!vehicle.isLeading && !vehicle.isTrailing)
                vehicleOffset += rotation * middleVehicelOffset;

            // TODO: always look toward moving direction, hot key to change direction
            return new CamSetting(position + vehicleOffset + camOffset, rotation);
        }
    }

}
