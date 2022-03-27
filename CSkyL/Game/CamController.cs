namespace CSkyL.Game
{
    using System;
    using Transform;

    public class CamController
    {
        public static CamController I {
            get {
                if (_instance is null || _instance._controller == null) {
                    _instance = new CamController();
                    if (_instance._controller is null) return _instance = null;
                }
                return _instance;
            }
        }
        private static CamController _instance = null;

        public TComp AddComponent<TComp>() where TComp : Behavior
            => _controller.gameObject.AddComponent<TComp>();
        public TComp GetComponent<TComp>() where TComp : UnityEngine.MonoBehaviour
            => _controller.gameObject.GetComponent<TComp>();

        public UnityEngine.Camera GetCamera()
            => Lang.ReadFields(_controller).Get<UnityEngine.Camera>("m_camera");

        public void SetDepthOfField(bool enabled)
        { if (_camDoF != null) _camDoF.enabled = enabled; }

        public void Restore()
        {
            _controller.enabled = true;

            if (_camDoF != null) _camDoF.enabled = _oDoFEnabled;
            if (_camTiltEffect != null) _camTiltEffect.enabled = _oTiltEffectEnabled;
        }

        public void Disable()
        {
            _controller.enabled = false;
            if (_camTiltEffect != null) _camTiltEffect.enabled = false;
        }

        // TODO: improvement: only works close to ground now.
        public Positioning LocateAt(Positioning positioning)
        {
            _controller.ClearTarget();

            var position = positioning.position;
            var angle = new Angle(positioning.angle.yawDegree,
                                  positioning.angle.pitchDegree.Clamp(-90f, -20f));


            _controller.m_currentAngle = _controller.m_targetAngle = angle._AsVec2;
            _controller.m_currentPosition = _controller.m_targetPosition = position._AsVec;
            _controller.m_currentSize = _controller.m_targetSize = (float) (
                (position.up - Map.GetMinHeightAt(position)).Clamp(10f, 1000f)
                    / Math.Sin(-angle.pitchDegree / 180f * Math.PI));
            _controller.m_currentHeight = _controller.m_targetHeight = position.up;

            return _GetUpdatedPositioning();
        }

        private CamController()
        {
            _controller = ToolsModifierControl.cameraController;
            if (_controller is null) return;
            _camDoF = GetComponent<UnityStandardAssets.ImageEffects.DepthOfField>();
            _camTiltEffect = GetComponent<TiltShiftEffect>();

            if (_camDoF != null) _oDoFEnabled = _camDoF.enabled;
            if (_camTiltEffect != null) _oTiltEffectEnabled = _camTiltEffect.enabled;
        }

        private CameraController _controller;

        private UnityStandardAssets.ImageEffects.DepthOfField _camDoF;
        private TiltShiftEffect _camTiltEffect;

        private bool _oDoFEnabled;
        private bool _oTiltEffectEnabled;

        // simulate how CamController would work
        private Positioning _GetUpdatedPositioning()
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

            return new Positioning(Position._FromVec(pos), Angle._FromQuat(rotation));
        }

    }
}
