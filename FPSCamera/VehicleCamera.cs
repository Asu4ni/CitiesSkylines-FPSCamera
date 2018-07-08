using System;
using ColossalFramework;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using FPSCamera.Utils;

namespace FPSCamera
{

    public class VehicleCamera : MonoBehaviour, InstanceCamera
    {
        private ushort followInstance;

        public bool following = false;

        private CameraController cameraController;
        public Camera camera;
        public DepthOfField effect;

        private VehicleManager vManager;
        private bool isReversed;
        public Vector3 userOffset = Vector3.zero;
        
        void Awake()
        {
            cameraController = GetComponent<CameraController>();
            camera = GetComponent<Camera>();
            effect = cameraController.GetComponent<DepthOfField>();
            vManager = VehicleManager.instance;
        }

        void Update()
        {
            if (following)
            {

                if ((vManager.m_vehicles.m_buffer[followInstance].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) != Vehicle.Flags.Created)
                {
                    StopFollowing();
                    return;
                }

                if ((vManager.m_vehicles.m_buffer[followInstance].m_flags & Vehicle.Flags.Spawned) == 0)
                {
                    StopFollowing();
                    return;
                }

                if(FPSCamera.instance.config.alwaysFrontVehicle)
                {
                    bool currentReversedStatus = AIUtils.GetReversedStatus(vManager, followInstance);
                    if (currentReversedStatus != isReversed)
                    {
                        followInstance = currentReversedStatus ?
                            VehicleManager.instance.m_vehicles.m_buffer[followInstance].GetLastVehicle(followInstance) :
                            VehicleManager.instance.m_vehicles.m_buffer[followInstance].GetFirstVehicle(followInstance);
                    }
                    isReversed = currentReversedStatus;
                }

                Vehicle v = vManager.m_vehicles.m_buffer[followInstance];

                Vector3 position = Vector3.zero;
                Quaternion orientation = Quaternion.identity;
                v.GetSmoothPosition((ushort)followInstance, out position, out orientation);
                Vector3 forward = orientation * Vector3.forward;
                Vector3 up = orientation * Vector3.up;

                Vector3 vehicleOffset = forward * v.Info.m_attachOffsetFront;
                if (v.m_leadingVehicle != 0 && v.m_trailingVehicle != 0)
                {
                    vehicleOffset += up * 3.0f;
                    vehicleOffset -= forward * 2.0f;
                }

                var pos = GetOffset(position, forward, up) + vehicleOffset +
                          forward * FPSCamera.instance.config.vehicleCameraOffsetX +
                          up * FPSCamera.instance.config.vehicleCameraOffsetY;
                camera.transform.position = pos + this.userOffset;
                Vector3 lookAt = pos + (orientation * Vector3.forward) * 1.0f;

                var currentOrientation = camera.transform.rotation;
                camera.transform.LookAt(lookAt, Vector3.up);
                camera.transform.rotation = Quaternion.Slerp(currentOrientation, camera.transform.rotation,
                    Time.deltaTime * 5f);

                float height = camera.transform.position.y - TerrainManager.instance.SampleDetailHeight(camera.transform.position);
                cameraController.m_targetPosition = camera.transform.position;

                if (effect)
                {
                    effect.enabled = FPSCamera.instance.config.enableDOF;
                }
                if (FPSCamera.instance.config.displaySpeed)
                {
                    GetInstanceSpeed();
                }
            }
        }

        public void GetInstanceSpeed()
        {
            Vehicle v = vManager.m_vehicles.m_buffer[followInstance];
            Vector3 velocity = v.GetSmoothVelocity(followInstance);
            FPSCameraSpeedUI.Instance.speed = velocity.magnitude;
        }

        public void SetFollowInstance(uint instance)
        {
            FPSCamera.instance.SetMode(false);
            
            followInstance = (ushort)instance;
            isReversed = AIUtils.GetReversedStatus(vManager, followInstance);
            if (FPSCamera.instance.config.alwaysFrontVehicle)
            {
                followInstance = isReversed ?
                    VehicleManager.instance.m_vehicles.m_buffer[followInstance].GetLastVehicle(followInstance) :
                    VehicleManager.instance.m_vehicles.m_buffer[followInstance].GetFirstVehicle(followInstance);
            }
            following = true;

            CameraUtils.SetCamera(cameraController, camera);
            if (FPSCamera.instance.config.displaySpeed)
            {
                FPSCameraSpeedUI.Instance.enabled = true;
            }
            FPSCamera.onCameraModeChanged(true);
            userOffset = Vector3.zero;

        }

        public void StopFollowing()
        {
            followInstance = 0;
            following = false;
            FPSCameraSpeedUI.Instance.enabled = false;
            CameraUtils.StopCamera(cameraController, camera);
            FPSCamera.onCameraModeChanged(false);
        }

        public Vector3 GetOffset(Vector3 position, Vector3 forward, Vector3 up)
        {
            Vector3 retVal = position +
                            forward * CameraUtils.CAMERAOFFSETFORWARD +
                            up * CameraUtils.CAMERAOFFSETUP;

            return retVal;
        }


    }

}
