using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace FPSCamera
{
    public abstract class BaseCamera : MonoBehaviour
    {
        protected const string unknownStr = "(unknown)";

        protected UUID followedID;

        protected CameraController cameraController;
        public Camera camera;
        public DepthOfField depthOfField;
        public Vector3 userOffset = Vector3.zero;   // TODO: move to FPSCamera

        protected BaseCamera(GameObject parentObject)
        {
            parentObject.AddComponent(GetType());
        }

        public void Awake()
        {
            enabled = false;
            cameraController = GetComponent<CameraController>();
            camera = GetComponent<Camera>();
            if (!cameraController) Log.Err("missing component: CameraController");
            if (!camera) Log.Err("missing component: Camera");
            depthOfField = cameraController.GetComponent<DepthOfField>();
        }

        public void SetInstanceToFollow(UUID followed)
        {
            enabled = true;
            followedID = GetIntentedInstance(followed);
            FPSCamera.Instance.SetMode(false);
            userOffset = Vector3.zero;
            CameraUT.SetCamera(cameraController, camera);
            FPSCameraSpeedUI.Instance.enabled = Config.Global.displaySpeed;            
            FPSCamera.Instance.onCameraModeChanged(true);
            SetInstanceToFollowPost();
        }
        protected virtual void SetInstanceToFollowPost() { }
        protected virtual UUID GetIntentedInstance(UUID id) => id;

        public void StopFollowing()
        {
            StopFollowingPre();
            FPSCameraSpeedUI.Instance.enabled = false;
            CameraUT.StopCamera(cameraController, camera);
            FPSCamera.Instance.onCameraModeChanged(false);
            enabled = false;
        }

        protected void UpdateUI()
        {
            FPSCameraSpeedUI.Instance.speed = GetVelocity().magnitude;
            FPSCameraSpeedUI.Instance.destinationName = GetDestinationStr();
            FPSCameraSpeedUI.Instance.passengersOrStreet = GetDisplayInfoStr();
        }
        protected abstract Vector3 GetVelocity();
        protected abstract string GetDestinationStr();
        protected abstract string GetDisplayInfoStr();

        protected virtual void StopFollowingPre() { }

        // TODO: change to return (position, lookAt)
        public void LateUpdate()
        {
            if (UpdateCam())
            {
                cameraController.m_targetPosition = transform.position;

                // TODO: move to FPSCamera
                if (depthOfField) depthOfField.enabled = Config.Global.enableDOF;
                if (Config.Global.displaySpeed) UpdateUI();
            }
        }
        protected abstract bool UpdateCam();    // false: stop/ignore/reset update
    }
}