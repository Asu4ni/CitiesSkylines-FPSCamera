using UnityEngine;

namespace FPSCamMod
{
    internal class VehicleCam : FPSCam
    {
        private VehicleID vehicleID;
        private bool wasReversed;

        public VehicleCam(UUID idToFollow) : base()
        {
            vehicleID = idToFollow.Vehicle;
            if (Config.G.StickToFrontVehicle)
                vehicleID = FPSVehicle.Of(vehicleID).FrontVehicleID();

            if (FPSVehicle.Of(vehicleID).exists) {
                Log.Msg($"start following vehicle(ID:{vehicleID})");
                wasReversed = FPSVehicle.Of(vehicleID).isReversed;
            }
            else {
                Log.Warn($"vehicle(ID:{vehicleID}) to follow does not exist");
                state = State.stopped;
            }
        }

        public override Vector3 GetVelocity() => FPSVehicle.Of(vehicleID).Velocity();
        public override string GetDestinationStr()
        {
            var vehicle = FPSVehicle.Of(FPSVehicle.Of(vehicleID).FrontVehicleID());
            if (vehicle.IsOfType(VehicleType.Bicycle)) {
                // TODO: ensure if alternatives are necessary
                var biker = FPSCitizen.Of(vehicle.OwnerID().Citizen);
                return GameUT.GetBuildingName(vehicle.TargetID().Building)
                       ?? GameUT.GetBuildingName(biker.targetBuildingID)
                       ?? GameUT.GetBuildingName(biker.TargetID().Building)
                       ?? unknownStr;
            }
            else {
                return GameUT.GetBuildingName(vehicle.TargetID().Building)
                       ?? GameUT.GetBuildingName(vehicle.OwnerID().Building)
                       ?? unknownStr;
            }
        }
        public override string GetDisplayInfoStr()
        {
            var vehicle = FPSVehicle.Of(FPSVehicle.Of(vehicleID).FrontVehicleID());
            string info = "";
            if (vehicle.IsOfService(Service.PublicTransport)) {
                info += $"Service> {vehicle.TransportLineName() ?? unknownStr}\n";
                vehicle.GetPassengerSizeCapacity(out int size, out int capacity);
                info += $"Passenger>{size,4} /{capacity,4}\n";
            }
            // TODO: investigae: not working
            info += $"At> {GameUT.RaycastRoad(vehicle.Position()) ?? unknownStr}";
            return info;
        }

        public override CamSetting GetNextCamSetting()
        {
            var vehicle = FPSVehicle.Of(vehicleID);

            if (!(vehicle.exists && vehicle.spawned)) {
                Log.Msg($"vehicle(ID:{vehicleID}) disappears");
                state = State.stopped;
                return CamSetting.Identity;
            }

            if (Config.G.StickToFrontVehicle && vehicle.isReversed != wasReversed) {
                Log.Msg($"vehicle(ID:{vehicleID}) changes direction");
                vehicleID = vehicle.FrontVehicleID();
            }

            // TODO: detect changing direction

            vehicle.PositionRotation(out Vector3 position, out Quaternion rotation);

            // TODO: ensure AttachOffsetFront

            var offset = CamUT.GetOffset(rotation,
                    Config.G.VehicleCamOffset.forward + Config.G.VehicleFOffsetForward
                        + vehicle.AttachOffsetFront(),
                    Config.G.VehicleCamOffset.up + Config.G.VehicleFOffsetUp
                        + (vehicle.isLeading || vehicle.isTrailing ?
                          0f : Config.G.MiddleVehicleFOffsetUp),
                    Config.G.VehicleCamOffset.right);

            // TODO: always look toward moving direction, hot key to change direction
            return new CamSetting(position + offset, rotation);
        }
    }

}
