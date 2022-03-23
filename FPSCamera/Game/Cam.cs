namespace FPSCamera.Game
{
    using Transform;
    using UnityEngine;
    using Range = FPSCamera.Utils.Range;

    public class Cam
    {
        public Cam(Camera camera) { _camera = camera; _targetSetting = Setting; }

        // Getter => current setting / Setter: set target setting
        public Positioning Positioning {
            get => new Positioning(Position.FromGame(_camera.transform.position),
                                   Angle.FromGame(_camera.transform.rotation));
            set => _targetSetting.positioning = value;
        }
        public Rect RenderRect {
            get => _camera.rect;
            set => _targetSetting.renderRect = value;
        }
        public float FieldOfView {
            get => _camera.fieldOfView;
            set => _targetSetting.fieldOfView = value;
        }
        public float NearClipPlane {
            get => _camera.nearClipPlane;
            set => _targetSetting.nearClipPlane = value;
        }
        public CamSetting Setting {
            get => new CamSetting(Positioning, RenderRect, FieldOfView, NearClipPlane);
            set => _targetSetting = value;
        }
        public Positioning TargetPositioning => _targetSetting.positioning;
        public float TargetFoV => _targetSetting.fieldOfView;

        public void ResetTarget() => _targetSetting = Setting;
        public bool AlmostAtTarget() => Setting.AlmostEquals(_targetSetting);

        public void AdvanceToTargetSmooth(float advanceFactor = 1f, bool instantMove = false,
                                          bool instantAngle = false, bool instantRRect = false,
                                          bool instantFoV = false, bool instantNCPlane = false)
            => AdvanceToTarget(advanceFactor, !instantMove, !instantAngle,
                               !instantRRect, !instantFoV, !instantNCPlane);
        public void AdvanceToTarget(float advanceFactor = 1f, bool smoothMove = false,
                                          bool smoothAngle = false, bool smoothRRect = false,
                                          bool smoothFoV = false, bool smoothNCPlane = false)
        {
            var current = Setting;

            _camera.transform.position = (!smoothMove ? _targetSetting.positioning.position :
                    current.positioning.position.GetNextOfSmoothTrans(
                            _targetSetting.positioning.position, advanceFactor,
                            new Range(Config.G.DeltaPosMin, Config.G.DeltaPosMax))
                ).AsGamePosition;

            _camera.transform.rotation = (!smoothAngle ? _targetSetting.positioning.angle :
                    current.positioning.angle.GetNextOfSmoothTrans(
                        _targetSetting.positioning.angle, advanceFactor,
                        new Range(Config.G.DeltaRotateMin, Config.G.DeltaRotateMax))
                ).AsGameRotation;

            _camera.rect = !smoothRRect ? _targetSetting.renderRect :
                current.renderRect.GetNextOfSmoothTrans(_targetSetting.renderRect, advanceFactor);

            _camera.fieldOfView = !smoothFoV ? _targetSetting.fieldOfView :
                current.fieldOfView.GetNextOfSmoothTrans(
                    _targetSetting.fieldOfView, advanceFactor, new Range(.25f, 5f));

            _camera.nearClipPlane = !smoothNCPlane ? _targetSetting.nearClipPlane :
                current.nearClipPlane.GetNextOfSmoothTrans(
                    _targetSetting.nearClipPlane, advanceFactor, new Range(.25f, 5f));
        }

        public void SetFullScreen(bool isFullScreen)
            => RenderRect = isFullScreen ? FullRect : RectWithoutMenuBar;

        public bool IsFullScreen => RenderRect.AlmostEquals(FullRect);

        public const float menubarHeightRatio = 0.105f;
        public static Rect FullRect => new Rect(0f, 0f, 1f, 1f);
        public static Rect RectWithoutMenuBar
            => new Rect(0f, FullRect.y + menubarHeightRatio,
                        1f, FullRect.height - menubarHeightRatio);

        private readonly Camera _camera;
        private CamSetting _targetSetting;
    }

    public struct CamSetting
    {
        public Positioning positioning;
        // range: [0f, 1f]
        // x, width: left to right / y, height: bottom to top
        public Rect renderRect;
        public float fieldOfView, nearClipPlane;

        public bool AlmostEquals(CamSetting other)
            => positioning.AlmostEquals(other.positioning) &&
               renderRect.AlmostEquals(other.renderRect) &&
               fieldOfView.AlmostEquals(other.fieldOfView) &&
               nearClipPlane.AlmostEquals(other.nearClipPlane);

        public CamSetting(Positioning positioning, Rect renderRect,
                          float fieldOfView, float nearClipPlane)
        {
            this.positioning = positioning; this.renderRect = renderRect;
            this.fieldOfView = fieldOfView; this.nearClipPlane = nearClipPlane;
        }
    }

    public static class Extension
    {
        private const float _rectError = 1f / 1024;
        public static bool AlmostEquals(this Rect current, Rect target)
            => current.x.AlmostEquals(target.x, _rectError) &&
               current.y.AlmostEquals(target.y, _rectError) &&
               current.width.AlmostEquals(target.width, _rectError) &&
               current.height.AlmostEquals(target.height, _rectError);

        private static readonly Range _range = new Range(1f / 256, 1f / 32);
        public static Rect GetNextOfSmoothTrans(this Rect current, Rect target,
                                                float advanceFactor)
            => new Rect(current.x.GetNextOfSmoothTrans(target.x, advanceFactor, _range),
                        current.y.GetNextOfSmoothTrans(target.y, advanceFactor, _range),
                        current.width.GetNextOfSmoothTrans(target.width, advanceFactor, _range),
                        current.height.GetNextOfSmoothTrans(target.height, advanceFactor, _range));
    }
}
