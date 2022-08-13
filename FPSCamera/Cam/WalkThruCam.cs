namespace FPSCamera.Cam
{
    using Configuration;
    using CSkyL;
    using CSkyL.Game;
    using CSkyL.Game.ID;
    using CSkyL.Game.Object;
    using CSkyL.Transform;
    using System.Linq;

    public class WalkThruCam : FollowCam, ICamUsingTimer
    {
        public override ObjectID TargetID => _currentCam.TargetID;
        public override IObjectToFollow Target => _currentCam.Target;

        public void SwitchTarget() => _SetRandomCam();
        public void ElapseTime(float seconds) => _elapsedTime += seconds;
        public float GetElapsedTime() => _elapsedTime;

        public override bool Validate()
        {
            if (!IsOperating) return false;

            var ok = _currentCam?.Validate() ?? false;
            if (!Config.G.ManualSwitch4Walk &&
                _elapsedTime > Config.G.Period4Walk) ok = false;
            if (!ok) {
                _SetRandomCam();
                ok = _currentCam?.Validate() ?? false;
                if (!ok) Log.Warn("no target for Walk-Thru mode");
            }
            return ok;
        }

        public override Positioning GetPositioning() => _currentCam.GetPositioning();
        public override float GetSpeed() => _currentCam.GetSpeed();
        public override void InputOffset(Offset inputOffset)
            => _currentCam.InputOffset(inputOffset);
        public override void InputReset() => _currentCam.InputReset();
        public override string GetTargetStatus() => _currentCam.GetTargetStatus();
        public override Utils.Infos GetTargetInfos() => _currentCam.GetTargetInfos();
        public override string SaveOffset() => _currentCam.SaveOffset();

        private void _SetRandomCam()
        {
            _currentCam = null;
            Log.Msg(" -- switching target");

            var list = Vehicle.GetIf((v) => {
                switch (v) {
                case PersonalVehicle _: return Config.G.SelectDriving;
                case TransitVehicle _: return Config.G.SelectPublicTransit;
                case ServiceVehicle _: return Config.G.SelectService;
                case MissionVehicle _: return Config.G.SelectService;
                case CargoVehicle _: return Config.G.SelectCargo;
                default:
                    Log.Warn("WalkThru selection: unknow vehicle type:"
                             + v.GetPrefabName());
                    return false;
                }
            }).OfType<Object>().Concat(
                       Pedestrian.GetIf((p) => {
                           if (p.IsHangingAround) return false;
                           switch (Vehicle.Of(p.RiddenVehicleID)) {
                           case TransitVehicle _: return Config.G.SelectPassenger;
                           case PersonalVehicle _: return false;    // already selected by Vehicle
                           case Vehicle v:
                               Log.Warn("WalkThru selection: unknow pedestrian type: on a "
                                        + v.GetPrefabName());
                               return false;
                           default:
                               return p.IsWaitingTransit ? Config.G.SelectWaiting
                                                         : Config.G.SelectPedestrian;
                           }
                       }).OfType<Object>()).ToList();
            if (!list.Any()) return;

            int attempt = 3;
            do _currentCam = Follow(list.GetRandomOne().ID);
            while (!(_currentCam?.Validate() ?? false) && --attempt >= 0);
            _elapsedTime = 0f;
        }

        private FollowCam _currentCam;
        private float _elapsedTime;
    }
}
