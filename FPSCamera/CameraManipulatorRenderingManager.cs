using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FPSCamera
{
    class CameraManipulatorManager : SimulationManagerBase<CameraManipulatorManager, MonoBehaviour>, IRenderableManager, ISimulationManager
    {
        public bool m_registered = false;

        protected override void BeginOverlayImpl(RenderManager.CameraInfo cameraInfo)
        {
            if (FPSCamera.instance.fpsModeEnabled)
            {
                float height = FPSCamera.instance.camera.transform.position.y - TerrainManager.instance.SampleDetailHeight(FPSCamera.instance.camera.transform.position);
                cameraInfo.m_position = FPSCamera.instance.camera.transform.position;
                cameraInfo.m_rotation = FPSCamera.instance.camera.transform.rotation;
                cameraInfo.m_height = height;
            }
            if (FPSCamera.instance.citizenCamera.following)
            {
                float height = FPSCamera.instance.camera.transform.position.y - TerrainManager.instance.SampleDetailHeight(FPSCamera.instance.gameObject.transform.position);
                cameraInfo.m_position = FPSCamera.instance.citizenCamera.camera.transform.position;
                cameraInfo.m_rotation = FPSCamera.instance.citizenCamera.camera.transform.rotation;
                cameraInfo.m_height = height;
            }
            if (FPSCamera.instance.vehicleCamera.following)
            {
                float height = FPSCamera.instance.gameObject.transform.position.y - TerrainManager.instance.SampleDetailHeight(FPSCamera.instance.gameObject.transform.position);
                cameraInfo.m_position = FPSCamera.instance.vehicleCamera.camera.transform.position;
                cameraInfo.m_rotation = FPSCamera.instance.vehicleCamera.camera.transform.rotation;
                cameraInfo.m_height = height;
            }
        }

  
    }
}
