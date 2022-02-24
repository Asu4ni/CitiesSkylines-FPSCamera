using UnityEngine;


namespace FPSCamera
{

    public class CitizenCamera : BaseCamera
    {
        private CitizenID citizenID;

        public bool inVehicle = false;
        public VehicleCamera vehicleCamera;

        protected override void SetInstanceToFollowPost()
        {
            citizenID = followedID.Citizen;
            Log.Msg("start following citizen | ID: " + citizenID.ID.ToString());
            inVehicle = false;
        }

        protected override Vector3 GetVelocity() => FPSCitizen.Of(citizenID).Velocity();

        protected override string GetDestinationStr()
        {
            var citizen = FPSCitizen.Of(citizenID);
            return GeneralUT.GetBuildingName(citizen.TargetBuildingID())
                   ?? GeneralUT.GetBuildingName(citizen.TargetID().Building)
                   ?? unknownStr;
        }
        protected override string GetDisplayInfoStr()
            => GeneralUT.RaycastRoad(FPSCitizen.Of(citizenID).Position()) ?? unknownStr;

        protected override void StopFollowingPre()
        {
            if (inVehicle) vehicleCamera.StopFollowing();
        }

        protected override bool UpdateCam()
        {
            var citizen = FPSCitizen.Of(citizenID);

            if (inVehicle)
            {
                if (!citizen.RiddenVehicleID().Exists())
                {
                    Log.Msg("citizen leaving the vehicle | ID: " + citizenID.ID.ToString());
                    inVehicle = false;
                    vehicleCamera.StopFollowing();
                    SetInstanceToFollow(followedID);
                }
                return false;
            }
            else if (!citizen.Exists())
            {
                Log.Msg("citizen disappears | ID: " + citizenID.ID.ToString());
                StopFollowing();
                return false;
            }
            else if (citizen.IsEnteringVehicle())
            {
                var vehicleID = citizen.RiddenVehicleID();
                if (vehicleID.Exists())
                {
                    Log.Msg("citizen entering a vehicle | ID: " + citizenID.ID.ToString());
                    var vehicle = FPSVehicle.Of(vehicleID);
                    // TODO: consider allowing to follow all kinds
                    if (vehicle.IsOfService(Service.PublicTransport))
                        vehicleCamera.SetInstanceToFollow(vehicle.FrontVehicleID());
                }
                else
                {
                    Log.Warn("vehicle not found while the citizen entering it | ID: "
                                    + citizenID.ID.ToString());
                    StopFollowing();
                }
                return false;
            }
            else
            {
                citizen.PositionRotation(out Vector3 position, out Quaternion rotation);

                var forward = rotation * Vector3.forward;
                var up = rotation * Vector3.up;

                var transform = camera.transform;

                transform.position = CameraUT.CamPosition(position, forward, up) + userOffset;
                var oldRotation = transform.rotation;
                transform.LookAt(transform.position + rotation * Vector3.forward, Vector3.up);
                transform.rotation = Quaternion.Slerp(oldRotation, transform.rotation,
                                                      Time.deltaTime);
                // TODO: substitute m_currentTargetRotation                
                return true;
            }
        }
    }
}
