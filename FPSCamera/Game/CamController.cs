namespace FPSCamera.Game
{
    using System;
    using Transform;

    public static class CamController
    {
        public static void Init()
        {
            _controller = UnityEngine.Object.FindObjectOfType<CameraController>();
            _camDoF = GetComponent<UnityStandardAssets.ImageEffects.DepthOfField>();
            _camTiltEffect = GetComponent<TiltShiftEffect>();

            if (_camDoF != null) _oDoFEnabled = _camDoF.enabled;
            if (_camTiltEffect != null) _oTiltEffectEnabled = _camTiltEffect.enabled;
        }

        public static CustomController AddCustomController<CustomController>()
                                        where CustomController : Behavior
            => _controller.gameObject.AddComponent<CustomController>();

        public static UnityEngine.Camera GetCamera()
            => Utils.ReadFields(_controller).Get<UnityEngine.Camera>("m_camera");

        public static void SetDepthOfField(bool enabled)
        { if (_camDoF != null) _camDoF.enabled = enabled; }

        public static void Restore()
        {
            _controller.enabled = true;

            if (_camDoF != null) _camDoF.enabled = _oDoFEnabled;
            if (_camTiltEffect != null) _camTiltEffect.enabled = _oTiltEffectEnabled;
        }

        public static void Disable()
        {
            _controller.enabled = false;
            if (_camTiltEffect != null) _camTiltEffect.enabled = false;
        }
        // TODO: improvement: only works close to ground now.
        public static Positioning LocateAt(Positioning positioning)
        {
            _controller.ClearTarget();

            var position = positioning.position;
            var angle = new Angle(positioning.angle.yawDegree,
                                  positioning.angle.pitchDegree > -20f ?
                                      -20f : positioning.angle.pitchDegree);


            _controller.m_currentAngle = _controller.m_targetAngle = angle.AsGameAngle;
            _controller.m_currentPosition = _controller.m_targetPosition = position.AsGamePosition;
            _controller.m_currentSize = _controller.m_targetSize = position.up * 1.4f;
            _controller.m_currentHeight = _controller.m_targetHeight = position.up;

            return _GetUpdatedPositioning();
        }

        private static Comp GetComponent<Comp>() => _controller.GetComponent<Comp>();

        private static CameraController _controller;

        private static UnityStandardAssets.ImageEffects.DepthOfField _camDoF;
        private static TiltShiftEffect _camTiltEffect;

        private static bool _oDoFEnabled;
        private static bool _oTiltEffectEnabled;

        // simulate how CamController would work
        private static Positioning _GetUpdatedPositioning()
        {
            var targetSize = _controller.m_targetSize
                             .Clamp(_controller.m_minDistance, _controller.m_maxDistance);
            var targetPos = _controller.m_targetPosition;
            var targetAngle = _controller.m_targetAngle;
            var targetH = _controller.m_targetHeight;
            var camera = GetCamera();
            var maxDist = _controller.m_maxDistance;

            var dist = 0f;
            var pos = UnityEngine.Vector3.zero;
            for (int i = 0; i < 3; i++) {
                targetH = TerrainManager.instance
                          .SampleRawHeightSmoothWithWater(targetPos, true, 2f);
                targetPos.y = targetH + targetSize / 20f + 10f;
                dist = (float) (targetSize * Math.Max(0f, 1f - targetH / maxDist)
                       / Math.Tan((float) Math.PI / 180f * camera.fieldOfView));
                var angle = ToolManager.instance.m_properties.m_mode.IsFlagSet(
                                ItemClass.Availability.ThemeEditor) ? targetAngle.y :
                                90f - (90f - targetAngle.y) * (_controller.m_maxTiltDistance / 2f
                                / (_controller.m_maxTiltDistance / 2f + targetSize));
                var rotate = UnityEngine.Quaternion.Euler(angle, targetAngle.x, 0f);
                var newPos = targetPos + rotate * UnityEngine.Vector3.forward * (float) -dist;
                pos = CameraController.ClampCameraPosition(newPos);
                var diff = pos - newPos;
                targetPos += diff;
                if (diff.sqrMagnitude < 0.0001f) break;
            }

            targetPos.y += CameraController.CalculateCameraHeightOffset(pos, dist);
            targetPos = CameraController.ClampCameraPosition(targetPos);

            dist = (float) (targetSize * Math.Max(0f, 1f - targetH / maxDist)
                      / Math.Tan(Math.PI / 180f * camera.fieldOfView));
            var rotation = UnityEngine.Quaternion.Euler(targetAngle.y, targetAngle.x, 0f);
            pos = targetPos + rotation * UnityEngine.Vector3.forward * (float) -dist;
            pos.y += CameraController.CalculateCameraHeightOffset(pos, dist);
            pos = CameraController.ClampCameraPosition(pos);

            return new Positioning(Position.FromGame(pos), Angle.FromGame(rotation));
        }

    }
}
