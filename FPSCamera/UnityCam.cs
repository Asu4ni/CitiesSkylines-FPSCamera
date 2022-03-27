namespace FPSCamera
{
    using CSkyL;
    using CSkyL.Transform;
    using Range = CSkyL.Math.Range;

    public class UnityCam : CSkyL.Game.Cam
    {
        // Getter => current setting / Setter: set target setting
        public override Positioning Positioning {
            set => _targetSetting.positioning = value;
        }
        public override RenderArea Area {
            set => _targetSetting.area = value;
        }
        public override float FieldOfView {
            set => _targetSetting.fieldOfView = value;
        }
        public override float NearClipPlane {
            set => _targetSetting.nearClipPlane = value;
        }
        public override Setting AllSetting {
            set => _targetSetting = value;
        }
        public Positioning TargetPositioning => _targetSetting.positioning;
        public float TargetFoV => _targetSetting.fieldOfView;

        public void ResetTarget() => _targetSetting = AllSetting;
        public bool AlmostAtTarget() => AllSetting.AlmostEquals(_targetSetting);

        public void AdvanceToTargetSmooth(float advanceRatio = 1f, bool instantMove = false,
                                          bool instantAngle = false, bool instantRRect = false,
                                          bool instantFoV = false, bool instantNCPlane = false)
            => AdvanceToTarget(advanceRatio, !instantMove, !instantAngle,
                               !instantRRect, !instantFoV, !instantNCPlane);
        public void AdvanceToTarget(float advanceRatio = 1f, bool smoothMove = false,
                                          bool smoothAngle = false, bool smoothArea = false,
                                          bool smoothFoV = false, bool smoothNCPlane = false)
        {
            var current = AllSetting;

            base.Positioning = new Positioning(
                smoothMove ? current.positioning.position.AdvanceToTarget(
                                 _targetSetting.positioning.position, advanceRatio,
                                 new Range(Config.G.MinTransMove, Config.G.MaxTransMove)) :
                             _targetSetting.positioning.position,
                smoothAngle ? current.positioning.angle.AdvanceToTarget(
                                  _targetSetting.positioning.angle, advanceRatio,
                                  new Range(Config.G.MinTransRotate, Config.G.MaxTransRotate)) :
                              _targetSetting.positioning.angle
            );

            base.Area = smoothArea ?
                            current.area.AdvanceToTarget(_targetSetting.area, advanceRatio) :
                            _targetSetting.area;

            base.FieldOfView = smoothFoV ? current.fieldOfView.AdvanceToTarget(
                                               _targetSetting.fieldOfView, advanceRatio,
                                               new Range(.25f, 5f)) :
                                           _targetSetting.fieldOfView;

            base.NearClipPlane = smoothNCPlane ? current.nearClipPlane.AdvanceToTarget(
                                                     _targetSetting.nearClipPlane, advanceRatio,
                                                     new Range(.25f, 5f)) :
                                                 _targetSetting.nearClipPlane;
        }

        public void SetFullScreen(bool isFullScreen)
            => Area = isFullScreen ? RenderArea.Full : originalArea;

        public UnityCam() : base(CSkyL.Game.CamController.I.GetCamera())
        {
            originalArea = Area;
            _targetSetting = AllSetting;
        }

        public readonly RenderArea originalArea;
        private Setting _targetSetting;
    }
}
