using ColossalFramework;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace FPSCamera
{

    public class VehicleCamera : MonoBehaviour
    {
        private ushort followInstance;
        public bool following = false;

        private CameraController cameraController;
        public Camera camera;
        public DepthOfField effect;

        private VehicleManager vManager;

        private float cameraOffsetForward = 2.75f;
        private float cameraOffsetUp = 1.5f;

        private Vehicle currentVehicle;

        public Vector3 userOffset = Vector3.zero;
        private Vector3 GetCameraOffsetForVehicleType(Vehicle v, Vector3 forward, Vector3 up)
        {
            currentVehicle = v;

            var offset = forward * v.Info.m_attachOffsetFront +
                         forward * cameraOffsetForward +
                         up * cameraOffsetUp;

            if (v.m_leadingVehicle != 0)
            {
                offset += up*3.0f;
                offset -= forward*2.0f;
            }
            else if(v.Info.name == "Train Engine")
            {
                offset += forward * 2.0f;
            }


            return offset;
        }

        public void SetFollowInstance(ushort instance)
        {
            FPSCamera.instance.SetMode(false);
            followInstance = instance;
            following = true;
            camera.nearClipPlane = 1f;
            cameraController.enabled = false;
            cameraController.m_maxDistance = 50f;

            //Set to 1/4 minimum vanilla value( ground level )
            effect.focalLength = 10;
            //A bit bigger, to reduce blur some more
            effect.focalSize = 0.8f;

            camera.fieldOfView = FPSCamera.instance.config.fieldOfView;
            FPSCamera.onCameraModeChanged(true);
            userOffset = Vector3.zero;
        }

        public void StopFollowing()
        {
            following = false;
            cameraController.enabled = true;
            camera.nearClipPlane = 1.0f;
            cameraController.m_maxDistance = 4000f;

            FPSCamera.onCameraModeChanged(false);
       
            camera.fieldOfView = FPSCamera.instance.originalFieldOfView;
            if (FPSCamera.instance.hideUIComponent != null && FPSCamera.instance.config.integrateHideUI)
            {
                FPSCamera.instance.hideUIComponent.SendMessage("Show");
            }
            FPSCamera.instance.ui.Hide();

        }

        void Awake()
        {
            cameraController = GetComponent<CameraController>();
            camera = GetComponent<Camera>();
            vManager = VehicleManager.instance;
            effect = cameraController.GetComponent<DepthOfField>();
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

                var pos = position + GetCameraOffsetForVehicleType(v, forward, up) + forward * FPSCamera.instance.config.vehicleCameraOffsetX + up * FPSCamera.instance.config.vehicleCameraOffsetY;
                camera.transform.position = pos + userOffset;
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

    }

}
