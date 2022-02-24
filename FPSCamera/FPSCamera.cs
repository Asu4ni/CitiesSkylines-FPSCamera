using ColossalFramework.Math;
using ICities;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace FPSCamera
{
    public class FPSCamera : MonoBehaviour
    {
        public static FPSCamera Instance { get; private set; }

        public delegate void OnCameraModeChanged(bool state);
        public OnCameraModeChanged onCameraModeChanged;

        public delegate void OnUpdate();
        public OnUpdate onUpdate;

        public bool IsGameMode { get; private set; }

        private bool fpsModeEnabled = false;
        private CameraController controller;
        private Camera camera;
        float rotationY = 0f;

        private DepthOfField effect;
        private TiltShiftEffect legacyEffect;

        private Vector3 mainCameraPosition;
        private Quaternion mainCameraOrientation;

        public VehicleCamera vehicleCamera { get; private set; }
        public CitizenCamera citizenCamera { get; private set; }

        private bool cityWalkthroughMode = false;
        private float cityWalkthroughNextChangeTimer = 0.0f;

        public float originalFieldOfView { get; private set; } = 0.0f;

        private static readonly List<int> focalDistanceList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 25, 35, 45, 50, 75, 100, 150, 200, 500, 750, 1000, 2000, 5000, 10000 };
        private int focalIndex = 9;

        private bool inModeTransition = false;
        private Vector3 transitionTargetPosition = Vector3.zero;
        private Quaternion transitionTargetOrientation = Quaternion.identity;
        private Vector3 transitionStartPosition = Vector3.zero;
        private Quaternion transitionStartOrientation = Quaternion.identity;
        private float transitionT = 0.0f;
        private float existingTime = 0.0f;

        public FPSCameraUI UI { get; private set; }
        private FPSCameraSpeedUI speedUi;

        public static void Initialize(LoadMode mode)
        {
            if (Instance == null)
            {
                Instance = GameObject.FindObjectOfType<CameraController>().gameObject.AddComponent<FPSCamera>();
                Instance.IsGameMode = mode == LoadMode.LoadGame || mode == LoadMode.NewGame;

                if (Instance.IsGameMode)
                {
                    Instance.gameObject.AddComponent<GamePanelExtender>();
                    Instance.vehicleCamera = new VehicleCamera(Instance.gameObject);
                    Instance.citizenCamera = new CitizenCamera(Instance.gameObject, 
                                                               Instance.vehicleCamera);
                }
            }
        }

        public static void Deinitialize()
        {
            Destroy(Instance);
        }

        void Start()
        {
            controller = FindObjectOfType<CameraController>();
            camera = controller.GetComponent<Camera>();
            originalFieldOfView = camera.fieldOfView;
            effect = controller.GetComponent<DepthOfField>();
            legacyEffect = controller.GetComponent<TiltShiftEffect>();

            Config.Global = Config.Load() ?? Config.Global;

            SaveConfig();

            mainCameraPosition = gameObject.transform.position;
            mainCameraOrientation = gameObject.transform.rotation;

            mainCameraPosition = gameObject.transform.position;
            mainCameraOrientation = gameObject.transform.rotation;
            rotationY = -Instance.transform.localEulerAngles.x;

            UI = FPSCameraUI.Instance;
            speedUi = FPSCameraSpeedUI.Instance;
            speedUi.enabled = false;
            
        }
        
        public void SaveConfig()
        {
            Config.Save(Config.Global);
        }

        public void SetFieldOfView(float fov)
        {
            Config.Global.fieldOfView = fov;
            SaveConfig();
            if (fpsModeEnabled)
            {
                camera.fieldOfView = fov;
            }
        }

        public void EnterWalkthroughMode()
        {
            cityWalkthroughMode = true;
            cityWalkthroughNextChangeTimer = Config.Global.walkthroughModeTimer;

            if ( Config.Global.integrateHideUI)
            {
                UIHider.Hide();
            }

            WalkthroughModeSwitchTarget();
            FPSCameraUI.Instance.Hide();
        }

        public void ResetConfig()
        {
            Config.Global = new Config();
            SaveConfig();

            Destroy(FPSCameraUI.instance);
            FPSCameraUI.instance = null;
            UI = FPSCameraUI.Instance;
            UI.Show();
        }

        public void SetMode(bool fpsMode)
        {
            Instance.fpsModeEnabled = fpsMode;
            FPSCameraUI.instance.Hide();

            if (Config.Global.integrateHideUI)
            {
                if (Instance.fpsModeEnabled)
                {
                    UIHider.Hide();
                }
                else
                {
                    UIHider.Show();
                }
            }

            if (Instance.fpsModeEnabled)
            {
                camera.fieldOfView = Config.Global.fieldOfView;
                camera.nearClipPlane = 1.0f;

                Instance.controller.m_maxDistance = 50f;
                Instance.controller.m_currentSize = 5;
                Instance.controller.m_currentHeight =2f;
                Instance.controller.enabled = false;
                if (effect)
                {
                    effect.enabled = Config.Global.enableDOF;
                }
                if( legacyEffect)
                {
                    legacyEffect.enabled = false;
                }

                Cursor.visible = false;
                Instance.rotationY = -Instance.transform.localEulerAngles.x;
            }
            else
            {
                Instance.controller.m_maxDistance = 10000f;

                if (!Config.Global.animateTransitions)
                {
                    Instance.controller.enabled = true;
                }

                camera.fieldOfView = originalFieldOfView;
                Cursor.visible = true;
                effect.nearBlur = true;
            }
            
            onCameraModeChanged?.Invoke(fpsMode);
        }

        public static bool IsEnabled()
        {
            return Instance.fpsModeEnabled;
        }

        public ushort GetRandomVehicle()
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

                if(skip > 0)
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
        
        public uint GetRandomCitizenInstance()
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

        void WalkthroughModeSwitchTarget()
        {
            bool vehicleOrCitizen = Random.Range(0, 3) == 0;
            if (!vehicleOrCitizen)
            {
                if (citizenCamera.enabled)
                {
                    citizenCamera.StopFollowing();
                }

                var vehicle = GetRandomVehicle();
                if (vehicle != 0)
                {
                    vehicleCamera.SetInstanceToFollow((VehicleID) vehicle);
                }
            }
            else
            {
                if (vehicleCamera.enabled)
                {
                    vehicleCamera.StopFollowing();
                }

                var citizen = GetRandomCitizenInstance();
                if (citizen != 0)
                {
                    citizenCamera.SetInstanceToFollow((CitizenID) citizen);
                }
            }
        }

        void UpdateCityWalkthrough()
        {
            if (cityWalkthroughMode && !Config.Global.walkthroughModeManual)
            {
                cityWalkthroughNextChangeTimer -= Time.deltaTime;
                if (cityWalkthroughNextChangeTimer <= 0.0f || !(citizenCamera.enabled || vehicleCamera.enabled))
                {
                    cityWalkthroughNextChangeTimer = Config.Global.walkthroughModeTimer;
                    WalkthroughModeSwitchTarget();
                }
            }
            else if (cityWalkthroughMode)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    WalkthroughModeSwitchTarget();
                }
            }
        }

        void UpdateCameras()
        {
            if (vehicleCamera != null && vehicleCamera.enabled && Config.Global.allowUserOffsetInVehicleCitizenMode)
            {
                if (Input.GetKey(Config.Global.cameraMoveForward))
                {
                    vehicleCamera.userOffset += gameObject.transform.forward * Config.Global.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }
                else if (Input.GetKey(Config.Global.cameraMoveBackward))
                {
                    vehicleCamera.userOffset -= gameObject.transform.forward * Config.Global.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }

                if (Input.GetKey(Config.Global.cameraMoveLeft))
                {
                    vehicleCamera.userOffset -= gameObject.transform.right * Config.Global.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }
                else if (Input.GetKey(Config.Global.cameraMoveRight))
                {
                    vehicleCamera.userOffset += gameObject.transform.right * Config.Global.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }

                if (Input.GetKey(Config.Global.cameraZoomAway))
                {
                    vehicleCamera.userOffset -= gameObject.transform.up * Config.Global.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }
                else if (Input.GetKey(Config.Global.cameraZoomCloser))
                {
                    vehicleCamera.userOffset += gameObject.transform.up * Config.Global.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }

                if(Input.GetKeyDown(Config.Global.cameraRotateLeft))
                {
                    vehicleCamera.cameraRotationOffset -= 45;
                    if(vehicleCamera.cameraRotationOffset <= -360)
                    {
                        vehicleCamera.cameraRotationOffset = 0;
                    }
                }
                else if(Input.GetKeyDown(Config.Global.cameraRotateRight))
                {
                    vehicleCamera.cameraRotationOffset += 45;
                    if (vehicleCamera.cameraRotationOffset >= 360)
                    {
                        vehicleCamera.cameraRotationOffset = 0;
                    }
                }
            }

            if (citizenCamera != null && citizenCamera.enabled && Config.Global.allowUserOffsetInVehicleCitizenMode)
            {
                if (Input.GetKey(Config.Global.cameraMoveForward))
                {
                    citizenCamera.userOffset += gameObject.transform.forward * Config.Global.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }
                else if (Input.GetKey(Config.Global.cameraMoveBackward))
                {
                    citizenCamera.userOffset -= gameObject.transform.forward * Config.Global.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }

                if (Input.GetKey(Config.Global.cameraMoveLeft))
                {
                    citizenCamera.userOffset -= gameObject.transform.right * Config.Global.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }
                else if (Input.GetKey(Config.Global.cameraMoveRight))
                {
                    citizenCamera.userOffset += gameObject.transform.right * Config.Global.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }

                if (Input.GetKey(Config.Global.cameraZoomAway))
                {
                    citizenCamera.userOffset -= gameObject.transform.up * Config.Global.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }
                else if (Input.GetKey(Config.Global.cameraZoomCloser))
                {
                    citizenCamera.userOffset += gameObject.transform.up * Config.Global.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }
            }
        }

        void OnEscapePressed()
        {

            if (cityWalkthroughMode)
            {

                cityWalkthroughMode = false;
                if(Config.Global.integrateHideUI)
                {
                    UIHider.Hide();
                }

                if (vehicleCamera != null && vehicleCamera.enabled)
                {
                    vehicleCamera.StopFollowing();
                }

                if (citizenCamera != null && citizenCamera.enabled)
                {
                    citizenCamera.StopFollowing();
                }
                
            }
            else
            {
                if (vehicleCamera != null && vehicleCamera.enabled)
                {
                    if (Config.Global.integrateHideUI)
                    {
                        UIHider.Hide();
                    }
                    vehicleCamera.StopFollowing();
                }
                if (citizenCamera != null && citizenCamera.enabled)
                {
                    if (Config.Global.integrateHideUI)
                    {
                        UIHider.Hide();
                    }
                    citizenCamera.StopFollowing();
                }
                if (fpsModeEnabled)
                {
                    if (Config.Global.animateTransitions && fpsModeEnabled)
                    {
                        inModeTransition = true;
                        transitionT = 0.0f;

                        if ((gameObject.transform.position - mainCameraPosition).magnitude <= 1.0f)
                        {
                            transitionT = 1.0f;
                            mainCameraOrientation = gameObject.transform.rotation;
                        }

                        transitionStartPosition = gameObject.transform.position;
                        transitionStartOrientation = gameObject.transform.rotation;

                        transitionTargetPosition = mainCameraPosition;
                        transitionTargetOrientation = mainCameraOrientation;
                    }

                    SetMode(!fpsModeEnabled);
                }
            }
            
            UI.Hide();
        }

        void OnToggleCameraHotkeyPressed()
        {
            if (cityWalkthroughMode)
            {
                if (Config.Global.integrateHideUI)
                {
                    UIHider.Show();
                }

                cityWalkthroughMode = false;
                if (vehicleCamera.enabled)
                {
                    vehicleCamera.StopFollowing();
                }
                if (citizenCamera.enabled)
                {
                    citizenCamera.StopFollowing();
                }


            }
            else if (vehicleCamera != null && vehicleCamera.enabled)
            {
                vehicleCamera.StopFollowing();
            }
            else if (citizenCamera != null && citizenCamera.enabled)
            {
                citizenCamera.StopFollowing();
            }
            else
            {
                if (Config.Global.animateTransitions && fpsModeEnabled)
                {
                    inModeTransition = true;
                    transitionT = 0.0f;

                    if ((gameObject.transform.position - mainCameraPosition).magnitude <= 1.0f)
                    {
                        transitionT = 1.0f;
                        mainCameraOrientation = gameObject.transform.rotation;
                    }

                    transitionStartPosition = gameObject.transform.position;
                    transitionStartOrientation = gameObject.transform.rotation;

                    transitionTargetPosition = mainCameraPosition;
                    transitionTargetOrientation = mainCameraOrientation;
                }

                SetMode(!fpsModeEnabled);
            }
            UI.Hide();
        }

        public void LateUpdate()
        {
            float time = Time.deltaTime;
            if (time + existingTime <= 0.1)
            {
                existingTime+= time;
            }
            existingTime = 0.0f;


            onUpdate?.Invoke();

            UpdateCityWalkthrough();

            UpdateCameras();
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnEscapePressed();
            }

            if (Input.GetKeyDown(Config.Global.toggleFPSCameraHotkey))
            {
                OnToggleCameraHotkeyPressed();
            }

            var pos = gameObject.transform.position;

            float terrainY = TerrainManager.instance.SampleDetailHeight(gameObject.transform.position);
            float waterY = TerrainManager.instance.WaterLevel(new Vector2(gameObject.transform.position.x, gameObject.transform.position.z));
            terrainY = Mathf.Max(terrainY, waterY);

            if (Config.Global.animateTransitions && inModeTransition)
            {
                transitionT += Time.deltaTime * Config.Global.animationSpeed;

                gameObject.transform.position = Vector3.Slerp(transitionStartPosition, transitionTargetPosition, transitionT);
                gameObject.transform.rotation = Quaternion.Slerp(transitionStartOrientation, transitionTargetOrientation, transitionT);

                if (transitionT >= 1.0f)
                {
                    inModeTransition = false;

                    if (!fpsModeEnabled)
                    {
                        Instance.controller.enabled = true;
                    }
                }
            }
            else if (fpsModeEnabled)
            {
                if (Config.Global.snapToGround)
                {
                    Segment3 ray = new Segment3(gameObject.transform.position + new Vector3(0f, 1.5f, 0f), gameObject.transform.position + new Vector3(0f, -1000f, 0f));

                    Vector3 hitPos;
                    ushort nodeIndex;
                    ushort segmentIndex;
                    Vector3 hitPos2;
                    if (NetManager.instance.RayCast(null, ray, 0f, false, ItemClass.Service.Road, ItemClass.Service.PublicTransport, ItemClass.SubService.None, ItemClass.SubService.None, ItemClass.Layer.Default, ItemClass.Layer.None, NetNode.Flags.None, NetSegment.Flags.None, out hitPos, out nodeIndex, out segmentIndex)
                        | NetManager.instance.RayCast(null, ray, 0f, false, ItemClass.Service.Beautification, ItemClass.Service.Water, ItemClass.SubService.None, ItemClass.SubService.None, ItemClass.Layer.Default, ItemClass.Layer.None, NetNode.Flags.None, NetSegment.Flags.None, out hitPos2, out nodeIndex, out segmentIndex))
                    {
                        terrainY = Mathf.Max(terrainY, Mathf.Max(hitPos.y, hitPos2.y));
                    }

                    gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, new Vector3(pos.x, terrainY + Config.Global.groundOffset, pos.z), 0.9f);
                }

                float speedFactor = 1.0f;
                if (Config.Global.limitSpeedGround)
                {
                    speedFactor *= Mathf.Sqrt(terrainY);
                    speedFactor = Mathf.Clamp(speedFactor, 1.0f, 256.0f);
                }

                speedFactor = UserInput(speedFactor);

                if (effect != null)
                {
                    effect.enabled = Config.Global.enableDOF;
                    if (Config.Global.enableDOF)
                    {
                        effect.focalLength = focalDistanceList[focalIndex];
                    }
                }
                float height = Instance.camera.transform.position.y - TerrainManager.instance.SampleDetailHeight(Instance.camera.transform.position);
                RenderManager.instance.CameraHeight = height;

            }
            else
            {
                mainCameraPosition = camera.transform.position;
                mainCameraOrientation = camera.transform.rotation;
            }

            if (Config.Global.preventClipGround)
            {
                Segment3 ray = new Segment3(gameObject.transform.position + new Vector3(0f, 1.5f, 0f), gameObject.transform.position + new Vector3(0f, -1000f, 0f));

                Vector3 hitPos;
                ushort nodeIndex;
                ushort segmentIndex;
                Vector3 hitPos2;

                if (NetManager.instance.RayCast(null, ray, 0f, false, ItemClass.Service.Road, ItemClass.Service.PublicTransport, ItemClass.SubService.None, ItemClass.SubService.None, ItemClass.Layer.Default, ItemClass.Layer.None, NetNode.Flags.None, NetSegment.Flags.None, out hitPos, out nodeIndex, out segmentIndex)
                    | NetManager.instance.RayCast(null, ray, 0f, false, ItemClass.Service.Beautification, ItemClass.Service.Water, ItemClass.SubService.None, ItemClass.SubService.None, ItemClass.Layer.Default, ItemClass.Layer.None, NetNode.Flags.None, NetSegment.Flags.None, out hitPos2, out nodeIndex, out segmentIndex))
                {
                    terrainY = Mathf.Max(terrainY, Mathf.Max(hitPos.y, hitPos2.y));
                }

                if (transform.position.y < terrainY + Config.Global.groundOffset)
                {
                    transform.position = new Vector3(transform.position.x, terrainY + Config.Global.groundOffset, transform.position.z);
                }
            }
        }

        private float UserInput(float speedFactor)
        {
            if (Input.GetKey(Config.Global.goFasterHotKey))
            {
                speedFactor *= Config.Global.goFasterSpeedMultiplier;
            }

            if (Input.GetKey(Config.Global.cameraMoveForward))
            {
                gameObject.transform.position += gameObject.transform.forward * Config.Global.cameraMoveSpeed * speedFactor * Time.deltaTime;
            }
            else if (Input.GetKey(Config.Global.cameraMoveBackward))
            {
                gameObject.transform.position -= gameObject.transform.forward * Config.Global.cameraMoveSpeed * speedFactor * Time.deltaTime;
            }

            if (Input.GetKey(Config.Global.cameraMoveLeft))
            {
                gameObject.transform.position -= gameObject.transform.right * Config.Global.cameraMoveSpeed * speedFactor * Time.deltaTime;
            }
            else if (Input.GetKey(Config.Global.cameraMoveRight))
            {
                gameObject.transform.position += gameObject.transform.right * Config.Global.cameraMoveSpeed * speedFactor * Time.deltaTime;
            }

            if (Input.GetKey(Config.Global.cameraZoomAway))
            {
                gameObject.transform.position -= gameObject.transform.up * Config.Global.cameraMoveSpeed * speedFactor * Time.deltaTime;
            }
            else if (Input.GetKey(Config.Global.cameraZoomCloser))
            {
                gameObject.transform.position += gameObject.transform.up * Config.Global.cameraMoveSpeed * speedFactor * Time.deltaTime;
            }

            if (Input.GetKey(Config.Global.showMouseHotkey))
            {
                Cursor.visible = true;
            }
            else
            {
                float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * Config.Global.cameraRotationSensitivity;
                rotationY += Input.GetAxis("Mouse Y") * Config.Global.cameraRotationSensitivity * (Config.Global.invertYAxis ? -1.0f : 1.0f);
                transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
                Cursor.visible = false;
            }

            var d = Input.GetAxis("Mouse ScrollWheel");
            if (d > 0f && focalIndex >= 0)
            {
                focalIndex--;
            }
            else if (d < 0f && focalIndex < focalDistanceList.Count)
            {
                focalIndex++;
            }

            return speedFactor;
        }
    }

}
