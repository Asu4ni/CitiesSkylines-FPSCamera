using System;
using ColossalFramework;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

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
        public Vector3 userOffset = Vector3.zero;

        void Awake()
        {
            cameraController = GetComponent<CameraController>();
            camera = GetComponent<Camera>();
            vManager = VehicleManager.instance;
        }

        void Update()
        {
            if(following)
            {
                var i = followInstance;

                if ((vManager.m_vehicles.m_buffer[i].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) != Vehicle.Flags.Created)
                {
                    StopFollowing();
                    return;
                }

                if ((vManager.m_vehicles.m_buffer[i].m_flags & Vehicle.Flags.Spawned) == 0)
                {
                    StopFollowing();
                    return;
                }

                Vehicle v = vManager.m_vehicles.m_buffer[i];
                Vector3 position = Vector3.zero;
                Quaternion orientation = Quaternion.identity;
                v.GetSmoothPosition((ushort)i, out position, out orientation);
                Vector3 forward = orientation * Vector3.forward;
                Vector3 up = orientation * Vector3.up;

                Vector3 userOffset = forward * v.Info.m_attachOffsetFront;
                if (v.m_leadingVehicle != 0)
                {
                    userOffset += up * 3.0f;
                    userOffset -= forward * 2.0f;
                }
                else if (v.Info.name == "Train Engine")
                {
                    userOffset += forward * 2.0f;
                }

                var pos = GetOffset(position, forward, up, userOffset) + 
                          forward * FPSCamera.instance.config.vehicleCameraOffsetX +
                          up * FPSCamera.instance.config.vehicleCameraOffsetY;
                camera.transform.position = pos;
                Vector3 lookAt = pos + (orientation * Vector3.forward) * 1.0f;

                var currentOrientation = camera.transform.rotation;
                camera.transform.LookAt(lookAt, Vector3.up);
                camera.transform.rotation = Quaternion.Slerp(currentOrientation, camera.transform.rotation,
                    Time.deltaTime * 3.0f);

                float height = camera.transform.position.y - TerrainManager.instance.SampleDetailHeight(camera.transform.position);
                cameraController.m_currentPosition = camera.transform.position;
                effect.enabled = FPSCamera.instance.config.enableDOF;
            }
        }

        public void SetFollowInstance(uint instance)
        {
            FPSCamera.instance.SetMode(false);
            
            followInstance = (ushort)instance;
            following = true;

            CameraUtils.setCamera(cameraController, camera);

            FPSCamera.onCameraModeChanged(true);
            userOffset = Vector3.zero;
        }

        public void StopFollowing()
        {
            followInstance = 0;
            following = false;

            CameraUtils.stopCamera(cameraController, camera);
            FPSCamera.onCameraModeChanged(false);
        }

        public Vector3 GetOffset(Vector3 position, Vector3 forward, Vector3 up, Vector3 userOffset)
        {
            Vector3 retVal = position + userOffset +
                            forward * CameraUtils.CAMERAOFFSETFORWARD +
                            up * CameraUtils.CAMERAOFFSETUP;

            return retVal;
        }

    }

}
