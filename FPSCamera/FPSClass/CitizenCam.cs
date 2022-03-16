namespace FPSCamMod
{
    internal class CitizenCam : FPSCam
    {
        public CitizenCam(CitizenID idToFollow)
        {
            citizenID = idToFollow;
            if (GetCitizen().IsValid)
                Log.Msg($"start following citizen(ID:{citizenID})");
            else {
                Log.Warn($"citizen(ID:{citizenID}) to follow does not exist");
                state = State.finished;
            }
        }

        private FPSCitizen GetCitizen() => FPSCitizen.Of(citizenID);
        public override FPSInstanceToFollow GetFollowed() => GetCitizen();

        public override CamSetting TryGetCamSetting()
        {
            var citizen = GetCitizen();

            if (!citizen.IsValid) {
                Log.Msg($"citizen(ID:{citizenID}) disappears");
                state = State.finished;
                return CamSetting.Identity;
            }

            var vehicleID = citizen.RiddenVehicleID;
            if (state == State.normal && vehicleID.Exists) {
                if (vehicleID.Exists) {
                    Log.Msg($"citizen(ID:{citizenID}) entered a vehicle");
                    state = State.idle;
                    vehicleCamera = new VehicleCam(vehicleID);
                }
                else {
                    Log.Warn($"vehicle of citizen (ID:{citizenID}) not found while the citizen entering it");
                    state = State.finished;
                    return CamSetting.Identity;
                }
            }
            else if (vehicleCamera is object) {
                if (citizen.RiddenVehicleID.Exists && vehicleCamera.IsOperating) {
                    var vSetting = vehicleCamera.TryGetCamSetting();
                    if (vehicleCamera.IsOperating) return vSetting;
                }
                Log.Msg($"citizen(ID:{citizenID}) left the vehicle");
                vehicleCamera = null;
                state = State.normal;
            }

            var setting = citizen.GetCamSetting();
            var offset = CamUT.GetOffset(setting.rotation,
                                    Config.G.CitizenCamOffset.forward,
                                    Config.G.CitizenCamOffset.up + Config.G.CitizenFOffsetUp,
                                    Config.G.CitizenCamOffset.right);

            return new CamSetting(setting.position + offset, setting.rotation);
        }

        public override float GetSpeed()
            => state == State.idle && vehicleCamera is object ?
                    vehicleCamera.GetSpeed() : GetCitizen().GetSpeed();
        public override string GetInstanceName() => GetCitizen().GetName();
        public override string GetInstanceStatus()
        {
            var status = GetCitizen().GetStatus();
            if (state == State.idle && vehicleCamera is object)
                status += $" | ON {vehicleCamera.GetInstanceName()}: " +
                                $"{vehicleCamera.GetInstanceStatus()}";
            return status;
        }
        public override FPSInstanceToFollow.Details GetDetails()
        {
            var details = GetCitizen().GetDetails();
            if (state == State.idle && vehicleCamera is object)
                foreach (var pair in vehicleCamera.GetDetails())
                    details["Vehicle/" + pair.field] = pair.text;

            return details;
        }

        private CitizenID citizenID;
        private VehicleCam vehicleCamera = null;
    }
}
