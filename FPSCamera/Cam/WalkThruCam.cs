namespace FPSCamera.Cam
{
    using CSkyL;
    using CSkyL.Game;
    using CSkyL.Game.ID;
    using CSkyL.Game.Object;
    using CSkyL.Transform;
    using System.Linq;

    public class WalkThruCam : FollowCam, ICamUsingTimer
    {
        public override ObjectID TargetID => _currentCam.TargetID;

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
            }
            if (!ok) CSkyL.Log.Warn("no target for Walk-Thru mode");
            return ok;
        }

        public override Positioning GetPositioning() => _currentCam.GetPositioning();

        public override float GetSpeed() => _currentCam.GetSpeed();

        public override void InputOffset(Offset inputOffset)
            => _currentCam.InputOffset(inputOffset);
        public override void InputReset() => _currentCam.InputReset();

        public override string GetTargetStatus() => _currentCam.GetTargetStatus();

        public override Utils.Infos GetTargetInfos() => _currentCam.GetTargetInfos();

        public WalkThruCam(System.Func<Offset, Offset> handler) : base(handler) { }

        private void _SetRandomCam()
        {
            _currentCam = null;

            var list = Pedestrian.GetIf((p) => !p.IsHangingAround)
                                 .OfType<Object>().Concat(
                       Vehicle.GetIf((v) => true).OfType<Object>()).ToList();
            if (!list.Any()) return;

            int attempt = 3;
            do _currentCam = Follow(list.GetRandomOne().ID, _inputOffsetHandler);
            while (!(_currentCam?.Validate() ?? false) && --attempt >= 0);
            _elapsedTime = 0f;
        }

        private FollowCam _currentCam;
        private float _elapsedTime;
    }
}
