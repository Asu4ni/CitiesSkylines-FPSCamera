namespace FPSCamera.Game
{
    using Transform;
    using UnityEngine;

    public class Cam
    {
        public Cam(Camera camera) { _camera = camera; }

        public Positioning Positioning {
            get => new Positioning(Position.FromGame(_camera.transform.position),
                                   Angle.FromGame(_camera.transform.rotation));
            set {
                _camera.transform.position = value.position.AsGamePosition;
                _camera.transform.rotation = value.angle.AsGameRotation;
            }
        }

        public float FoV {
            get => _camera.fieldOfView;
            set => _camera.fieldOfView = value;
        }

        public float NearClipPlane {
            get => _camera.nearClipPlane;
            set => _camera.nearClipPlane = value;
        }

        private readonly Camera _camera;
    }
}
