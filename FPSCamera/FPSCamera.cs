using ColossalFramework.Math;
using ICities;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace FPSCamera
{
    public class FPSCamera : MonoBehaviour
    {

        public delegate void OnCameraModeChanged(bool state);

        public static OnCameraModeChanged onCameraModeChanged;

        public delegate void OnUpdate();

        public static OnUpdate onUpdate;

        public static bool editorMode = false;

        public static bool ipt2Enabled = false;

        public static void Initialize(LoadMode mode)
        {
            var controller = GameObject.FindObjectOfType<CameraController>();
            instance = controller.gameObject.AddComponent<FPSCamera>();

            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
            {
                instance.gameObject.AddComponent<GamePanelExtender>();
                instance.vehicleCamera = instance.gameObject.AddComponent<VehicleCamera>();
                instance.citizenCamera = instance.gameObject.AddComponent<CitizenCamera>();
                instance.citizenCamera.vehicleCamera = instance.vehicleCamera;
                instance.vehicleCamera.enabled = false;
                instance.citizenCamera.enabled = false;

                editorMode = false;

                ipt2Enabled = Util.FindIPT2();
            }
            else
            {
                editorMode = true;
            }
        }

        public static void Deinitialize()
        {
            Destroy(instance);
        }

        public static FPSCamera instance;

        public Configuration config;

        public bool fpsModeEnabled = false;
        private CameraController controller;
        public Camera camera;
        float rotationY = 0f;

        public DepthOfField effect;
        public TiltShiftEffect legacyEffect;

        private Vector3 mainCameraPosition;
        private Quaternion mainCameraOrientation;

        public bool checkedForHideUI = false;

        public VehicleCamera vehicleCamera;
        public CitizenCamera citizenCamera;
        
        public bool cityWalkthroughMode = false;
        private float cityWalkthroughNextChangeTimer = 0.0f;

        public float originalFieldOfView = 0.0f;

        private static readonly List<int> focalDistanceList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 25, 35, 45, 50, 75, 100, 150, 200, 500, 750, 1000, 2000, 5000, 10000 };
        private int focalIndex = 9;

        private bool inModeTransition = false;
        private Vector3 transitionTargetPosition = Vector3.zero;
        private Quaternion transitionTargetOrientation = Quaternion.identity;
        private Vector3 transitionStartPosition = Vector3.zero;
        private Quaternion transitionStartOrientation = Quaternion.identity;
        private float transitionT = 0.0f;
        private float existingTime = 0.0f;

        public FPSCameraUI ui;
        public FPSCameraSpeedUI speedUi;

        public Mesh mesh;

        void Start()
        {
            controller = FindObjectOfType<CameraController>();
            camera = controller.GetComponent<Camera>();
            originalFieldOfView = camera.fieldOfView;
            effect = controller.GetComponent<DepthOfField>();
            legacyEffect = controller.GetComponent<TiltShiftEffect>();

            config = Configuration.Deserialize(Configuration.configPath) ?? new Configuration();

            SaveConfig();

            mainCameraPosition = gameObject.transform.position;
            mainCameraOrientation = gameObject.transform.rotation;

            mainCameraPosition = gameObject.transform.position;
            mainCameraOrientation = gameObject.transform.rotation;
            rotationY = -instance.transform.localEulerAngles.x;

            ui = FPSCameraUI.Instance;
            speedUi = FPSCameraSpeedUI.Instance;
            speedUi.enabled = false;
            
        }

        public void SaveConfig()
        {
            Configuration.Serialize(Configuration.configPath, config);
        }

        public void SetFieldOfView(float fov)
        {
            config.fieldOfView = fov;
            SaveConfig();
            if (fpsModeEnabled)
            {
                camera.fieldOfView = fov;
            }
        }

        public void EnterWalkthroughMode()
        {
            cityWalkthroughMode = true;
            cityWalkthroughNextChangeTimer = config.walkthroughModeTimer;

            if ( config.integrateHideUI)
            {
                UIHider.Hide();
            }

            WalkthroughModeSwitchTarget();
            FPSCameraUI.Instance.Hide();
        }

        public void ResetConfig()
        {
            config = new Configuration();
            SaveConfig();

            Destroy(FPSCameraUI.instance);
            FPSCameraUI.instance = null;
            ui = FPSCameraUI.Instance;
            ui.Show();
        }

        public void SetMode(bool fpsMode)
        {
            instance.fpsModeEnabled = fpsMode;
            FPSCameraUI.instance.Hide();

            if (config.integrateHideUI)
            {
                if (instance.fpsModeEnabled)
                {
                    UIHider.Hide();
                }
                else
                {
                    UIHider.Show();
                }
            }

            if (instance.fpsModeEnabled)
            {
                camera.fieldOfView = config.fieldOfView;
                camera.nearClipPlane = 1.0f;

                instance.controller.m_maxDistance = 50f;
                instance.controller.m_currentSize = 5;
                instance.controller.m_currentHeight =2f;
                instance.controller.enabled = false;
                if (effect)
                {
                    effect.enabled = config.enableDOF;
                }
                if( legacyEffect)
                {
                    legacyEffect.enabled = false;
                }

                Cursor.visible = false;
                instance.rotationY = -instance.transform.localEulerAngles.x;
            }
            else
            {
                instance.controller.m_maxDistance = 10000f;

                if (!config.animateTransitions)
                {
                    instance.controller.enabled = true;
                }

                camera.fieldOfView = originalFieldOfView;
                Cursor.visible = true;
                effect.nearBlur = true;
            }
            
            onCameraModeChanged?.Invoke(fpsMode);
        }

        public static KeyCode GetToggleUIKey()
        {
            return instance.config.toggleFPSCameraHotkey;
        }

        public static bool IsEnabled()
        {
            return instance.fpsModeEnabled;
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
                if (citizenCamera.following)
                {
                    citizenCamera.StopFollowing();
                }

                var vehicle = GetRandomVehicle();
                if (vehicle != 0)
                {
                    vehicleCamera.SetFollowInstance(vehicle);
                }
            }
            else
            {
                if (vehicleCamera.following)
                {
                    vehicleCamera.StopFollowing();
                }

                var citizen = GetRandomCitizenInstance();
                if (citizen != 0)
                {
                    citizenCamera.SetFollowInstance(citizen);
                }
            }
        }

        void UpdateCityWalkthrough()
        {
            if (cityWalkthroughMode && !config.walkthroughModeManual)
            {
                cityWalkthroughNextChangeTimer -= Time.deltaTime;
                if (cityWalkthroughNextChangeTimer <= 0.0f || !(citizenCamera.following || vehicleCamera.following))
                {
                    cityWalkthroughNextChangeTimer = config.walkthroughModeTimer;
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
            if (vehicleCamera != null && vehicleCamera.following && config.allowUserOffsetInVehicleCitizenMode)
            {
                if (Input.GetKey(config.cameraMoveForward))
                {
                    vehicleCamera.userOffset += gameObject.transform.forward * config.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }
                else if (Input.GetKey(config.cameraMoveBackward))
                {
                    vehicleCamera.userOffset -= gameObject.transform.forward * config.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }

                if (Input.GetKey(config.cameraMoveLeft))
                {
                    vehicleCamera.userOffset -= gameObject.transform.right * config.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }
                else if (Input.GetKey(config.cameraMoveRight))
                {
                    vehicleCamera.userOffset += gameObject.transform.right * config.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }

                if (Input.GetKey(config.cameraZoomAway))
                {
                    vehicleCamera.userOffset -= gameObject.transform.up * config.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }
                else if (Input.GetKey(config.cameraZoomCloser))
                {
                    vehicleCamera.userOffset += gameObject.transform.up * config.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }

                if(Input.GetKeyDown(config.cameraRotateLeft))
                {
                    vehicleCamera.cameraRotationOffset -= 45;
                    if(vehicleCamera.cameraRotationOffset <= -360)
                    {
                        vehicleCamera.cameraRotationOffset = 0;
                    }
                }
                else if(Input.GetKeyDown(config.cameraRotateRight))
                {
                    vehicleCamera.cameraRotationOffset += 45;
                    if (vehicleCamera.cameraRotationOffset >= 360)
                    {
                        vehicleCamera.cameraRotationOffset = 0;
                    }
                }
            }

            if (citizenCamera != null && citizenCamera.following && config.allowUserOffsetInVehicleCitizenMode)
            {
                if (Input.GetKey(config.cameraMoveForward))
                {
                    citizenCamera.userOffset += gameObject.transform.forward * config.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }
                else if (Input.GetKey(config.cameraMoveBackward))
                {
                    citizenCamera.userOffset -= gameObject.transform.forward * config.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }

                if (Input.GetKey(config.cameraMoveLeft))
                {
                    citizenCamera.userOffset -= gameObject.transform.right * config.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }
                else if (Input.GetKey(config.cameraMoveRight))
                {
                    citizenCamera.userOffset += gameObject.transform.right * config.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }

                if (Input.GetKey(config.cameraZoomAway))
                {
                    citizenCamera.userOffset -= gameObject.transform.up * config.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }
                else if (Input.GetKey(config.cameraZoomCloser))
                {
                    citizenCamera.userOffset += gameObject.transform.up * config.cameraMoveSpeed * 0.25f * Time.deltaTime;
                }
            }
        }

        void OnEscapePressed()
        {

            if (cityWalkthroughMode)
            {

                cityWalkthroughMode = false;
                if(config.integrateHideUI)
                {
                    UIHider.Hide();
                }

                if (vehicleCamera != null && vehicleCamera.following)
                {
                    vehicleCamera.StopFollowing();
                }

                if (citizenCamera != null && citizenCamera.following)
                {
                    citizenCamera.StopFollowing();
                }
                
            }
            else
            {
                if (vehicleCamera != null && vehicleCamera.following)
                {
                    if (config.integrateHideUI)
                    {
                        UIHider.Hide();
                    }
                    vehicleCamera.StopFollowing();
                }
                if (citizenCamera != null && citizenCamera.following)
                {
                    if (config.integrateHideUI)
                    {
                        UIHider.Hide();
                    }
                    citizenCamera.StopFollowing();
                }
                if (fpsModeEnabled)
                {
                    if (config.animateTransitions && fpsModeEnabled)
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
            
            ui.Hide();
        }

        void OnToggleCameraHotkeyPressed()
        {
            if (cityWalkthroughMode)
            {
                if (config.integrateHideUI)
                {
                    UIHider.Show();
                }

                cityWalkthroughMode = false;
                if (vehicleCamera.following)
                {
                    vehicleCamera.StopFollowing();
                }
                if (citizenCamera.following)
                {
                    citizenCamera.StopFollowing();
                }


            }
            else if (vehicleCamera != null && vehicleCamera.following)
            {
                vehicleCamera.StopFollowing();
            }
            else if (citizenCamera != null && citizenCamera.following)
            {
                citizenCamera.StopFollowing();
            }
            else
            {
                if (config.animateTransitions && fpsModeEnabled)
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
            ui.Hide();
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

            if (Input.GetKeyDown(config.toggleFPSCameraHotkey))
            {
                OnToggleCameraHotkeyPressed();
            }

            var pos = gameObject.transform.position;

            float terrainY = TerrainManager.instance.SampleDetailHeight(gameObject.transform.position);
            float waterY = TerrainManager.instance.WaterLevel(new Vector2(gameObject.transform.position.x, gameObject.transform.position.z));
            terrainY = Mathf.Max(terrainY, waterY);

            if (config.animateTransitions && inModeTransition)
            {
                transitionT += Time.deltaTime * config.animationSpeed;

                gameObject.transform.position = Vector3.Slerp(transitionStartPosition, transitionTargetPosition, transitionT);
                gameObject.transform.rotation = Quaternion.Slerp(transitionStartOrientation, transitionTargetOrientation, transitionT);

                if (transitionT >= 1.0f)
                {
                    inModeTransition = false;

                    if (!fpsModeEnabled)
                    {
                        instance.controller.enabled = true;
                    }
                }
            }
            else if (fpsModeEnabled)
            {
                if (config.snapToGround)
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

                    gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, new Vector3(pos.x, terrainY + config.groundOffset, pos.z), 0.9f);
                }

                float speedFactor = 1.0f;
                if (config.limitSpeedGround)
                {
                    speedFactor *= Mathf.Sqrt(terrainY);
                    speedFactor = Mathf.Clamp(speedFactor, 1.0f, 256.0f);
                }

                speedFactor = UserInput(speedFactor);

                if (effect != null)
                {
                    effect.enabled = config.enableDOF;
                    if (config.enableDOF)
                    {
                        effect.focalLength = focalDistanceList[focalIndex];
                    }
                }
                float height = instance.camera.transform.position.y - TerrainManager.instance.SampleDetailHeight(instance.camera.transform.position);
                RenderManager.instance.CameraHeight = height;

            }
            else
            {
                mainCameraPosition = camera.transform.position;
                mainCameraOrientation = camera.transform.rotation;
            }

            if (config.preventClipGround)
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

                if (transform.position.y < terrainY + config.groundOffset)
                {
                    transform.position = new Vector3(transform.position.x, terrainY + config.groundOffset, transform.position.z);
                }
            }
        }

        private float UserInput(float speedFactor)
        {
            if (Input.GetKey(config.goFasterHotKey))
            {
                speedFactor *= config.goFasterSpeedMultiplier;
            }

            if (Input.GetKey(config.cameraMoveForward))
            {
                gameObject.transform.position += gameObject.transform.forward * config.cameraMoveSpeed * speedFactor * Time.deltaTime;
            }
            else if (Input.GetKey(config.cameraMoveBackward))
            {
                gameObject.transform.position -= gameObject.transform.forward * config.cameraMoveSpeed * speedFactor * Time.deltaTime;
            }

            if (Input.GetKey(config.cameraMoveLeft))
            {
                gameObject.transform.position -= gameObject.transform.right * config.cameraMoveSpeed * speedFactor * Time.deltaTime;
            }
            else if (Input.GetKey(config.cameraMoveRight))
            {
                gameObject.transform.position += gameObject.transform.right * config.cameraMoveSpeed * speedFactor * Time.deltaTime;
            }

            if (Input.GetKey(config.cameraZoomAway))
            {
                gameObject.transform.position -= gameObject.transform.up * config.cameraMoveSpeed * speedFactor * Time.deltaTime;
            }
            else if (Input.GetKey(config.cameraZoomCloser))
            {
                gameObject.transform.position += gameObject.transform.up * config.cameraMoveSpeed * speedFactor * Time.deltaTime;
            }

            if (Input.GetKey(config.showMouseHotkey))
            {
                Cursor.visible = true;
            }
            else
            {
                float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * config.cameraRotationSensitivity;
                rotationY += Input.GetAxis("Mouse Y") * config.cameraRotationSensitivity * (config.invertYAxis ? -1.0f : 1.0f);
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
