using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace FPSCamera
{

    public class CitizenCamera : MonoBehaviour
    {
        private uint followInstance;
        public bool following = false;
        public bool inVehicle = false;

        private CameraController cameraController;
        public Camera camera;
        public DepthOfField effect;

        private CitizenManager cManager;

        private float cameraOffsetForward = 0.2f;
        private float cameraOffsetUp = 1.5f;

        public Vector3 userOffset = Vector3.zero;

        public void SetFollowInstance(uint instance)
        {
            FPSCamera.instance.SetMode(false);
            followInstance = instance;
            following = true;
            camera.nearClipPlane = 0.75f;
            cameraController.enabled = false;
            cameraController.m_maxDistance = 50f;
            cameraController.m_currentSize = 5;
            camera.fieldOfView = FPSCamera.instance.config.fieldOfView;
            effect.enabled = FPSCamera.instance.config.enableDOF;
            //Set to 1/4 minimum vanilla value( ground level )
            effect.focalLength = 10;
            //A bit bigger, to reduce blur some more
            effect.focalSize = 0.8f;

            FPSCamera.onCameraModeChanged(true);
        }

        public void StopFollowing()
        {
            following = false;
            cameraController.enabled = true;
            camera.nearClipPlane = 1.0f;
            cameraController.m_maxDistance = 4000f;
            FPSCamera.onCameraModeChanged(false);
            userOffset = Vector3.zero;
            camera.fieldOfView = FPSCamera.instance.originalFieldOfView;

            if (FPSCamera.instance.hideUIComponent != null && FPSCamera.instance.config.integrateHideUI)
            {
                FPSCamera.instance.hideUIComponent.SendMessage("Show");
            }
        }

        void Awake()
        {
            cameraController = GetComponent<CameraController>();
            camera = GetComponent<Camera>();
            cManager = CitizenManager.instance;
            effect = cameraController.GetComponent<DepthOfField>();

        }

        void Update()
        {
            if (following)
            {
                var citizen = cManager.m_citizens.m_buffer[followInstance];
                var i = citizen.m_instance;

                var flags = cManager.m_instances.m_buffer[i].m_flags;
                if ((flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted)) != CitizenInstance.Flags.Created)
                {
                    StopFollowing();
                    return;
                }

                if ((flags & CitizenInstance.Flags.EnteringVehicle) != 0)
                {
                    StopFollowing();
                    return;
                }

                CitizenInstance c = cManager.m_instances.m_buffer[i];
                Vector3 position = Vector3.zero;
                Quaternion orientation = Quaternion.identity;
                c.GetSmoothPosition((ushort)i, out position, out orientation);

                Vector3 forward = orientation * Vector3.forward;
                Vector3 up = orientation * Vector3.up;

                var pos = position +
                          forward*cameraOffsetForward +
                          up*cameraOffsetUp;
                camera.transform.position = pos +
                                            userOffset;
                Vector3 lookAt = pos + (orientation * Vector3.forward) * 1.0f;
                var currentOrientation = camera.transform.rotation;
                camera.transform.LookAt(lookAt, Vector3.up);
                camera.transform.rotation = Quaternion.Slerp(currentOrientation, camera.transform.rotation,
                    Time.deltaTime*2.0f);

                float height = camera.transform.position.y - TerrainManager.instance.SampleDetailHeight(camera.transform.position);
                cameraController.m_currentPosition = camera.transform.position;

                effect.enabled = FPSCamera.instance.config.enableDOF;

            }
        }

    }

}
