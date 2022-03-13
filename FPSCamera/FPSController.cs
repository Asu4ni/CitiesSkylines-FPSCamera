using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace FPSCamMod
{
    public class FPSController : MonoBehaviour
    {
        public void StartFreeCam()
        {
            Log.Msg("try starting FreeCam mode");
            SwitchState(State.freeCam);
        }
        public void StartFollow(UUID idToFollow)
        {
            Log.Msg("try starting Follow mode");
            this.idToFollow = idToFollow;
            SwitchState(State.follow);
        }
        public void StartWalkThruMode()
        {
            Log.Msg("try starting WalkThru mode");
            SwitchState(State.walkThru);
        }

        private void Start()
        {
            camGame = CamControllerUT.GetComponent<Camera>();
            camDOF = CamControllerUT.GetComponent<DepthOfField>();
            camTiltEffect = CamControllerUT.GetComponent<TiltShiftEffect>();

            oFieldOfView = camFOV;
            if (camDOF is null) { Log.Msg("component <DepthOfField> not found"); }
            else oDOFEnabled = camDOF.enabled;
            if (camTiltEffect is null) { Log.Msg("component <TiltShiftEffect> not found"); }
            else oTiltEffectEnabled = camTiltEffect.enabled;

            state = State.idle;
            ResetUI();
        }

        private bool isIdle => state == State.idle;
        private bool isFreeCam => state == State.freeCam;
        private bool isFollowing => state == State.follow || state == State.walkThru;
        private bool isCamOn => state == State.freeCam || isFollowing;

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
            Log.Assert((newState == State.idle != isIdle) && newState != State.exiting
                        || (state == State.exiting && newState == State.freeCam),
                       $"FPSController state invalid transition: [{state}] > [{newState}]");

            switch (state) {
            case State.idle: EnableFPSCam(); break;
            case State.freeCam:
                if (newState == State.idle) newState = State.exiting;
                break;
            case State.follow:
            case State.walkThru:
                if (newState == State.idle) newState = State.exiting;
                StopFollowing(); break;
            case State.exiting: break;
            }
            switch (newState) {
            case State.idle: DisableFPSCam(); break;
            case State.freeCam: PrepareFreeCam(); break;
            case State.follow: PrepareFollowing(); break;
            case State.walkThru: PrepareWalkThru(); break;
            case State.exiting: PrepareExiting(); break;
            }
            state = newState;
        }

        private void StopFollowing()
        {
            camToFollow = null;
            if (Config.G.ShowInfoPanel4Follow) followModeUI.enabled = false;
        }

        private void EnableFPSCam()
        {
            Log.Msg("start FPS camera");

            targetSetting = originalSetting = CamSetting;
            configPanelUI.OnCamActivate();
            infoPanelUI.OnCamActivate();
            if (Config.G.HideUIwhenActivate) UIutils.HideUI();

            ResetCamLocal();
            CamControllerUT.Disable();
            camGame.nearClipPlane = 1f;    // TODO: ensure

            if (camDOF) camDOF.enabled = Config.G.EnableDOF;
            if (camTiltEffect) camTiltEffect.enabled = false;
        }
        private void DisableFPSCam()
        {
            Log.Msg("stop FPS camera");

            configPanelUI.OnCamDeactivate();
            infoPanelUI.OnCamDeactivate();
            if (Config.G.HideUIwhenActivate) UIutils.ShowUI();

            CamControllerUT.Enable();
            camFOV = oFieldOfView;

            if (camDOF) camDOF.enabled = oDOFEnabled;
            if (camTiltEffect) camTiltEffect.enabled = oTiltEffectEnabled;
            Cursor.visible = true;
        }


        private void PrepareFreeCam() { }
        private void PrepareFollowing()
        {
            switch (idToFollow.Type.switchValue) {
            case ObjectType.sCitizen:
                camToFollow = new CitizenCam(idToFollow); break;
            case ObjectType.sVehicle:
                camToFollow = new VehicleCam(idToFollow); break;
            default:
                Log.Warn($"Following UUID:{idToFollow} is not supported");
                camToFollow = null;
                return;
            }
            ResetCamLocal();
            if (Config.G.ShowInfoPanel4Follow) followModeUI.SetAssociatedCam(camToFollow);
            Log.Msg($"start following UUID:{idToFollow}");
        }
        private void PrepareWalkThru() { SwitchTarget4WalkThru(); }
        private void PrepareExiting()
        {
            targetFOV = oFieldOfView;
            if (Config.G.SetToOriginalPos) targetSetting = originalSetting;
            else CamControllerUT.LocateAt(CamSetting);
        }

        void ResetCamLocal()
        {
            userOffset = CamOffset.Identity;
            targetFOV = Config.G.CamFieldOfView;
        }

        private bool SwitchTarget4WalkThru()    // return whether succeed
        {
            walkThruTimer = Config.G.Period4WalkThru;
            idToFollow = GetWalkThruTarget();
            if (idToFollow.exists) PrepareFollowing();
            return idToFollow.exists;
        }
        private UUID GetWalkThruTarget()
        {
            bool chooseCitizen = Random.Range(0, 2) == 0;
            camToFollow = null;

            var vehicleID = FPSVehicle.GetRandomID();
            var citizenID = FPSCitizen.GetRandomID();

            if (vehicleID.exists && citizenID.exists)
                return chooseCitizen ? (UUID) citizenID : vehicleID;
            else if (vehicleID.exists) return vehicleID;
            else if (citizenID.exists) return citizenID;
            Log.Msg("GetWalkThruTarget: nothing found");
            return UUID.Empty;
        }

        private float camFOV { get => camGame.fieldOfView; set => camGame.fieldOfView = value; }
        private Vector3 CamPosition {
            get => camGame.transform.position;
            set => camGame.transform.position = value;
        }
        private Quaternion CamRotation {
            get => camGame.transform.rotation;
            set => camGame.transform.rotation = value;
        }
        private CamSetting CamSetting {
            get => new CamSetting(CamPosition, CamRotation);
            set { CamPosition = value.position; CamRotation = value.rotation; }
        }

        private void UpdateWalkThru()
        {
            Log.Assert(state == State.walkThru,
                       "FPSController.UpdateWalkThru called at incorrect state.");

            if (!Config.G.ClickToSwitch4WalkThru) walkThruTimer -= Time.deltaTime;
            if (camToFollow is null || (Config.G.ClickToSwitch4WalkThru ?
                                        ControlUT.MousePrimary : walkThruTimer <= 0.0f)) {
                Log.Msg("UpdateWalkThru: switching target");
                if (!SwitchTarget4WalkThru()) {
                    SwitchState(State.idle);
                    Dialog.ShowMsg("Cannot find any vehicle or citizen");
                }
            }
        }

        private void UpdateFollowCam(CamOffset controlOffset)
        {
            const float heightMovementFactor = .2f;

            if (camToFollow is object) {
                controlOffset.deltaPos.y *= heightMovementFactor;
                var setting = camToFollow.GetNextCamSetting();
                if (camToFollow.isRunning) {
                    targetSetting = setting;
                    userOffset.deltaPos += CamUT.GetRotation(userOffset.deltaEulerXY)
                                           * controlOffset.deltaPos;
                    userOffset.deltaEulerXY += controlOffset.deltaEulerXY;
                    userOffset.deltaEulerXY.x = Utils.ModulusClamp(userOffset.deltaEulerXY.x,
                        -Config.G.MaxVertRotate4Follow, Config.G.MaxVertRotate4Follow, 360, -180);
                    userOffset.deltaEulerXY.y = Utils.ModulusClamp(userOffset.deltaEulerXY.y,
                        -Config.G.MaxHoriRotate4Follow, Config.G.MaxHoriRotate4Follow, 360, -180);

                    targetSetting.position += targetSetting.rotation * userOffset.deltaPos;
                    var euler = targetSetting.rotation.eulerAngles;
                    targetSetting.rotation.eulerAngles =
                        new Vector3(euler.x + userOffset.deltaEulerXY.x,
                                    euler.y + userOffset.deltaEulerXY.y, euler.z);
                }
                else camToFollow = null;
            }
            else if (state != State.walkThru) SwitchState(State.idle);
        }
        private void UpdateFreeCam(CamOffset controlOffset)
        {
            targetSetting.position += CamSetting.rotation * controlOffset.deltaPos;
            var euler = targetSetting.rotation.eulerAngles;
            euler.x = Utils.ModulusClamp(euler.x + controlOffset.deltaEulerXY.x,
                            -Config.G.MaxVertRotate, Config.G.MaxVertRotate, 360f, -180f);
            euler.y += controlOffset.deltaEulerXY.y;
            euler.z = 0;
            targetSetting.rotation.eulerAngles = euler;

            if (Config.G.GroundClippingOption != Config.GroundClipping.None) {
                var minHeight = GameUT.GetMinHeightAt(CamPosition) + Config.G.GroundLevelOffset;
                if (Config.G.GroundClippingOption == Config.GroundClipping.SnapToGround
                    || CamPosition.y < minHeight)
                    targetSetting.position.y = minHeight;
            }
        }
        private CamOffset GetControlOffsetAfterHandleInput()
        {
            if (ControlUT.KeyCamToggle) {
                if (isCamOn) SwitchState(State.idle);
                else StartFreeCam();
            }
            if (!isCamOn) return CamOffset.Identity;

            if (ControlUT.KeyCamReset) ResetCamLocal();

            CamOffset controlOffset = CamOffset.Identity;
            var speed = (isFreeCam ? Config.G.MovementSpeed : 1f) *
                        (ControlUT.KeyFaster ? Config.G.SpeedUpFactor : 1f);

            if (ControlUT.KeyForward) controlOffset.deltaPos += Vector3.forward;
            if (ControlUT.KeyBackward) controlOffset.deltaPos += Vector3.back;
            if (ControlUT.KeyLeft) controlOffset.deltaPos += Vector3.left;
            if (ControlUT.KeyRight) controlOffset.deltaPos += Vector3.right;
            if (ControlUT.KeyUp) controlOffset.deltaPos += Vector3.up;
            if (ControlUT.KeyDown) controlOffset.deltaPos += Vector3.down;
            controlOffset.deltaPos *= speed * Time.deltaTime;

            Cursor.visible =
                isFreeCam && Config.G.ShowCursorWhileFreeCam != ControlUT.KeySwitchCursor
                || isFollowing && Config.G.ShowCursorWhileFollow != ControlUT.KeySwitchCursor;

            ref Vector2 delXY = ref controlOffset.deltaEulerXY;
            if (!Cursor.visible) {
                delXY.y = ControlUT.MouseMoveHori * Config.G.RotateSensitivity / 4f;
                delXY.x = -ControlUT.MouseMoveVert * Config.G.RotateSensitivity / 4f;
                if (Config.G.InvertRotateHorizontal) delXY.y = -delXY.y;
                if (Config.G.InvertRotateVertical) delXY.x = -delXY.x;
            }
            if (ControlUT.KeyRotateL)
                delXY.y -= Time.deltaTime * Config.G.RotateSensitivity;
            if (ControlUT.KeyRotateR)
                delXY.y += Time.deltaTime * Config.G.RotateSensitivity;
            if (ControlUT.KeyRotateU)
                delXY.x -= Time.deltaTime * Config.G.RotateSensitivity;
            if (ControlUT.KeyRotateD)
                delXY.x += Time.deltaTime * Config.G.RotateSensitivity;

            const float factor = 1.1f;
            var scroll = ControlUT.MouseScroll;
            if (scroll > 0f && targetFOV > Config.G.CamFieldOfView.Min) targetFOV /= factor;
            else if (scroll < 0f && targetFOV < Config.G.CamFieldOfView.Max) targetFOV *= factor;

            return controlOffset;
        }

        private void DestroyUI()
        {
            if (configPanelUI is object) Destroy(configPanelUI);
            if (followModeUI is object) Destroy(followModeUI);
            if (infoPanelUI is object) Destroy(infoPanelUI);
        }
        internal void ResetUI()
        {
            DestroyUI();
            configPanelUI = gameObject.AddComponent<ConfigPanelUI>();
            configPanelUI.RegisterWalkThruCallBack(StartWalkThruMode);
            configPanelUI.RegisterKeyDownEvent(ControlUT.GetKeyDownHandler(KeyDownHandler));
            followModeUI = gameObject.AddComponent<FollowModeUI>();
            if (ModLoad.IsInGameMode) {
                infoPanelUI = gameObject.AddComponent<InfoPanelUI>();
                infoPanelUI.registerFollowCallBack(StartFollow);
            }
        }

        // return true if the key is matched and should be consumed
        private bool KeyDownHandler(KeyCode key)
        {
            bool used = false;
            if (key == KeyCode.Escape) {
                if (configPanelUI.PanelExpanded) { configPanelUI.OnEsc(); used = true; }
                if (isCamOn) { SwitchState(State.idle); used = true; }
            }
            return used;
        }

        private void LateUpdate()
        {
            var controlOffset = GetControlOffsetAfterHandleInput();
            if (isIdle) return;

            switch (state) {
            case State.freeCam:
                UpdateFreeCam(controlOffset); break;
            case State.follow:
                UpdateFollowCam(controlOffset); break;
            case State.walkThru:
                UpdateFollowCam(controlOffset);
                UpdateWalkThru(); break;
            case State.exiting:
                if (Utils.AlmostSame(targetFOV, camFOV)
                    && CamSetting == targetSetting) SwitchState(State.idle);
                break;
            }

            if (Config.G.SmoothTransition) {
                var distance = Vector3.Distance(targetSetting.position, CamPosition);
                if (distance > Config.G.GiveUpTransitionDistance) {
                    // TODO: fade out fade in
                    CamSetting = targetSetting; camFOV = targetFOV;
                }
                else {
                    var reduceFactor = Config.G.GetReduceFactor(Time.deltaTime);
                    CamPosition = isFollowing && distance < Config.G.InstantMoveMax ?
                                  targetSetting.position :
                                  GameUT.GetNextPosFromSmoothTrans(
                                            CamPosition, targetSetting.position, reduceFactor,
                                            Config.G.DeltaPosMin, Config.G.DeltaPosMax);
                    CamRotation = GameUT.GetNextQuatFromSmoothTrans(
                                            CamRotation, targetSetting.rotation, reduceFactor,
                                            Config.G.DeltaRotateMin, Config.G.DeltaRotateMax);
                    camFOV = GameUT.GetNextFloatFromSmoothTrans(
                                            camFOV, targetFOV, reduceFactor, 1f, 5f);
                }
            }
            else { CamSetting = targetSetting; camFOV = targetFOV; }
        }
        private void OnDestroy() => DestroyUI();

        private Camera camGame;
        private DepthOfField camDOF;
        private TiltShiftEffect camTiltEffect;
        // CmameraController properties
        private float oFieldOfView;
        private bool oDOFEnabled;
        private bool oTiltEffectEnabled;

        // UI
        private ConfigPanelUI configPanelUI;
        private FollowModeUI followModeUI;
        private InfoPanelUI infoPanelUI;

        // local camera properties
        private CamOffset userOffset;
        private float targetFOV;
        private CamSetting targetSetting, originalSetting;

        // state
        private enum State : byte { idle, freeCam, exiting, follow, walkThru }
        private State state = State.idle;


        // state: follow
        private UUID idToFollow;
        private FPSCam camToFollow = null;

        // state: walkThru
        private float walkThruTimer = 0f;
    }
}
