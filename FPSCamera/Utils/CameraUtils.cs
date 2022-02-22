using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace FPSCamera
{
    class CameraUtils
    {
        public const float CameraOffsetForward = 2.75f;
        public const float CameraOffsetUp = 1.5f;
        
        // set up the camera: disable FOV if needed and hide the UI
        public static void SetCamera(CameraController controller, Camera camera)
        {
            Log.Msg("setting camera");
            DepthOfField effect = controller.GetComponent<DepthOfField>();
            TiltShiftEffect legacyEffect = controller.GetComponent<TiltShiftEffect>();

            if (Config.Global.integrateHideUI)
            {
                UIHider.Hide();
            }
            FPSCamera.Instance.UI.Hide();

            camera.nearClipPlane = 1f;
            controller.enabled = false;
            controller.m_maxDistance = 50f;
            camera.fieldOfView = Config.Global.fieldOfView;
            camera.transform.position = Vector3.zero;
            camera.transform.rotation = Quaternion.identity;
            
            if (effect != null)
            {
                effect.enabled = Config.Global.enableDOF;                
                effect.focalLength = 10f;   // set to 1/4 minimum vanilla value (ground level)                
                effect.focalSize = 0.8f;    // a bit bigger, to reduce blur some more
                effect.nearBlur = false;
            }
            if (legacyEffect != null) legacyEffect.enabled = false;            
        }

        // return the camera back to the game controller
        public static void StopCamera(CameraController controller, Camera camera)
        {
            Log.Msg("stopping camera");
            if (Config.Global.integrateHideUI)
            {
                UIHider.Show();
            }
            FPSCamera.Instance.UI.Hide();
            camera.transform.position = Vector3.zero;
            camera.transform.rotation = Quaternion.identity;
        
            controller.enabled = true;
            camera.nearClipPlane = 1.0f;
            controller.m_maxDistance = 4000f;
            camera.fieldOfView = FPSCamera.Instance.originalFieldOfView;
        }
    }
}
