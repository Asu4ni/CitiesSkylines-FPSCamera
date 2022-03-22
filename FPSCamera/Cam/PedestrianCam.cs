namespace FPSCamera.Cam
{
    using Transform;
    using Wrapper;

    public class PedestrianCam : FollowCamWithCam<PedestrianID, Pedestrian, VehicleCam>
    {
        public PedestrianCam(PedestrianID pedID, System.Func<Offset, Offset> handler)
                    : base(pedID, handler)
        {
            if (IsOperating)
                Log.Msg($"following pedestrian(ID:{_id})");
            else
                Log.Warn($"pedestrian(ID:{_id}) to follow does not exist");
        }

        public override string GetTargetStatus()
        {
            var status = _target.GetStatus();
            if (_state is UsingOtherCam)
                status = $"using \"{_camOther.Target.Name}\" for {status}";
            return status;
        }

        protected override Offset _LocalOffset
            => new Offset(new LocalMovement
            {
                forward = Config.G.PedestrianCamOffset.forward,
                up = Config.G.PedestrianCamOffset.up + Config.G.PedestrianFOffsetUp,
                right = Config.G.PedestrianCamOffset.right
            }, DeltaAttitude.None);

        public override Utils.Infos GetTargetInfos()
        {
            var details = _target.GetInfos();
            if (_state is UsingOtherCam) {
                details["v/status"] = _camOther.GetTargetStatus();
                foreach (var pair in _camOther.GetTargetInfos())
                    details["v/" + pair.field] = pair.text;
            }

            return details;
        }

        protected override bool _ReadyToSwitchToOtherCam
            => _target.RiddenVehicleID is VehicleID && !_target.IsEnteringVehicle;
        protected override bool _ReadyToSwitchBack => !_ReadyToSwitchToOtherCam;
        protected override VehicleCam _CreateAnotherCam()
        {
            Log.Msg($"pedestrian(ID:{_id}) entered a vehicle");
            return new VehicleCam(_target.RiddenVehicleID, _inputOffsetHandler);
        }
    }
}
