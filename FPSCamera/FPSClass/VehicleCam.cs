namespace FPSCamMod
{
    internal class VehicleCam : FPSCam
    {
        public VehicleCam(VehicleID idToFollow)
        {
            vehicleID = idToFollow;
            if (Config.G.StickToFrontVehicle)
                vehicleID = GetVehicle().GetFrontVehicleID();

            var vehicle = GetVehicle();
            if (vehicle.IsValid) {
                Log.Msg($"start following vehicle(ID:{vehicleID})");
                wasReversed = vehicle.IsReversed;
            }
            else {
                Log.Warn($"vehicle(ID:{vehicleID}) to follow does not exist");
                state = State.finished;
            }
        }

        private FPSVehicle GetVehicle() => FPSVehicle.Of(vehicleID);
        public override FPSInstanceToFollow GetFollowed() => GetVehicle();

        public override CamSetting TryGetCamSetting()
        {
            var vehicle = GetVehicle();

            if (!(vehicle.IsValid && vehicle.IsSpawned)) {
                Log.Msg($"vehicle(ID:{vehicleID}) disappears");
                state = State.finished;
                return CamSetting.Identity;
            }

            if (Config.G.StickToFrontVehicle && vehicle.IsReversed != wasReversed) {
                Log.Msg($"vehicle(ID:{vehicleID}) changes direction");
                vehicleID = vehicle.GetFrontVehicleID();
                wasReversed = !wasReversed;
            }

            var setting = vehicle.GetCamSetting();
            // TODO: ensure AttachOffsetFront
            var offset = CamUT.GetOffset(setting.rotation,
                    Config.G.VehicleCamOffset.forward + Config.G.VehicleFOffsetForward
                        + vehicle.GetAttachOffsetFront(),
                    Config.G.VehicleCamOffset.up + Config.G.VehicleFOffsetUp
                        + (vehicle.IsLeading || vehicle.IsTrailing ?
                          0f : Config.G.MiddleVehicleFOffsetUp),
                    Config.G.VehicleCamOffset.right);

            return new CamSetting(setting.position + offset, setting.rotation);
        }

        private VehicleID vehicleID;
        private bool wasReversed;
    }
}
