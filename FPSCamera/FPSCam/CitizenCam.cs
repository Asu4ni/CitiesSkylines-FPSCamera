using UnityEngine;

namespace FPSCamMod
{
    internal class CitizenCam : FPSCam
    {
        public CitizenCam(UUID idToFollow) : base()
        {
            citizenID = idToFollow.Citizen;
            if (FPSCitizen.Of(citizenID).exists)
                Log.Msg($"start following citizen(ID:{citizenID})");
            else {
                Log.Warn($"citizen(ID:{citizenID}) to follow does not exist");
                state = State.stopped;
            }
        }

        public override Vector3 GetVelocity()
                => state == State.waiting && vehicleCamera is object ?
                    vehicleCamera.GetVelocity() : FPSCitizen.Of(citizenID).Velocity();
        public override string GetDestinationStr()
        {
            var citizen = FPSCitizen.Of(citizenID);
            var str = GameUT.GetBuildingName(citizen.targetBuildingID)
                   ?? GameUT.GetBuildingName(citizen.TargetID().Building)
                   ?? unknownStr;
            if (state == State.waiting && vehicleCamera is object)
                str += "\n-Vehicle > " + vehicleCamera.GetDestinationStr();

            return str;
        }
        public override string GetDisplayInfoStr()
        {
            // TODO: integrate RaycastRoad
            var info = $"Name> {FPSCitizen.Of(citizenID).Name()}";
            if (state == State.waiting && vehicleCamera is object)
                info += "\n--- Vehicle ---\n" + vehicleCamera.GetDisplayInfoStr();
            return info;
        }

        public override CamSetting GetNextCamSetting()
        {
            var citizen = FPSCitizen.Of(citizenID);

            if (state == State.following && citizen.isEnteringVehicle) {
                var vehicleID = citizen.riddenVehicleID;
                if (vehicleID.exists) {
                    Log.Msg($"citizen(ID:{citizenID}) entering a vehicle");
                    state = State.waiting;
                    vehicleCamera = new VehicleCam(FPSVehicle.Of(vehicleID).FrontVehicleID());
                }
                else {
                    Log.Warn($"vehicle of citizen (ID:{citizenID}) not found while the citizen entering it");
                    state = State.stopped;
                    return CamSetting.Identity;
                }
            }

            if (vehicleCamera is object) {
                if (citizen.riddenVehicleID.exists && vehicleCamera.isRunning) {
                    var setting = vehicleCamera.GetNextCamSetting();
                    if (vehicleCamera.isRunning) return setting;
                }
                Log.Msg($"citizen(ID:{citizenID}) leaving the vehicle");
                vehicleCamera = null;
                state = State.following;
            }

            if (!citizen.exists) {
                Log.Msg($"citizen(ID:{citizenID}) disappears");
                state = State.stopped;
                return CamSetting.Identity;
            }

            citizen.PositionRotation(out Vector3 position, out Quaternion rotation);

            var offset = CamUT.GetOffset(rotation,
                                    Config.G.CitizenCamOffset.forward,
                                    Config.G.CitizenCamOffset.up + Config.G.CitizenFOffsetUp,
                                    Config.G.CitizenCamOffset.right);

            return new CamSetting(position + offset, rotation);
        }

        private CitizenID citizenID;
        private VehicleCam vehicleCamera = null;
    }
}
