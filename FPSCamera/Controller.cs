namespace FPSCamera
{
    using Transform;
    using Wrapper;
    using static Utils;
    using CamController = Game.CamController;
    using Control = Game.Control;

    public class Controller : Game.Behavior
    {
        public void StartFreeCam()
        {
            Log.Msg("starting FreeCam mode");
            SwitchState(State.FreeCam);
        }
        public void StartFollow(ID idToFollow)
        {
            Log.Msg("starting Follow mode");
            _idToFollow = idToFollow;
            SwitchState(State.Follow);
        }
        public void StartWalkThruMode()
        {
            Log.Msg("starting WalkThru mode");
            SwitchState(State.WalkThru);
        }

        private bool isIdle => _state == State.Idle;
        private bool isFreeCam => _state == State.FreeCam;
        private bool isFollowing => _state == State.Follow || _state == State.WalkThru;
        private bool isCamOn => _state == State.FreeCam || isFollowing;

        private void EnableFPSCam()
        {
            Log.Msg("start FPS camera");

            _uiMainPanel.OnCamActivate();
            _uiFollowButtons?.OnCamActivate();
            if (Config.G.HideUIwhenActivate) UI.Helper.HideUI();

            _positioning = _originalPositioning = _camGame.Positioning;
            _fieldOfView = _originalFieldOfView = _camGame.FoV;
            ResetCamLocal();

            _camGame.NearClipPlane = 1f;
            CamController.SetDepthOfField(Config.G.EnableDof);
            CamController.Disable();
        }
        private void DisableFPSCam()
        {
            Log.Msg("stop FPS camera");

            _uiMainPanel.OnCamDeactivate();
            _uiFollowButtons?.OnCamDeactivate();
            if (Config.G.HideUIwhenActivate) UI.Helper.ShowUI();

            Control.ShowCursor();
            CamController.Enable();
        }

        /*  state transitions:
         *  
         *  Fr\To   |   idle   |  freeCam  |   follow  |  walkThru | exitFree (eF)   
         *  idle    |     -    |   E > P   |   E > P   |   E > P   |      -       
         *  freeCam |   ->eF   |     -     |     -     |     -     | (redirect) P 
         *  follow  | S > ->eF |     -     |     -     |     -     | (redirect) P
         *  walkThru| S > ->eF |     -     |     -     |     -     | (redirect) P
         *  exiting |     D    |     P     |     -     |     -     |      -       
         *  
         *   * -: impossible | E: EnableFPSCam | D: DisableFPSCam | S: StopFollow | P: Prepare***
         */
        private void SwitchState(State newState)
        {
            Log.Assert((newState == State.Idle != isIdle) && newState != State.Exiting
                        || (_state == State.Exiting && newState == State.FreeCam),
                       $"FPSController state invalid transition: [{_state}] > [{newState}]");

            switch (_state) {
            case State.Idle: EnableFPSCam(); break;
            case State.FreeCam:
                if (newState == State.Idle) newState = State.Exiting;
                break;
            case State.Follow:
            case State.WalkThru:
                if (newState == State.Idle) newState = State.Exiting;
                StopFollowing(); break;
            case State.Exiting: break;
            }
            switch (newState) {
            case State.Idle: DisableFPSCam(); break;
            case State.FreeCam: PrepareFreeCam(); break;
            case State.Follow: PrepareFollowing(); break;
            case State.WalkThru: PrepareWalkThru(); break;
            case State.Exiting: PrepareExiting(); break;
            }
            _state = newState;
        }

        private void PrepareFreeCam() { }
        private void PrepareFollowing()
        {
            switch (_idToFollow) {
            case HumanID humanID:
                if (Object.Of(humanID) is Pedestrian p)
                    _camToFollow = new Cam.PedestrianCam(p.pedestrianID);
                else return; break;
            case PedestrianID pedID:
                _camToFollow = new Cam.PedestrianCam(pedID); break;
            case VehicleID vehicleID:
                _camToFollow = new Cam.VehicleCam(vehicleID); break;
            default:
                Log.Warn($"Following UUID:{_idToFollow} is not supported");
                _camToFollow = null;
                return;
            }
            _uiCamInfoPanel.SetAssociatedCam(_camToFollow);
            ResetCamLocal();
        }
        private void PrepareWalkThru() { SwitchTarget4WalkThru(); }
        private void PrepareExiting()
        {
            _targetFieldOfView = _originalFieldOfView;
            if (Config.G.SetToOriginalPos) {
                _exitingTimer = Config.G.MaxExitingDuration;
                _targetPositioning = _originalPositioning;
            }
            else {
                _exitingTimer = 0f;
                CamController.LocateAt(_positioning);
            }
        }

        private void StopFollowing()
        {
            _camToFollow = null;
            _uiCamInfoPanel.enabled = false;
        }

        void ResetCamLocal()
        {
            _userOffset = Offset.None;

            _targetFieldOfView = Config.G.CamFieldOfView;
            _targetPositioning = _positioning;
        }

        private bool SwitchTarget4WalkThru()    // return whether succeed
        {
            _walkThruTimer = Config.G.Period4WalkThru;
            _idToFollow = GetWalkThruTarget();
            if (_idToFollow is null) return false;
            PrepareFollowing();
            return true;
        }
        private ID GetWalkThruTarget()
        {
            const double pOfHuman = .4;

            var vehicleID = Vehicle.GetRandomID();
            var pedID = Pedestrian.GetRandomID();

            if (vehicleID is object && pedID is object)
                return RandomTrue(pOfHuman) ? (ID) pedID : vehicleID;
            if (vehicleID is object) return vehicleID;
            if (pedID is object) return pedID;

            Log.Msg("GetWalkThruTarget: nothing found");
            return null;
        }

        private void UpdateWalkThru()
        {
            if (!Config.G.ClickToSwitch4WalkThru)
                _walkThruTimer -= Control.DurationFromLastFrame;
            if (_camToFollow is null || (Config.G.ClickToSwitch4WalkThru ?
                                                Control.MouseSecond :
                                                _walkThruTimer <= 0f)) {
                Log.Msg("UpdateWalkThru: switching target");
                if (!SwitchTarget4WalkThru()) {
                    SwitchState(State.Idle);
                    Dialog.ShowMsg("Cannot find any vehicle or citizen");
                }
            }
        }

        private void UpdateFollowCam(Offset controlOffset)
        {
            const float heightMovementFactor = .2f;

            if (_camToFollow is object) {
                // make vertical movement slower
                controlOffset.movement.up *= heightMovementFactor;
                _userOffset = _userOffset.FollowedBy(controlOffset);
                _userOffset.deltaAttitude = _userOffset.deltaAttitude.Clamp(
                        new Range(-Config.G.MaxHoriRotate4Follow, Config.G.MaxHoriRotate4Follow),
                        new Range(-Config.G.MaxVertRotate4Follow, Config.G.MaxVertRotate4Follow));
                if (_camToFollow.GetPositioning() is Positioning followPositioning) {
                    _targetPositioning = followPositioning.Apply(_userOffset);
                    _uiCamInfoPanel.enabled = Config.G.DisplayInfoPanel;
                }
                else _camToFollow = null;
            }
            else if (_state != State.WalkThru) SwitchState(State.Idle);
        }
        private void UpdateFreeCam(Offset controlOffset)
        {
            _targetPositioning = _targetPositioning.Apply(controlOffset);
            _targetPositioning.angle = _targetPositioning.angle.Clamp(
                    pitchRange: new Range(-Config.G.MaxVertRotate, Config.G.MaxVertRotate));

            if (Config.G.GroundClippingOption != Config.GroundClipping.None) {
                var minHeight = Game.Map.GetMinHeightAt(_targetPositioning.position)
                                            + Config.G.GroundLevelOffset;
                if (Config.G.GroundClippingOption == Config.GroundClipping.SnapToGround
                            || _targetPositioning.position.up < minHeight)
                    _targetPositioning.position.up = minHeight;
            }
        }
        private Offset GetControlOffsetAfterHandleInput()
        {
            if (Control.KeyCamToggle) {
                if (isCamOn) SwitchState(State.Idle);
                else StartFreeCam();
            }
            if (!isCamOn) return null;

            if (Control.KeyCamReset) ResetCamLocal();

            var movement = LocalMovement.None;
            var speed = (isFreeCam ? Config.G.MovementSpeed : 1f) *
                        (Control.KeyFaster ? Config.G.SpeedUpFactor : 1f);

            if (Control.KeyForward) movement.forward += 1f;
            if (Control.KeyBackward) movement.forward -= 1f;
            if (Control.KeyRight) movement.right += 1f;
            if (Control.KeyLeft) movement.right -= 1f;
            if (Control.KeyUp) movement.up += 1f;
            if (Control.KeyDown) movement.up -= 1f;
            movement *= speed * Control.DurationFromLastFrame;

            var cursorVisible =
                    isFreeCam && Config.G.ShowCursorWhileFreeCam != Control.KeySwitchCursor ||
                    isFollowing && Config.G.ShowCursorWhileFollow != Control.KeySwitchCursor;
            Control.ShowCursor(cursorVisible);

            float yawDegree = 0f, pitchDegree = 0f;
            if (!cursorVisible) {
                yawDegree = Control.MouseMoveHori * Config.G.RotateSensitivity / 4;
                pitchDegree = Control.MouseMoveVert * Config.G.RotateSensitivity / 4f;
                if (Config.G.InvertRotateHorizontal) yawDegree = -yawDegree;
                if (Config.G.InvertRotateVertical) pitchDegree = -pitchDegree;
            }
            if (Control.KeyRotateR)
                yawDegree += Control.DurationFromLastFrame * Config.G.RotateSensitivity;
            if (Control.KeyRotateL)
                yawDegree -= Control.DurationFromLastFrame * Config.G.RotateSensitivity;
            if (Control.KeyRotateU)
                pitchDegree += Control.DurationFromLastFrame * Config.G.RotateSensitivity;
            if (Control.KeyRotateD)
                pitchDegree -= Control.DurationFromLastFrame * Config.G.RotateSensitivity;

            const float factor = 1.1f;
            var scroll = Control.MouseScroll;
            if (scroll > 0f && _targetFieldOfView > Config.G.CamFieldOfView.Min)
                _targetFieldOfView /= factor;
            else if (scroll < 0f && _targetFieldOfView < Config.G.CamFieldOfView.Max)
                _targetFieldOfView *= factor;

            return new Offset(movement, new DeltaAttitude(yawDegree, pitchDegree));
        }

        private void DestroyUI()
        {
            if (_uiMainPanel != null) Destroy(_uiMainPanel);
            if (_uiCamInfoPanel != null) Destroy(_uiCamInfoPanel);
            if (_uiFollowButtons != null) Destroy(_uiFollowButtons);
        }
        internal void ResetUI()
        {
            DestroyUI();
            _uiMainPanel = gameObject.AddComponent<UI.MainPanel>();
            _uiMainPanel.SetWalkThruCallBack(StartWalkThruMode);
            _uiMainPanel.SetKeyDownEvent(KeyDownHandler);
            _uiCamInfoPanel = gameObject.AddComponent<UI.CamInfoPanel>();
            if (Mod.IsInGameMode) {
                _uiFollowButtons = gameObject.AddComponent<UI.FollowButtons>();
                _uiFollowButtons.registerFollowCallBack(StartFollow);
            }
        }

        // return true if the key is matched and should be consumed
        private bool KeyDownHandler(UnityEngine.KeyCode key)
        {
            bool used = false;
            if (key == UnityEngine.KeyCode.Escape) {
                if (_uiMainPanel.PanelExpanded) { _uiMainPanel.OnEsc(); used = true; }
                if (isCamOn) { SwitchState(State.Idle); used = true; }
            }
            return used;
        }

        private bool GetFinishedAfterUpdateExiting()
        {
            _exitingTimer -= Control.DurationFromLastFrame;
            return _exitingTimer <= 0f ||
                        (_targetFieldOfView.AlmostEqual(_fieldOfView) &&
                        _positioning.AlmostEquals(_targetPositioning));
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
            var controlOffset = GetControlOffsetAfterHandleInput();
            if (isIdle) return;

            switch (_state) {
            case State.FreeCam:
                UpdateFreeCam(controlOffset); break;
            case State.Follow:
                UpdateFollowCam(controlOffset); break;
            case State.WalkThru:
                UpdateFollowCam(controlOffset);
                UpdateWalkThru(); break;
            case State.Exiting:
                if (GetFinishedAfterUpdateExiting()) SwitchState(State.Idle);
                break;
            }

            if (Config.G.SmoothTransition) {
                var distance = _positioning.position.DistanceTo(_targetPositioning.position);
                if (distance > Config.G.GiveUpTransitionDistance) {
                    // TODO: fade out fade in
                    _positioning = _targetPositioning; _fieldOfView = _targetFieldOfView;
                }
                else {
                    var reduceFactor = Config.G.GetReduceFactor(Control.DurationFromLastFrame);

                    var position = isFollowing && distance <= Config.G.InstantMoveMax ?
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
            }
            else { _positioning = _targetPositioning; _fieldOfView = _targetFieldOfView; }

            _camGame.Positioning = _positioning;
            _camGame.FoV = _fieldOfView;
        }

        private Game.Cam _camGame;

        // UI
        private UI.MainPanel _uiMainPanel;
        private UI.CamInfoPanel _uiCamInfoPanel;
        private UI.FollowButtons _uiFollowButtons;

        // local camera properties
        private Offset _userOffset;
        private float _fieldOfView, _targetFieldOfView;
        private Positioning _positioning, _targetPositioning;

        private Positioning _originalPositioning;
        private float _originalFieldOfView;

        // state
        private enum State : byte { Idle, FreeCam, Exiting, Follow, WalkThru }
        private State _state = State.Idle;
        private float _exitingTimer = 0f;

        // state: follow
        private ID _idToFollow;
        private Cam.Base _camToFollow;

        // state: walkThru
        private float _walkThruTimer = 0f;
    }
}
