namespace FPSCamera.Cam
{
    using CSkyL.Transform;

    public class FreeCam : Base
    {
        public override bool Validate() => true;
        public override Positioning GetPositioning() => _positioning;

        public override float GetSpeed()
            => _lastPosition.DistanceTo(_positioning.position)
                       / CSkyL.Game.Utils.TimeSinceLastFrame;

        public override void InputOffset(Offset inputOffset)
        {
            _lastPosition = _positioning.position;
            _positioning = _positioning.Apply(inputOffset);
            _positioning.angle = _positioning.angle.Clamp(pitchRange:
                    new CSkyL.Math.Range(-Config.G.MaxPitchDeg4Free, Config.G.MaxPitchDeg4Free));

            if (Config.G.GroundClippingOption != Config.GroundClipping.None) {
                var minHeight = CSkyL.Game.Map.GetMinHeightAt(_positioning.position)
                                            + Config.G.GroundLevelOffset;
                if (Config.G.GroundClippingOption == Config.GroundClipping.SnapToGround
                            || _positioning.position.up < minHeight)
                    _positioning.position.up = minHeight;
            }
        }
        public override void InputReset() { }

        public FreeCam(Positioning initPositioning)
        { _positioning = initPositioning; _lastPosition = _positioning.position; }

        private Positioning _positioning;
        private Position _lastPosition;
    }
}
