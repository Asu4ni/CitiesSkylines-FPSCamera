namespace FPSCamera.Cam
{
    using Transform;
    using Wrapper;

    internal class Vehicle : Follow<VehicleID, Wrapper.Vehicle>
    {
        public Vehicle(VehicleID vehicleID) : base(vehicleID)
        {
            if (Config.G.StickToFrontVehicle)
                _id = Target.GetFrontVehicleID();

            var vehicle = Target;
            if (vehicle.IsValid) {
                Log.Msg($"start following vehicle(ID:{_id})");
                _wasReversed = vehicle.IsReversed;
            }
            else {
                Log.Warn($"vehicle(ID:{_id}) to follow does not exist");
                state = State.Finished;
            }
        }

        protected override Positioning _GetPositioning()
        {
            var vehicle = Target;
            if (Config.G.StickToFrontVehicle && vehicle.IsReversed != _wasReversed) {
                Log.Msg($"vehicle(ID:{_id}) changes direction");
                _id = vehicle.GetFrontVehicleID();
                _wasReversed = !_wasReversed;
            }

            return vehicle.GetCamPositioning().Apply(new LocalMovement
            {
                forward = Config.G.VehicleCamOffset.forward +
                          Config.G.VehicleFOffsetForward + vehicle.GetAttachOffsetFront(),
                up = Config.G.VehicleCamOffset.up + Config.G.VehicleFOffsetUp +
                     (vehicle.IsMiddle ? Config.G.MiddleVehicleFOffsetUp : 0f),
                right = Config.G.VehicleCamOffset.right
            });
        }

        private bool _wasReversed;
    }
}
