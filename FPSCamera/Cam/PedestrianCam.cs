namespace FPSCamera.Cam
{
    using Transform;
    using Wrapper;

    public class PedestrianCam : Follow<PedestrianID, Wrapper.Pedestrian>
    {
        public PedestrianCam(PedestrianID pedID) : base(pedID)
        {
            if (IsOperating)
                Log.Msg($"start following pedestrian(ID:{_id})");
            else
                Log.Warn($"pedestrian(ID:{_id}) to follow does not exist");
        }

        protected override Positioning _GetPositioning()
        {
            var pedestrian = Target;
            if (state == State.Normal && pedestrian.RiddenVehicleID is VehicleID vehicleID) {
                Log.Msg($"pedestrian(ID:{_id}) entered a vehicle");
                state = State.Idle;
                _camVehicle = new VehicleCam(vehicleID);
            }

            if (_camVehicle is object) {
                if (pedestrian.RiddenVehicleID is VehicleID &&
                    _camVehicle.GetPositioning() is Positioning p) return p;

                Log.Msg($"pedestrian(ID:{_id}) left the vehicle");
                _camVehicle = null;
                state = State.Normal;
            }

            return pedestrian.GetPositioning().Apply(new LocalMovement
            {
                forward = Config.G.CitizenCamOffset.forward,
                up = Config.G.CitizenCamOffset.up + Config.G.CitizenFOffsetUp,
                right = Config.G.CitizenCamOffset.right
            });
        }

        protected override float _GetSpeed()
            => state == State.Idle && _camVehicle is object ?
                    _camVehicle.GetSpeed() : Target.GetSpeed();
        protected override string _GetStatus()
        {
            var status = Target.GetStatus();
            if (state == State.Idle && _camVehicle is object)
                status += $" | ON {_camVehicle.GetName()}: " + $"{_camVehicle.GetStatus()}";
            return status;
        }
        protected override Utils.Infos _GetInfos()
        {
            var details = Target.GetInfos();
            if (state == State.Idle && _camVehicle is object)
                foreach (var pair in _camVehicle.GetInfos())
                    details["v/" + pair.field] = pair.text;

            return details;
        }

        private VehicleCam _camVehicle = null;
    }
}
