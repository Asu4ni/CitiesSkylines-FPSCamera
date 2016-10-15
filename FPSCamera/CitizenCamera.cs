using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace FPSCamera
{

    public class CitizenCamera : MonoBehaviour, InstanceCamera
    {
        private uint followInstance;
        public bool following = false;
        public bool inVehicle = false;

        private CameraController cameraController;
        public Camera camera;
        public DepthOfField effect;

        private CitizenManager cManager;

        public Vector3 userOffset = Vector3.zero;
        public VehicleCamera vehicleCamera;

        public void SetFollowInstance(uint instance)
        {
            FPSCamera.instance.SetMode(false);
            followInstance = instance;
            following = true;
            CameraUtils.setCamera(cameraController, camera);
            FPSCamera.onCameraModeChanged(true);
        }

        public void StopFollowing()
        {
            following = false;
            CameraUtils.stopCamera(cameraController, camera);
            userOffset = Vector3.zero;
            camera.fieldOfView = FPSCamera.instance.originalFieldOfView;
            FPSCamera.onCameraModeChanged(false);
            if(!inVehicle)
            {
                vehicleCamera.StopFollowing();
            }
        }

        void Awake()
        {
            cameraController = GetComponent<CameraController>();
            camera = GetComponent<Camera>();
            cManager = CitizenManager.instance;
        }

        void Update()
        {
            if (following)
            {
                var citizen = cManager.m_citizens.m_buffer[followInstance];
                var i = citizen.m_instance;
                var flags = cManager.m_instances.m_buffer[i].m_flags;

                if ( inVehicle )
                {
                    if (citizen.m_vehicle == 0)
                    {
                        inVehicle = false;
                        vehicleCamera.StopFollowing();
                        SetFollowInstance(followInstance);
                    }
                    return;
                }
                
                if ((flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted)) != CitizenInstance.Flags.Created)
                {
                    inVehicle = false;
                    StopFollowing();
                    return;
                }

                if ((flags & CitizenInstance.Flags.EnteringVehicle) != 0)
                {                    
                    if ( citizen.m_vehicle != 0 )
                    {
                        ushort vehicleId = citizen.m_vehicle;
                        if((VehicleManager.instance.m_vehicles.m_buffer[vehicleId].Info.GetService() == ItemClass.Service.PublicTransport))
                        {
                            while(VehicleManager.instance.m_vehicles.m_buffer[vehicleId].m_leadingVehicle != 0)
                            {
                                vehicleId = VehicleManager.instance.m_vehicles.m_buffer[vehicleId].m_leadingVehicle;
                            }
                            inVehicle = true;
                            vehicleCamera.SetFollowInstance(vehicleId);
                            return;
                        }

                    }
                    StopFollowing();
                    return;
                }

                CitizenInstance c = cManager.m_instances.m_buffer[i];
                Vector3 position = Vector3.zero;
                Quaternion orientation = Quaternion.identity;
                c.GetSmoothPosition((ushort)i, out position, out orientation);

                Vector3 forward = orientation * Vector3.forward;
                Vector3 up = orientation * Vector3.up;

                camera.transform.position = GetOffset(position, forward, up, userOffset);
                Vector3 lookAt = camera.transform.position + (orientation * Vector3.forward) * 1.0f;
                var currentOrientation = camera.transform.rotation;
                camera.transform.LookAt(lookAt, Vector3.up);
                camera.transform.rotation = Quaternion.Slerp(currentOrientation, camera.transform.rotation,
                    Time.deltaTime*2.0f);

                float height = camera.transform.position.y - TerrainManager.instance.SampleDetailHeight(camera.transform.position);
                cameraController.m_currentPosition = camera.transform.position;

                if(effect)
                {
                    effect.enabled = FPSCamera.instance.config.enableDOF;
                }

            }
        }

        public Vector3 GetOffset( Vector3 position, Vector3 forward, Vector3 up, Vector3 userOffset)
        {
            Vector3 retVal = position +
                          forward * CameraUtils.CAMERAOFFSETFORWARD +
                          up * CameraUtils.CAMERAOFFSETUP;
            return retVal + userOffset;
        }
        
    }

}
