namespace FPSCamera
{
    using Configuration;
    using CSkyL.Transform;
    using CamController = CSkyL.Game.CamController;
    using Control = CSkyL.Game.Control;
    using CUtils = CSkyL.Game.Utils;
    using Log = CSkyL.Log;

    public class Controller : CSkyL.Game.Behavior
    {
        public bool IsActivated => _state == State.Activated;
        public bool IsIdle => _state == State.Idle;

        public void StartFreeCam()
        {
            Log.Msg("Starting FreeCam mode");
            _SetCam(new Cam.FreeCam(_camUnity.Positioning));
        }

        public void StartFollow(CSkyL.Game.ID.ObjectID idToFollow)
        {
            Log.Msg("Starting Follow mode");
            var newCam = Cam.FollowCam.Follow(idToFollow);
            if (newCam is Cam.FollowCam cam) _SetCam(newCam);
            else Log.Msg($"Fail to start Follow mode (ID: {idToFollow})");
        }
        public void StartWalkThruMode()
        {
            Log.Msg("Starting WalkThru mode");
            _SetCam(new Cam.WalkThruCam());
        }

        public void StopFPSCam()
        {
            if (!IsActivated) return;
            _camMod = null;

            _exitingTimer = Config.G.MaxExitingDuration;
            _camUnity.AllSetting = _originalSetting;
            if (!Config.G.SetBackCamera)
                _camUnity.Positioning = CamController.I.LocateAt(_camUnity.Positioning);

            _camUnity.SetFullScreen(false);
            _uiHidden = false;
            _uiCamInfoPanel.enabled = false;

            _state = State.Exiting;
        }

        public bool OnEsc()
        {
            if (_uiMainPanel.OnEsc()) return true;
            if (_camMod is object) {
                StopFPSCam();
                return true;
            }
            return false;
        }

        private void _SetCam(Cam.Base newCam)
        {
            _camMod = newCam;
            _uiCamInfoPanel.SetAssociatedCam(newCam);
            if (IsIdle) _EnableFPSCam();
            else _uiMainPanel.OnCamActivate();
        }

        private void _EnableFPSCam()
        {
            _originalSetting = _camUnity.AllSetting;
            _ResetCamGame();

            CamController.I.SetDepthOfField(Config.G.EnableDof);
            CamController.I.Disable();

            _uiMainPanel.OnCamActivate();

            _state = State.Activated;
        }
        private void _DisableFPSCam()
        {
            Log.Msg("FPS camera stopped");

            _uiMainPanel.OnCamDeactivate();
            Control.ShowUI();

            Control.ShowCursor();
            CamController.I.Restore();
            _state = State.Idle;
        }
        private void _ResetCamGame()
        {
            _camUnity.ResetTarget();
            _camUnity.FieldOfView = Config.G.CamFieldOfView;
            _camUnity.NearClipPlane = Config.G.CamNearClipPlane;
        }

        private Offset _GetInputOffsetAfterHandleInput()
        {
            if (Control.KeyTriggered(Config.G.KeyCamToggle)) {
                if (IsActivated) StopFPSCam();
                else StartFreeCam();
            }
            if (!IsActivated || !_camMod.Validate()) return null;

            if (Control.MouseTriggered(Control.MouseButton.Middle) ||
                Control.KeyTriggered(Config.G.KeyCamReset)) {
                _camMod.InputReset();
                _ResetCamGame();
            }

            if (Control.MouseTriggered(Control.MouseButton.Secondary))
                (_camMod as Cam.WalkThruCam)?.SwitchTarget();
            if (Control.KeyTriggered(Config.G.KeyAutoMove))
                (_camMod as Cam.FreeCam)?.ToggleAutoMove();
            if (Control.KeyTriggered(Config.G.KeySaveOffset) &&
                    _camMod is Cam.FollowCam followCam) {
                if (followCam.SaveOffset() is string name)
                    _uiMainPanel.ShowMessage($"Offset saved for <{name}>");
            }


            var movement = LocalMovement.None;
            { // key movement
                if (Control.KeyPressed(Config.G.KeyMoveForward)) movement.forward += 1f;
                if (Control.KeyPressed(Config.G.KeyMoveBackward)) movement.forward -= 1f;
                if (Control.KeyPressed(Config.G.KeyMoveRight)) movement.right += 1f;
                if (Control.KeyPressed(Config.G.KeyMoveLeft)) movement.right -= 1f;
                if (Control.KeyPressed(Config.G.KeyMoveUp)) movement.up += 1f;
                if (Control.KeyPressed(Config.G.KeyMoveDown)) movement.up -= 1f;
                movement *= (Control.KeyPressed(Config.G.KeySpeedUp) ? Config.G.SpeedUpFactor : 1f)
                            * Config.G.MovementSpeed * CUtils.TimeSinceLastFrame
                            / CSkyL.Game.Map.ToKilometer(1f);
            }

            var cursorVisible = Control.KeyPressed(Config.G.KeyCursorToggle) ^ (
                                _camMod is Cam.FreeCam ? Config.G.ShowCursor4Free
                                                    : Config.G.ShowCursor4Follow);
            Control.ShowCursor(cursorVisible);

            float yawDegree = 0f, pitchDegree = 0f;
            { // key rotation
                if (Control.KeyPressed(Config.G.KeyRotateRight)) yawDegree += 1f;
                if (Control.KeyPressed(Config.G.KeyRotateLeft)) yawDegree -= 1f;
                if (Control.KeyPressed(Config.G.KeyRotateUp)) pitchDegree += 1f;
                if (Control.KeyPressed(Config.G.KeyRotateDown)) pitchDegree -= 1f;

                if (yawDegree != 0f || pitchDegree != 0f) {
                    var factor = Config.G.RotateKeyFactor * CUtils.TimeSinceLastFrame;
                    yawDegree *= factor; pitchDegree *= factor;
                }
                else if (!cursorVisible) {
                    // mouse rotation
                    const float factor = .2f;
                    yawDegree = Control.MouseMoveHori * Config.G.RotateSensitivity *
                                (Config.G.InvertRotateHorizontal ? -1f : 1f) * factor;
                    pitchDegree = Control.MouseMoveVert * Config.G.RotateSensitivity *
                                  (Config.G.InvertRotateVertical ? -1f : 1f) * factor;
                }
            }
            { // scroll zooming
                var scroll = Control.MouseScroll;
                var targetFoV = _camUnity.TargetFoV;
                if (scroll > 0f && targetFoV > Config.G.CamFieldOfView.Min)
                    _camUnity.FieldOfView = targetFoV / Config.G.FoViewScrollfactor;
                else if (scroll < 0f && targetFoV < Config.G.CamFieldOfView.Max)
                    _camUnity.FieldOfView = targetFoV * Config.G.FoViewScrollfactor;
            }
            return new Offset(movement, new DeltaAttitude(yawDegree, pitchDegree));
        }

