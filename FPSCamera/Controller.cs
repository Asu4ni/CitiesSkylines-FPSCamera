namespace FPSCamera
{
    using Transform;
    using Wrapper;
    using static Utils;
    using CamController = Game.CamController;
    using Control = Game.Control;

    public class Controller : Game.Behavior
    {
        public bool IsActivated => _state == State.Activated;
        public bool IsIdle => _state == State.Idle;

        public void StartFreeCam()
        {
            _SetCam(new Cam.FreeCam(_camGame.Positioning));
            Log.Msg("starting FreeCam mode");
        }
        public void StartFollow(ID idToFollow)
        {
            var newCam = Cam.FollowCam.Follow(idToFollow, _FollowCamInputOffsetHandler);
            if (newCam is Cam.FollowCam cam) {
                _SetCam(newCam);
                Log.Msg("starting Follow mode");
            }
            else Log.Msg($"fail to start Follow mode (ID: {idToFollow})");
        }
        public void StartWalkThruMode()
        {
            _SetCam(new Cam.WalkThruCam(_FollowCamInputOffsetHandler));
            Log.Msg("starting WalkThru mode");
        }

        public void StopFPSCam()
        {
            if (!IsActivated) return;
            _camMod = null;

            _exitingTimer = Config.G.MaxExitingDuration;
            _camGame.Setting = _originalSetting;
            if (!Config.G.SetToOriginalPos)
                _camGame.Positioning = CamController.LocateAt(_camGame.Positioning);

            _camGame.SetFullScreen(false);
            _uiCamInfoPanel.enabled = false;

            _state = State.Exiting;
        }

        public void ResetUI()
        {
            _DestroyUI();
            _uiMainPanel = gameObject.AddComponent<UI.MainPanel>();
            _uiMainPanel.SetWalkThruCallBack(StartWalkThruMode);
            _uiCamInfoPanel = gameObject.AddComponent<UI.CamInfoPanel>();
            if (Mod.IsInGameMode) {
                _uiFollowButtons = gameObject.AddComponent<UI.FollowButtons>();
                _uiFollowButtons.registerFollowCallBack(StartFollow);
            }
        }

        private void _SetCam(Cam.Base newCam)
        {
            _camMod = newCam;
            _uiCamInfoPanel.SetAssociatedCam(newCam);
            if (IsIdle) _EnableFPSCam();
        }

        private void _EnableFPSCam()
        {
            _originalSetting = _camGame.Setting;
            _ResetCamGame();

            CamController.SetDepthOfField(Config.G.EnableDof);
            CamController.Disable();

            _uiMainPanel.OnCamActivate();

            _state = State.Activated;
        }
        private void _DisableFPSCam()
        {
            Log.Msg("FPS camera stopped");

            _uiMainPanel.OnCamDeactivate();
            Control.ShowUI();

            Control.ShowCursor();
            CamController.Restore();
            _state = State.Idle;
        }
        private void _ResetCamGame()
        {
            _camGame.ResetTarget();
            _camGame.FieldOfView = Config.G.CamFieldOfView;
            _camGame.NearClipPlane = Config.G.CamNearClipPlane;
        }

        // TODO: handle Esc
        private Offset _GetInputOffsetAfterHandleInput()
        {
            if (Control.KeyTriggered(Control.Key.CamToggle)) {
                if (IsActivated) StopFPSCam();
                else StartFreeCam();
            }
            if (!IsActivated || !_camMod.Validate()) return null;

            if (Control.MouseTriggered(Control.MouseButton.Middle) ||
                Control.KeyTriggered(Control.Key.CamReset)) {
                _camMod.InputReset();
                _ResetCamGame();
            }

            if (Control.MouseTriggered(Control.MouseButton.Secondary))
                (_camMod as Cam.WalkThruCam)?.SwitchTarget();

            var movement = LocalMovement.None;
            { // key movement
                if (Control.KeyPressed(Control.Key.Forward)) movement.forward += 1f;
                if (Control.KeyPressed(Control.Key.Backward)) movement.forward -= 1f;
                if (Control.KeyPressed(Control.Key.Right)) movement.right += 1f;
                if (Control.KeyPressed(Control.Key.Left)) movement.right -= 1f;
                if (Control.KeyPressed(Control.Key.Up)) movement.up += 1f;
                if (Control.KeyPressed(Control.Key.Down)) movement.up -= 1f;
                movement *= (Control.KeyPressed(Control.Key.Faster) ? Config.G.SpeedUpFactor : 1f)
                            * Config.G.MovementSpeed * Control.DurationFromLastFrame
                            / Game.Map.ToKilometer(1f);
            }

            var cursorVisible = Control.KeyPressed(Control.Key.CursorToggle) ^ (
                                _camMod is Cam.FreeCam ? Config.G.ShowCursorWhileFreeCam
                                                    : Config.G.ShowCursorWhileFollow);
            Control.ShowCursor(cursorVisible);

            float yawDegree = 0f, pitchDegree = 0f;
            { // key rotation
                if (Control.KeyPressed(Control.Key.RotateR)) yawDegree += 1f;
                if (Control.KeyPressed(Control.Key.RotateL)) yawDegree -= 1f;
                if (Control.KeyPressed(Control.Key.RotateU)) pitchDegree += 1f;
                if (Control.KeyPressed(Control.Key.RotateD)) pitchDegree -= 1f;

                if (yawDegree != 0f || pitchDegree != 0f) {
                    var factor = Config.G.KeyRotateFactor * Control.DurationFromLastFrame;
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
                var targetFoV = _camGame.TargetFoV;
                if (scroll > 0f && targetFoV > Config.G.CamFieldOfView.Min)
                    _camGame.FieldOfView = targetFoV / Config.G.FoViewScrollfactor;
                else if (scroll < 0f && targetFoV < Config.G.CamFieldOfView.Max)
                    _camGame.FieldOfView = targetFoV * Config.G.FoViewScrollfactor;
            }
            return new Offset(movement, new DeltaAttitude(yawDegree, pitchDegree));
        }

        private void _DestroyUI()
        {
            if (_uiMainPanel != null) Destroy(_uiMainPanel);
            if (_uiCamInfoPanel != null) Destroy(_uiCamInfoPanel);
            if (_uiFollowButtons != null) Destroy(_uiFollowButtons);
        }

        protected override void _Init()
        {
            _state = State.Idle;
            ResetUI();
        }
        protected override void _SetUp()
        {
            _camGame = new Game.Cam(CamController.GetCamera());
        }
        protected override void _UpdateLate()
        {
            try {
                var controlOffset = _GetInputOffsetAfterHandleInput();

                if (IsIdle) return;

                if (_state == State.Exiting) {
                    _exitingTimer -= Control.DurationFromLastFrame;
                    if (_camGame.AlmostAtTarget() is bool done && done || _exitingTimer <= 0f) {
                        if (!done) _camGame.AdvanceToTarget();
                        _DisableFPSCam();
                        return;
                    }
                }
                else if (!_camMod.Validate()) { StopFPSCam(); return; }
                else {
                    _camMod.InputOffset(controlOffset);
                    (_camMod as Cam.ICamUsingTimer)?.ElapseTime(Control.DurationFromLastFrame);
                    _camGame.Positioning = _camMod.GetPositioning();
                }

                // TODO: fade out fade in for any instant move
                var distance = _camGame.Positioning.position
                                   .DistanceTo(_camGame.TargetPositioning.position);
                var factor = Config.G.GetAdvanceFactor(Control.DurationFromLastFrame);
                if (Config.G.SmoothTransition) {
                    if (distance > Config.G.GiveUpTransitionDistance ||
                        _camMod is Cam.FollowCam && distance <= Config.G.InstantMoveMax)
                        _camGame.AdvanceToTargetSmooth(factor, instantMove: true);
                    else _camGame.AdvanceToTargetSmooth(factor);
                }
                else {
                    _camGame.AdvanceToTarget(factor, smoothRRect: true);
                }
                if (IsActivated) {
                    _uiCamInfoPanel.enabled = Config.G.DisplayInfoPanel;
                    if (Config.G.HideUIwhenActivate ^ _camGame.IsFullScreen) {
                        Control.ShowUI(!Config.G.HideUIwhenActivate);
                        _camGame.SetFullScreen(Config.G.HideUIwhenActivate);
                    }
                }
            }
            catch (System.Exception e) {
                Log.Err("Unrecognized Error: " + e.ToString());
            }
        }

        private Offset _FollowCamInputOffsetHandler(Offset inputOffset)
            => new Offset(inputOffset.movement, inputOffset.deltaAttitude.Clamp(
                    new Range(-Config.G.MaxHoriRotate4Follow, Config.G.MaxHoriRotate4Follow),
                    new Range(-Config.G.MaxVertRotate4Follow, Config.G.MaxVertRotate4Follow))
               );

        // Cameras
        private Cam.Base _camMod;
        private Game.Cam _camGame;
        private Game.CamSetting _originalSetting;

        // UI
        private UI.MainPanel _uiMainPanel;
        private UI.CamInfoPanel _uiCamInfoPanel;
        private UI.FollowButtons _uiFollowButtons;

        // state
        private enum State { Idle, Exiting, Activated }
        private State _state = State.Idle;
        private float _exitingTimer = 0f;
    }
}
