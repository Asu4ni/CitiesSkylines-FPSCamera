namespace FPSCamera.Cam
{
    using Transform;
    using Wrapper;

    public class VehicleCam : FollowCam<VehicleID, Vehicle>
    {
        public VehicleCam(VehicleID vehicleID, System.Func<Offset, Offset> handler)
                    : base(vehicleID, handler)
        {
            if (!IsOperating) {
                Log.Warn($"vehicle(ID:{_id}) to follow does not exist");
                return;
            }
            if (Config.G.StickToFrontVehicle &&
                !_SwitchTarget(_target.GetFrontVehicleID())) {
                Log.Warn($"vehicle(ID:{_id}) to follow does not exist");
                return;
            }
            Log.Msg($"following vehicle(ID:{_id})");
            _wasReversed = _target.IsReversed;
        }

        public override bool Validate()
        {
            if (!base.Validate()) return false;

            if (_target.IsReversed != _wasReversed) {
                Log.Msg($"vehicle(ID:{_id}) changes direction");
                _wasReversed = !_wasReversed;
                if (Config.G.StickToFrontVehicle &&
                    !_SwitchTarget(_target.GetFrontVehicleID())) return false;
            }
            return true;
        }

        protected override Offset _LocalOffset
            => new Offset(new LocalMovement
            {
                forward = Config.G.VehicleCamOffset.forward +
                          Config.G.VehicleFOffsetForward + _target.GetAttachOffsetFront(),
                up = Config.G.VehicleCamOffset.up + Config.G.VehicleFOffsetUp +
                            (_target.IsMiddle ? Config.G.MiddleVehicleFOffsetUp : 0f),
                right = Config.G.VehicleCamOffset.right
            }, DeltaAttitude.None);

        private bool _wasReversed;
    }
}