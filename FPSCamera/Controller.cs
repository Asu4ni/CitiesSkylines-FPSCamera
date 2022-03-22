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
            _cam = null;
            _targetFieldOfView = _originalFieldOfView;
            if (Config.G.SetToOriginalPos) {
                _exitingTimer = Config.G.MaxExitingDuration;
                _targetPositioning = _originalPositioning;
            }
            else {
                _exitingTimer = 0f;
                CamController.LocateAt(_positioning);
            }
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
            _cam = newCam;
            _uiCamInfoPanel.SetAssociatedCam(newCam);
            if (IsIdle) _EnableFPSCam();
        }

        private void _EnableFPSCam()
        {
            _uiMainPanel.OnCamActivate();
            if (Config.G.HideUIwhenActivate) UI.Helper.HideUI();

            _positioning = _originalPositioning = _targetPositioning = _camGame.Positioning;
            _fieldOfView = _originalFieldOfView = _camGame.FoV;
            _targetFieldOfView = Config.G.CamFieldOfView;

            _camGame.NearClipPlane = 1f;
            CamController.SetDepthOfField(Config.G.EnableDof);
            CamController.Disable();
            _state = State.Activated;
        }
        private void _DisableFPSCam()
        {
            Log.Msg("FPS camera stopped");

            _uiMainPanel.OnCamDeactivate();
            _uiCamInfoPanel.enabled = false;
            if (Config.G.HideUIwhenActivate) UI.Helper.ShowUI();

            Control.ShowCursor();
            CamController.Restore();
            _state = State.Idle;
        }


        // TODO: handle Esc
        private Offset _GetInputOffsetAfterHandleInput()
        {
            if (Control.KeyTriggered(Control.Key.CamToggle)) {
                if (IsActivated) StopFPSCam();
                else StartFreeCam();
            }
            if (!IsActivated || !_cam.Validate()) return null;

            if (Control.MouseTriggered(Control.MouseButton.Middle) ||
                Control.KeyTriggered(Control.Key.CamReset)) {
                _cam.InputReset();
                _targetFieldOfView = Config.G.CamFieldOfView;
            }

            if (Control.MouseTriggered(Control.MouseButton.Secondary))
                (_cam as Cam.WalkThruCam)?.SwitchTarget();

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
                                _cam is Cam.FreeCam ? Config.G.ShowCursorWhileFreeCam
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
                if (scroll > 0f && _targetFieldOfView > Config.G.CamFieldOfView.Min)
                    _targetFieldOfView /= Config.G.FoViewScrollfactor;
                else if (scroll < 0f && _targetFieldOfView < Config.G.CamFieldOfView.Max)
                    _targetFieldOfView *= Config.G.FoViewScrollfactor;
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
            var controlOffset = _GetInputOffsetAfterHandleInput();

            if (IsIdle) return;

            if (_state == State.Exiting) {
                _exitingTimer -= Control.DurationFromLastFrame;
                if (_exitingTimer <= 0f || _targetFieldOfView.AlmostEqual(_fieldOfView) &&
                                           _positioning.AlmostEquals(_targetPositioning)) {
                    _DisableFPSCam();
                    return;
                }
            }
            else if (!_cam.Validate()) { StopFPSCam(); return; }
            else {
                _cam.InputOffset(controlOffset);
                (_cam as Cam.ICamUsingTimer)?.ElapseTime(Control.DurationFromLastFrame);
                _targetPositioning = _cam.GetPositioning();
            }

            var distance = _positioning.position.DistanceTo(_targetPositioning.position);
            if (Config.G.SmoothTransition && distance <= Config.G.GiveUpTransitionDistance) {
                var reduceFactor = Config.G.GetReduceFactor(Control.DurationFromLastFrame);

                var position = _cam is Cam.FollowCam && distance <= Config.G.InstantMoveMax ?
                                   _targetPositioning.position :
                                   _positioning.position.GetNextOfSmoothTrans(
                                       _targetPositioning.position, reduceFactor,
                                       new Range(Config.G.DeltaPosMin, Config.G.DeltaPosMax));

                var angle = _positioning.angle.GetNextOfSmoothTrans(
                                _targetPositioning.angle, reduceFactor,
                                new Range(Config.G.DeltaRotateMin, Config.G.DeltaRotateMax));

                _fieldOfView = _fieldOfView.GetNextOfSmoothTrans(
                                   _targetFieldOfView, reduceFactor, new Range(1f, 5f));
                _positioning = new Positioning(position, angle);

            }
            else {
                // TODO: fade out fade in
                _positioning = _targetPositioning; _fieldOfView = _targetFieldOfView;
            }
            _camGame.Positioning = _positioning;
            _camGame.FoV = _fieldOfView;

            _uiCamInfoPanel.enabled = Config.G.DisplayInfoPanel;
        }

        private Offset _FollowCamInputOffsetHandler(Offset inputOffset)
            => new Offset(inputOffset.movement, inputOffset.deltaAttitude.Clamp(
                    new Range(-Config.G.MaxHoriRotate4Follow, Config.G.MaxHoriRotate4Follow),
                    new Range(-Config.G.MaxVertRotate4Follow, Config.G.MaxVertRotate4Follow))
               );

        private Game.Cam _camGame;
        private Cam.Base _cam;

        // UI
        private UI.MainPanel _uiMainPanel;
        private UI.CamInfoPanel _uiCamInfoPanel;
        private UI.FollowButtons _uiFollowButtons;

        // camera properties
        private float _fieldOfView, _targetFieldOfView;
        private Positioning _positioning, _targetPositioning;

        private float _originalFieldOfView;
        private Positioning _originalPositioning;

        // state
        private enum State { Idle, Exiting, Activated }
        private State _state = State.Idle;
        private float _exitingTimer = 0f;
    }
}
