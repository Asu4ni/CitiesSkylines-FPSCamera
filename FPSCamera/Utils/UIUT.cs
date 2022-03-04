using UnityEngine;

namespace FPSCamMod
{
    internal static class UIUT
    {
        // TODO: investigate

        public static ColossalFramework.UI.UIView UIView
                        => Object.FindObjectOfType<ColossalFramework.UI.UIView>();
        public static void HideUI()
        {
            var cameraController = GameObject.FindObjectOfType<CameraController>();
            var camera = cameraController.gameObject.GetComponent<Camera>();
            camera.GetComponent<OverlayEffect>().enabled = false;
            bool cachedEnabled = cameraController.enabled;
            var cameras = GameObject.FindObjectsOfType<Camera>();
            foreach (var cam in cameras)
            {
                if (cam.name == "UIView")
                {
                    cam.enabled = false;
                    break;
                }
            }
            camera.rect = new Rect(0.0f, 0.0f, 1f, 1f);

            // For some reason, the cameracontroller's not picking this up before it's disabled...
            cameraController.enabled = true;
            cameraController.m_freeCamera = true;
            cameraController.enabled = cachedEnabled;
        }
        public static void ShowUI()
        {
            var cameraController = GameObject.FindObjectOfType<CameraController>();
            var camera = cameraController.gameObject.GetComponent<Camera>();
            camera.GetComponent<OverlayEffect>().enabled = true;
            bool cachedEnabled = cameraController.enabled;

            var cameras = GameObject.FindObjectsOfType<Camera>();
            foreach (var cam in cameras)
            {
                if (cam.name == "UIView")
                {
                    cam.enabled = true;
                    break;
                }
            }

            // For some reason, the cameracontroller's not picking this up before it's disabled...
            cameraController.enabled = true;
            cameraController.m_freeCamera = false;
            cameraController.enabled = cachedEnabled;

            camera.rect = new Rect(0.0f, 0.105f, 1f, 0.895f);
        }
    }
}
