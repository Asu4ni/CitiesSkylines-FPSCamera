namespace FPSCamera.Game
{
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

        public static UnityEngine.Camera GetCamera() => GetComponent<UnityEngine.Camera>();

        public static void SetDepthOfField(bool enabled) => _camDoF.enabled = enabled;

        public static void Enable()
        {
            _controller.enabled = true;

            if (_camDoF != null) _camDoF.enabled = _oDoFEnabled;
            if (_camTiltEffect != null) _camTiltEffect.enabled = _oTiltEffectEnabled;
        }

        public static void Disable()
        {
            _controller.enabled = false;
            _controller.ClearTarget();
            _camTiltEffect.enabled = false;
        }

        public static void LocateAt(Transform.Positioning positioning)
        {
            _controller.ClearTarget();

            var angle = new Transform.Angle(positioning.angle.yawDegree,
                positioning.angle.pitchDegree > -20f ? -20f : positioning.angle.pitchDegree);
            _controller.m_currentAngle = _controller.m_targetAngle = angle.AsGameAngle;
            _controller.m_currentPosition = _controller.m_targetPosition
                                                             = positioning.position.AsGamePosition;
            _controller.m_currentSize = _controller.m_targetSize = 100f;
            _controller.m_currentHeight = _controller.m_targetHeight = positioning.position.up;
        }

        private static Comp GetComponent<Comp>() => _controller.GetComponent<Comp>();

        private static CameraController _controller;

        private static UnityStandardAssets.ImageEffects.DepthOfField _camDoF;
        private static TiltShiftEffect _camTiltEffect;

        private static bool _oDoFEnabled;
        private static bool _oTiltEffectEnabled;
    }
}
