using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace FPSCamera
{
    class CameraUtils
    {

        public const float CAMERAOFFSETFORWARD = 2.75f;
        public const float CAMERAOFFSETUP = 1.5f;

        /// <summary>
        /// Sets up the camera, basically disable FOV if needed and hide the UI
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="camera"></param>
        public static void SetCamera(CameraController controller,Camera camera)
        {
            DepthOfField effect = controller.GetComponent<DepthOfField>();
            TiltShiftEffect legacyEffect = controller.GetComponent<TiltShiftEffect>();

            if (FPSCamera.instance.config.integrateHideUI)
            {
                UIHider.Hide();
            }
            FPSCamera.instance.ui.Hide();

            camera.nearClipPlane = 1f;
            controller.enabled = false;
            controller.m_maxDistance = 50f;
            camera.fieldOfView = FPSCamera.instance.config.fieldOfView;
            camera.transform.position = Vector3.zero;
            camera.transform.rotation = Quaternion.identity;
            
            if (effect != null)
            {
                effect.enabled = FPSCamera.instance.config.enableDOF;
                //Set to 1/4 minimum vanilla value( ground level )
                effect.focalLength = 10;
                //A bit bigger, to reduce blur some more
                effect.focalSize = 0.8f;
                effect.nearBlur = false;
            }
            if (legacyEffect != null)
            {
                legacyEffect.enabled = false;
            }
            
        }

        /// <summary>
        /// Returns the camera back to the game controller, basically undo what SetCamera did
        /// </summary>
        /// <seealso cref="SetCamera(CameraController, Camera)"/>
        /// <param name="controller"></param>
        /// <param name="camera"></param>
        public static void StopCamera(CameraController controller, Camera camera)
        {
            DepthOfField effect = controller.GetComponent<DepthOfField>();
            TiltShiftEffect legacyEffect = controller.GetComponent<TiltShiftEffect>();

            if (FPSCamera.instance.config.integrateHideUI)
            {
                UIHider.Show();
            }
            FPSCamera.instance.ui.Hide();
            camera.transform.position = Vector3.zero;
            camera.transform.rotation = Quaternion.identity;
        
            controller.enabled = true;
            camera.nearClipPlane = 1.0f;
            controller.m_maxDistance = 4000f;
            camera.fieldOfView = FPSCamera.instance.originalFieldOfView;
        }

    }
}
