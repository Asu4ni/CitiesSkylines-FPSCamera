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

        private void StopFollowing()
        {
            camToFollow = null;
            if (Config.Global.displaySpeed) camInfoUI.enabled = false;
        }

        protected void Start()
        {
            camController = FindObjectOfType<CameraController>();
            camUnity = camController.GetComponent<Camera>();
            camDOF = camController.GetComponent<DepthOfField>();
            camTiltEffect = camController.GetComponent<TiltShiftEffect>();

            // TODO: introduce UI Manager
            mainUI = gameObject.AddComponent<FPSCamUI>();
            mainUI.registerWalkThruCallBack(StartWalkThruMode);
            camInfoUI = gameObject.AddComponent<FPSCamInfoUI>();
            Log.Assert(camInfoUI is object, "adding FPSCamInfoUI failed");
            if (ModLoad.IsInGameMode)
            {
                panelUI = gameObject.AddComponent<GamePanelExtender>();
                Log.Assert(panelUI is object, "adding panelUI failed");
                panelUI.registerFollowCallBack(StartFollow);
            }

            oFieldOfView = camUnity.fieldOfView;
            oMaxDistance = camController.m_maxDistance;
            if (camDOF is null) { Log.Msg("component Camera not found"); }
            else oDOFEnabled = camDOF.enabled;
            if (camTiltEffect is null) { Log.Msg("component camTiltEffect not found"); }
            else oTiltEffectEnabled = camTiltEffect.enabled;
            camUnity.nearClipPlane = 1.0f;  // TODO: need restore while turn off?

            state = State.idle;
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
                if (Config.Global.animateTransitions && newState == State.idle)
                    newState = State.exitFreeCam;
                break;
            case State.follow:
            case State.walkThru:
                if (Config.Global.animateTransitions && newState == State.idle)
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

        private void EnableFPSCam()
        {
            Log.Msg("start FPS camera");

            transitionTarget = (CamSetting) camUnity.transform;
            mainUI.Deactivate();
            if (Config.Global.integrateHideUI) UIHider.Hide();

            targetFOV = Config.Global.fieldOfView;

            camController.m_maxDistance = 50f;  // TODO: consider to remove
            camController.enabled = false;

            if (camDOF) camDOF.enabled = Config.Global.enableDOF;
            if (camTiltEffect) camTiltEffect.enabled = false;
        }
        private void DisableFPSCam()
        {
            Log.Msg("stop FPS camera");

            mainUI.Activate();
            if (Config.Global.integrateHideUI) UIHider.Show();

            camController.enabled = true;
            camUnity.fieldOfView = oFieldOfView;
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
            if (Config.Global.displaySpeed) camInfoUI.SetAssociatedCam(camToFollow);
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
            targetFOV = Config.Global.fieldOfView;
        }

        private bool SwitchTarget4WalkThru()    // return whether succeed
        {
            walkThruTimer = Config.Global.walkthroughModeTimer;
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

            if (!Config.Global.walkThruManualSwitch) walkThruTimer -= Time.deltaTime;
            if (camToFollow is null || (Config.Global.walkThruManualSwitch ?
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

        private CamSetting GetTargetSettingAfterUpdateFollowCam(CamOffset controlOffset)
        {
            const float heightMovementFactor = .1f;
            const float fovX = 30f, fovY = 45f;

            if (camToFollow is object)
            {
                controlOffset.deltaPos.y *= heightMovementFactor;
                var setting = camToFollow.GetNextCamSetting();
                if (camToFollow.isRunning)
                {
                    userOffset.deltaPos += CamUT.GetRotation(userOffset.deltaEulerXY)
                                           * controlOffset.deltaPos;
                    userOffset.deltaEulerXY += controlOffset.deltaEulerXY;
                    userOffset.deltaEulerXY.x =
                        Utils.ModulusClamp(userOffset.deltaEulerXY.x, -fovX, fovX, 360, -180);
                    userOffset.deltaEulerXY.y =
                        Utils.ModulusClamp(userOffset.deltaEulerXY.y, -fovY, fovY, 360, -180);

                    setting.position += setting.rotation * userOffset.deltaPos;
                    var euler = setting.rotation.eulerAngles;
                    setting.rotation.eulerAngles =
                        new Vector3(euler.x + userOffset.deltaEulerXY.x,
                                    euler.y + userOffset.deltaEulerXY.y, euler.z);
                    return setting;
                }
                camToFollow = null;
            }
            if (state != State.walkThru) SwitchState(State.idle);
            return camSetting;  // remain the same
        }
        private CamSetting GetTargetSettingAfterUpdateFreeCam(CamOffset controlOffset)
        {
            const float fovX = 70f;

            CamSetting setting = camSetting;
            setting.position += setting.rotation * controlOffset.deltaPos;
            var euler = setting.rotation.eulerAngles;
            euler.x = Utils.ModulusClamp(euler.x + controlOffset.deltaEulerXY.x,
                                                -fovX, fovX, 360f, -180f);
            euler.y += controlOffset.deltaEulerXY.y;
            euler.z = 0;
            setting.rotation.eulerAngles = euler;

            if (Config.Global.snapToGround || Config.Global.preventClipGround)
            {
                var minHeight = GeneralUT.GetMinHeightAt(camPosition)
                                + Config.Global.groundOffset * 3f;    // TODO: find suitable factor
                if (Config.Global.snapToGround || camPosition.y < minHeight)
                    setting.position.y = minHeight;
            }
            return setting;
        }
        private CamOffset GetControlOffsetAfterHandleInput()
        {
            if (ControlUT.KeyToggle || ControlUT.KeyEsc)
            {
                if (isCamOn) SwitchState(State.idle);
                else if (ControlUT.KeyToggle) StartFreeCam();
            }
            if (!isCamOn) return CamOffset.Identity;

            if (ControlUT.KeyReset) ResetCamLocal();

            CamOffset controlOffset = CamOffset.Identity;
            var speed = (isFreeCam ? Config.Global.cameraMoveSpeed : 1f) *
                        (ControlUT.KeyFaster ? Config.Global.goFasterSpeedMultiplier : 1f);

            if (ControlUT.KeyForward) controlOffset.deltaPos += Vector3.forward;
            if (ControlUT.KeyBackward) controlOffset.deltaPos += Vector3.back;
            if (ControlUT.KeyLeft) controlOffset.deltaPos += Vector3.left;
            if (ControlUT.KeyRight) controlOffset.deltaPos += Vector3.right;
            if (ControlUT.KeyUp) controlOffset.deltaPos += Vector3.up;
            if (ControlUT.KeyDown) controlOffset.deltaPos += Vector3.down;
            controlOffset.deltaPos *= speed * Time.deltaTime;

            Cursor.visible =
                isFreeCam && Config.Global.showCursorWhileFreeCam != ControlUT.KeySwitchCursor
                || isFollowing && Config.Global.showCursorWhileFollow != ControlUT.KeySwitchCursor;


            ref Vector2 delXY = ref controlOffset.deltaEulerXY;
            if (!Cursor.visible)
            {
                delXY.y = ControlUT.MouseMoveHori * Config.Global.cameraRotationSensitivity;
                delXY.x = -ControlUT.MouseMoveVert * Config.Global.cameraRotationSensitivity;
                if (Config.Global.invertRotateHorizontal) delXY.y = -delXY.y;
                if (Config.Global.invertRotateVertical) delXY.x = -delXY.x;
            }

            // TODO: ensure factor 2f
            if (ControlUT.KeyRotateL)
                delXY.y -= Time.deltaTime * Config.Global.cameraRotationSensitivity * 2f;
            if (ControlUT.KeyRotateR)
                delXY.y += Time.deltaTime * Config.Global.cameraRotationSensitivity * 2f;

            // TODO: ENSURE FACTOR
            float maxFOV = 75f, minFOV = 10f, factor = 1.1f;
            var scroll = ControlUT.MouseScroll;
            if (scroll > 0f) { if (targetFOV > minFOV) targetFOV /= factor; }
            else if (scroll < 0f && targetFOV < maxFOV) targetFOV *= factor;

            return controlOffset;
        }

        private void LateUpdate()
        {
            var controlOffset = GetControlOffsetAfterHandleInput();
            if (isIdle) return;

            camUnity.fieldOfView = Config.Global.animateTransitions ?
                    Utils.GetNextValueOfSmoothTransition(
                        camUnity.fieldOfView, targetFOV, Time.deltaTime, 1 / 3f, .5f, 5f)
                    : targetFOV;
            // TODO: smooth transition for all situation
            switch (state)
            {
            case State.freeCam:
                camSetting = GetTargetSettingAfterUpdateFreeCam(controlOffset); break;
            case State.follow:
                camSetting = GetTargetSettingAfterUpdateFollowCam(controlOffset); break;
            case State.walkThru:
                camSetting = GetTargetSettingAfterUpdateFollowCam(controlOffset);
                UpdateWalkThru(); break;
            case State.exitFreeCam:
                if (Utils.AlmostSame(targetFOV, camUnity.fieldOfView)
                    && camSetting == transitionTarget) SwitchState(State.idle);
                else camSetting = CamUT.GetNextSettingOfSmoothTransition(
                                        camSetting, transitionTarget, Time.deltaTime);
                break;
            }
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
        private FPSCamUI mainUI;
        private FPSCamInfoUI camInfoUI;
        private GamePanelExtender panelUI;

        // local camera properties
        private CamOffset userOffset;
        private float targetFOV;

        // state
        private enum State : byte { idle, freeCam, exitFreeCam, follow, walkThru }
        private State state = State.idle;

        // state: exitFreeCam
        // TODO: config to switch on/off
        private CamSetting transitionTarget;

        // state: follow
        private UUID idToFollow;
        private FPSCam camToFollow = null;

        // state: walkThru
        private float walkThruTimer = 0f;
    }
}
