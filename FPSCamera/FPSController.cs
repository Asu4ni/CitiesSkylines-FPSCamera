using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace FPSCamMod
{
    // TODO: make static
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

        protected void Start()
        {
            camController = FindObjectOfType<CameraController>();
            camUnity = camController.GetComponent<Camera>();
            camDOF = camController.GetComponent<DepthOfField>();
            camTiltEffect = camController.GetComponent<TiltShiftEffect>();

            oFieldOfView = camFOV;
            oMaxDistance = camController.m_maxDistance;
            if (camDOF is null) { Log.Msg("component Camera not found"); }
            else oDOFEnabled = camDOF.enabled;
            if (camTiltEffect is null) { Log.Msg("component camTiltEffect not found"); }
            else oTiltEffectEnabled = camTiltEffect.enabled;
            camUnity.nearClipPlane = 1.0f;  // TODO: need restore while turn off?

            state = State.idle;
            ResetUI();
        }

        private bool isIdle => state == State.idle;
        private bool isFreeCam => state == State.freeCam;
        private bool isFollowing => state == State.follow || state == State.walkThru;
        private bool isCamOn => state == State.freeCam || isFollowing;

        /*  state transitions:
         *  
         *  Fr\To   |      idle      |  freeCam  |   follow  |  walkThru | exitFree (eF)   
         *  idle    |        -       |   E > P   |   E > P   |   E > P   |      -       
         *  freeCam |    ->eF / D    |     -     |     -     |     -     | (redirect) P 
         *  follow  | S > (->eF / D) |     -     |     -     |     -     | (redirect) P
         *  walkThru| S > (->eF / D) |     -     |     -     |     -     | (redirect) P
         *  exitFree|        D       |     P     |     -     |     -     |      -       
         *  
         *    * -: impossible | E: EnableFPSCam | D: DisableFPSCam | S: StopFollow
         */
        private void SwitchState(State newState)
        {
            Log.Assert((newState == State.idle != isIdle) && newState != State.exitFreeCam
                        || (state == State.exitFreeCam && newState == State.freeCam),
                       $"FPSController state invalid transition: [{state}] > [{newState}]");

            switch (state)
            {
            case State.idle: EnableFPSCam(); break;
            case State.freeCam:
                if (Config.G.SmoothTransition && newState == State.idle)
                    newState = State.exitFreeCam;
                break;
            case State.follow:
            case State.walkThru:
                if (Config.G.SmoothTransition && newState == State.idle)
                    newState = State.exitFreeCam;
                StopFollowing(); break;
            case State.exitFreeCam: break;
            }
            switch (newState)
            {
            case State.idle: DisableFPSCam(); break;
            case State.freeCam: PrepareFreeCam(); break;
            case State.follow: PrepareFollowing(); break;
            case State.walkThru: PrepareWalkThru(); break;
            case State.exitFreeCam: PrepareExitFreeCam(); break;
            }
            state = newState;
        }

        private void StopFollowing()
        {
            camToFollow = null;
            if (Config.G.ShowInfoPanel4Follow) followCamUI.enabled = false;
        }

        private void EnableFPSCam()
        {
            Log.Msg("start FPS camera");

            transitionTarget = (CamSetting) camUnity.transform;
            configPanelUI.OnCamActivate();
            infoPanelUI.OnCamActivate();
            if (Config.G.HideUIwhenActivate) UIUT.HideUI();

            ResetCamLocal();
            camController.m_maxDistance = 50f;  // TODO: consider to remove
            camController.enabled = false;

            if (camDOF) camDOF.enabled = Config.G.EnableDOF;
            if (camTiltEffect) camTiltEffect.enabled = false;
        }
        private void DisableFPSCam()
        {
            Log.Msg("stop FPS camera");

            configPanelUI.OnCamDeactivate();
            infoPanelUI.OnCamDeactivate();
            if (Config.G.HideUIwhenActivate) UIUT.ShowUI();

            camController.enabled = true;
            camFOV = oFieldOfView;
            camController.m_maxDistance = oMaxDistance;

            if (camDOF) camDOF.enabled = oDOFEnabled;
            if (camTiltEffect) camTiltEffect.enabled = oTiltEffectEnabled;
            Cursor.visible = true;
        }


        private void PrepareFreeCam() { }
        private void PrepareFollowing()
        {
            switch (idToFollow.Type.switchValue)
            {
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
            if (Config.G.ShowInfoPanel4Follow) followCamUI.SetAssociatedCam(camToFollow);
            Log.Msg($"start following UUID:{idToFollow}");
        }
        private void PrepareWalkThru() { SwitchTarget4WalkThru(); }
        private void PrepareExitFreeCam()
        {
            Log.Msg("FPSController: transition to exit FreeCam");
            targetFOV = oFieldOfView;
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
            bool vehicleOrCitizen = Random.Range(0, 1) == 0;
            camToFollow = null;

            var vehicleID = (VehicleID) GetRandomVehicle();
            var citizenID = (CitizenID) GetRandomCitizenInstance();

            if (vehicleID.exists && citizenID.exists)
                return vehicleOrCitizen ? (UUID) vehicleID : citizenID;
            else if (vehicleID.exists) return vehicleID;
            else if (citizenID.exists) return citizenID;
            Log.Msg("GetWalkThruTarget: nothing found");
            return UUID.Empty;
        }

        private float camFOV { get => camUnity.fieldOfView; set => camUnity.fieldOfView = value; }
        private Vector3 camPosition
        {
            get => camUnity.transform.position;
            set => camUnity.transform.position = value;
        }
        private Quaternion camRotation
        {
            get => camUnity.transform.rotation;
            set => camUnity.transform.rotation = value;
        }
        private CamSetting camSetting
        {
            get => new CamSetting(camPosition, camRotation);
            set { camPosition = value.position; camRotation = value.rotation; }
        }

        private ushort GetRandomVehicle()
        {
            var vmanager = VehicleManager.instance;
            int skip = Random.Range(0, vmanager.m_vehicleCount - 1);

            for (ushort i = 0; i < vmanager.m_vehicles.m_buffer.Length; i++)
            {
                if ((vmanager.m_vehicles.m_buffer[i].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) != Vehicle.Flags.Created)
                {
                    continue;
                }

                if (vmanager.m_vehicles.m_buffer[i].Info.m_vehicleAI is CarTrailerAI)
                {
                    continue;
                }

                if (skip > 0)
                {
                    skip--;
                    continue;
                }

                return i;
            }

            for (ushort i = 0; i < vmanager.m_vehicles.m_buffer.Length; i++)
            {
                if ((vmanager.m_vehicles.m_buffer[i].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) !=
                    Vehicle.Flags.Created)
                {
                    continue;
                }

                if (vmanager.m_vehicles.m_buffer[i].Info.m_vehicleAI is CarTrailerAI)
                {
                    continue;
                }

                return i;
            }

            return 0;
        }
        private uint GetRandomCitizenInstance()
        {
            var cmanager = CitizenManager.instance;
            int skip = Random.Range(0, cmanager.m_instanceCount - 1);

            for (uint i = 0; i < cmanager.m_instances.m_buffer.Length; i++)
            {
                if ((cmanager.m_instances.m_buffer[i].m_flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted)) != CitizenInstance.Flags.Created)
                {
                    continue;
                }

                if (skip > 0)
                {
                    skip--;
                    continue;
                }

                return cmanager.m_instances.m_buffer[i].m_citizen;
            }

            for (uint i = 0; i < cmanager.m_instances.m_buffer.Length; i++)
            {
                if ((cmanager.m_instances.m_buffer[i].m_flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted)) != CitizenInstance.Flags.Created)
                {
                    continue;
                }

                return cmanager.m_instances.m_buffer[i].m_citizen;
            }

            return 0;
        }

        private void UpdateWalkThru()
        {
            Log.Assert(state == State.walkThru,
                       "FPSController.UpdateWalkThru called at incorrect state.");

            if (!Config.G.ClickToSwitch4WalkThru) walkThruTimer -= Time.deltaTime;
            if (camToFollow is null || (Config.G.ClickToSwitch4WalkThru ?
                                        ControlUT.MousePrimary : walkThruTimer <= 0.0f))
            {
                Log.Msg("UpdateWalkThru: switching target");
                if (!SwitchTarget4WalkThru())
                {
                    SwitchState(State.idle);
                    Dialog.ShowMsg("Cannot find any vehicle or citizen");
                }
            }
        }

        private void UpdateFollowCam(CamOffset controlOffset)
        {
            const float heightMovementFactor = .1f;

            if (camToFollow is object)
            {
                controlOffset.deltaPos.y *= heightMovementFactor;
                var setting = camToFollow.GetNextCamSetting();
                if (camToFollow.isRunning)
                {
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
            targetSetting.position += camSetting.rotation * controlOffset.deltaPos;
            var euler = targetSetting.rotation.eulerAngles;
            euler.x = Utils.ModulusClamp(euler.x + controlOffset.deltaEulerXY.x,
                            -Config.G.MaxVertRotate, Config.G.MaxVertRotate, 360f, -180f);
            euler.y += controlOffset.deltaEulerXY.y;
            euler.z = 0;
            targetSetting.rotation.eulerAngles = euler;

            if (Config.G.GroundClippingOption != Config.GroundClipping.None)
            {
                var minHeight = GameUT.GetMinHeightAt(camPosition) + Config.G.DistanceFromGround;
                if (Config.G.GroundClippingOption == Config.GroundClipping.SnapToGround
                    || camPosition.y < minHeight)
                    targetSetting.position.y = minHeight;
            }
        }
        private CamOffset GetControlOffsetAfterHandleInput()
        {
            if (ControlUT.KeyToggle || ControlUT.KeyEsc)
            {
                if (ControlUT.KeyEsc) configPanelUI.onEsc();
                if (isCamOn) SwitchState(State.idle);
                else if (ControlUT.KeyToggle) StartFreeCam();
            }
            if (!isCamOn) return CamOffset.Identity;

            if (ControlUT.KeyReset) ResetCamLocal();

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


            // TODO: ensure rotateSensitivity factor 2f
            ref Vector2 delXY = ref controlOffset.deltaEulerXY;
            if (!Cursor.visible)
            {
                delXY.y = ControlUT.MouseMoveHori * Config.G.rotateSensitivity / 4f;
                delXY.x = -ControlUT.MouseMoveVert * Config.G.rotateSensitivity / 4f;
                if (Config.G.InvertRotateHorizontal) delXY.y = -delXY.y;
                if (Config.G.InvertRotateVertical) delXY.x = -delXY.x;
            }
            if (ControlUT.KeyRotateL)
                delXY.y -= Time.deltaTime * Config.G.rotateSensitivity;
            if (ControlUT.KeyRotateR)
                delXY.y += Time.deltaTime * Config.G.rotateSensitivity;
            if (ControlUT.KeyRotateU)
                delXY.x -= Time.deltaTime * Config.G.rotateSensitivity;
            if (ControlUT.KeyRotateD)
                delXY.x += Time.deltaTime * Config.G.rotateSensitivity;

            // TODO: ENSURE FACTOR
            float factor = 1.1f;
            var scroll = ControlUT.MouseScroll;
            if (scroll > 0f && targetFOV > Config.G.CamFieldOfView.Min) targetFOV /= factor;
            else if (scroll < 0f && targetFOV < Config.G.CamFieldOfView.Max) targetFOV *= factor;

            return controlOffset;
        }

        internal void ResetUI()
        {
            if (configPanelUI is object) Destroy(configPanelUI);
            if (followCamUI is object) Destroy(followCamUI);
            if (infoPanelUI is object) Destroy(infoPanelUI);

            // TODO: introduce UI Manager
            configPanelUI = gameObject.AddComponent<ConfigPanelUI>();
            configPanelUI.registerWalkThruCallBack(StartWalkThruMode);
            followCamUI = gameObject.AddComponent<FolloCamUI>();
            if (ModLoad.IsInGameMode)
            {
                infoPanelUI = gameObject.AddComponent<InfoPanelUI>();
                infoPanelUI.registerFollowCallBack(StartFollow);
            }
        }

        private void LateUpdate()
        {
            var controlOffset = GetControlOffsetAfterHandleInput();
            if (isIdle) return;

            camFOV = Config.G.SmoothTransition ?
                            CamUT.GetNextValueOfSmoothTransition(
                                    camFOV, targetFOV, Time.deltaTime, 1 / 3f, .5f, 5f)
                            : targetFOV;

            CamSetting targetSetting = camSetting;
            switch (state)
            {
            case State.freeCam:
                UpdateFreeCam(controlOffset); break;
            case State.follow:
                UpdateFollowCam(controlOffset); break;
            case State.walkThru:
                UpdateFollowCam(controlOffset);
                UpdateWalkThru(); break;
            case State.exitFreeCam:
                if (Utils.AlmostSame(targetFOV, camFOV)
                    && camSetting == targetSetting) SwitchState(State.idle);
                break;
            }
            camSetting = Config.G.SmoothTransition ?
                                CamUT.GetNextSettingOfSmoothTransition(
                                        camSetting, targetSetting, Time.deltaTime)
                                : targetSetting;
            // TODO: better solution like using velocity?
            // Following Cam requires instant movement
            if (Config.G.SmoothTransition && isFollowing &&
                     (targetSetting.position - camPosition).magnitude < 10f)
                camPosition = targetSetting.position;
        }

        // TODO: expect to remove
        private CameraController camController;
        private Camera camUnity;
        private DepthOfField camDOF;
        private TiltShiftEffect camTiltEffect;
        // CmameraController properties
        private float oFieldOfView;
        private float oMaxDistance;
        private bool oDOFEnabled;
        private bool oTiltEffectEnabled;

        // UI
        private ConfigPanelUI configPanelUI;
        private FolloCamUI followCamUI;
        private InfoPanelUI infoPanelUI;

        // local camera properties
        private CamOffset userOffset;
        private float targetFOV;
        private CamSetting targetSetting;

        // state
        private enum State : byte { idle, freeCam, exitFreeCam, follow, walkThru }
        private State state = State.idle;


        // state: follow
        private UUID idToFollow;
        private FPSCam camToFollow = null;

        // state: walkThru
        private float walkThruTimer = 0f;
    }
}