        private void _SetUpUI()
        {
            _uiMainPanel = gameObject.AddComponent<UI.MainPanel>();
            _uiMainPanel.SetWalkThruCallBack(StartWalkThruMode);
            _uiCamInfoPanel = gameObject.AddComponent<UI.CamInfoPanel>();
            if (CUtils.InGameMode) {
                _uiFollowButtons = gameObject.AddComponent<UI.FollowButtons>();
                _uiFollowButtons.registerFollowCallBack(StartFollow);
            }
        }

        protected override void _Init()
        {
            _state = State.Idle;
            _uiHidden = false;
        }
        protected override void _SetUp()
        {
            _camUnity = new UnityCam();
            _SetUpUI();
        }
        protected override void _UpdateLate()
        {
            try {
                var controlOffset = _GetInputOffsetAfterHandleInput();

                if (IsIdle) return;

                if (_state == State.Exiting) {
                    _exitingTimer -= CUtils.TimeSinceLastFrame;
                    if (_camUnity.AlmostAtTarget() is bool done && done || _exitingTimer <= 0f) {
                        if (!done) _camUnity.AdvanceToTarget();
                        _DisableFPSCam();
                        return;
                    }
                }
                else if (!_camMod.Validate()) { StopFPSCam(); return; }
                else {
                    _camMod.InputOffset(controlOffset);
                    (_camMod as Cam.ICamUsingTimer)?.ElapseTime(CUtils.TimeSinceLastFrame);
                    _camUnity.Positioning = _camMod.GetPositioning();
                }

                var distance = _camUnity.Positioning.position
                                   .DistanceTo(_camUnity.TargetPositioning.position);
                var factor = Config.G.GetAdvanceRatio(CUtils.TimeSinceLastFrame);
                if (Config.G.SmoothTransition) {
                    if (distance > Config.G.GiveUpTransDistance)
                        _camUnity.AdvanceToTargetSmooth(factor,
                                                        instantMove: true, instantAngle: true);
                    else if (_camMod is Cam.FollowCam && distance <= Config.G.InstantMoveMax)
                        _camUnity.AdvanceToTargetSmooth(factor, instantMove: true);
                    else _camUnity.AdvanceToTargetSmooth(factor);
                }
                else {
                    _camUnity.AdvanceToTarget(factor, smoothArea: true);
                }

                if (IsActivated) {
                    _uiCamInfoPanel.enabled = Config.G.ShowInfoPanel;
                    if (Config.G.HideGameUI ^ _uiHidden) {
                        _uiHidden = Config.G.HideGameUI;
                        Control.ShowUI(!_uiHidden);
                        _camUnity.SetFullScreen(_uiHidden);
                    }
                }
            }
            catch (System.Exception e) {
                Log.Err("Unrecognized Error: " + e.ToString());
            }
        }

        // Cameras
        private Cam.Base _camMod;
        private UnityCam _camUnity;
        private UnityCam.Setting _originalSetting;

        // UI
        private UI.MainPanel _uiMainPanel;
        private UI.CamInfoPanel _uiCamInfoPanel;
        private UI.FollowButtons _uiFollowButtons;
        private bool _uiHidden;

        // state
        private enum State { Idle, Exiting, Activated }
        private State _state = State.Idle;
        private float _exitingTimer = 0f;
    }
}
