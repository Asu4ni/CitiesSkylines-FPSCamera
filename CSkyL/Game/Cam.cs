namespace CSkyL.Game
{
    using CSkyL.Transform;
    using UnityEngine;
    using Range = CSkyL.Math.Range;

    public class Cam
    {
        public virtual Positioning Positioning {
            get => new Positioning(Position._FromVec(_camera.transform.position),
                                   Angle._FromQuat(_camera.transform.rotation));
            set {
                _camera.transform.position = value.position._AsVec;
                _camera.transform.rotation = value.angle._AsQuat;
            }
        }
        public virtual RenderArea Area {
            get => RenderArea._FromRect(_camera.rect);
            set => _camera.rect = value._AsRect;
        }
        public virtual float FieldOfView {
            get => _camera.fieldOfView;
            set => _camera.fieldOfView = value;
        }
        public virtual float NearClipPlane {
            get => _camera.nearClipPlane;
            set => _camera.nearClipPlane = value;
        }
        public virtual Setting AllSetting {
            get => new Setting(Positioning, Area, FieldOfView, NearClipPlane);
            set {
                Positioning = value.positioning; Area = value.area;
                FieldOfView = value.fieldOfView; NearClipPlane = value.nearClipPlane;
            }
        }

        public bool IsFullScreen => Area.AlmostEquals(RenderArea.Full);

        public Cam(Camera camGame) { _camera = camGame; }

        protected readonly Camera _camera;

        // value range [0, 1]
        // left to right, bottom to top
        public class RenderArea
        {
            public float left, right;
            public float bottom, top;

            public static readonly RenderArea Full = new RenderArea(0f, 1f, 0f, 1f);

            public RenderArea(float left, float right, float bottom, float top)
            {
                this.left = left; this.right = right; this.bottom = bottom; this.top = top;
                if (left > right) { left = 0f; right = 1f; }
                if (bottom > top) { bottom = 0f; top = 1f; }
            }

            public bool AlmostEquals(RenderArea target)
                => left.AlmostEquals(target.left, _error) &&
                    right.AlmostEquals(target.right, _error) &&
                    bottom.AlmostEquals(target.bottom, _error) &&
                    top.AlmostEquals(target.top, _error);

            private static readonly Range _range = new Range(1f / 256, 1f / 32);
            public RenderArea AdvanceToTarget(RenderArea target, float advanceRatio)
                => new RenderArea(left.AdvanceToTarget(target.left, advanceRatio, _range),
                                  right.AdvanceToTarget(target.right, advanceRatio, _range),
                                  bottom.AdvanceToTarget(target.bottom, advanceRatio, _range),
                                  top.AdvanceToTarget(target.top, advanceRatio, _range));

            internal Rect _AsRect => new Rect(left, bottom, right - left, top - bottom);
            internal static RenderArea _FromRect(Rect r)
                => new RenderArea(r.x, r.x + r.width, r.y, r.y + r.height);

            private const float _error = 1f / 1024;
        }

        public struct Setting
        {
            public Positioning positioning;
            // range: [0f, 1f]
            // x, width: left to right / y, height: bottom to top
            public RenderArea area;
            public float fieldOfView, nearClipPlane;

            public bool AlmostEquals(Setting other)
                => positioning.AlmostEquals(other.positioning) &&
                   area.AlmostEquals(other.area) &&
                   fieldOfView.AlmostEquals(other.fieldOfView) &&
                   nearClipPlane.AlmostEquals(other.nearClipPlane);

            public Setting(Positioning positioning, RenderArea renderArea,
                              float fieldOfView, float nearClipPlane)
            {
                this.positioning = positioning; this.area = renderArea;
                this.fieldOfView = fieldOfView; this.nearClipPlane = nearClipPlane;
            }
        }
    }
}
