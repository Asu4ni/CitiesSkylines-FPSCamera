namespace FPSCamera.Cam
{
    using Configuration;
    using CSkyL.Game;
    using CSkyL.Game.ID;
    using CSkyL.Game.Object;
    using CSkyL.Transform;
    using Log = CSkyL.Log;

    public class PedestrianCam : FollowCamWithCam<PedestrianID, Pedestrian, VehicleCam>
    {
        public PedestrianCam(PedestrianID pedID) : base(pedID)
        {
            if (IsOperating)
                Log.Msg($" -- following pedestrian(ID:{_id})");
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
            => Config.G.PedestrianFixedOffset.AsOffSet;

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
        protected override bool _ReadyToSwitchBack {
            get {
                if (_ReadyToSwitchToOtherCam) return false;
                Log.Msg($" -- pedestrian(ID:{_id}) left the vehicle");
                return true;
            }
        }

        protected override VehicleCam _CreateAnotherCam()
        {
            Log.Msg($" -- pedestrian(ID:{_id}) entered a vehicle");
            return new VehicleCam(_target.RiddenVehicleID);
        }
    }
}
