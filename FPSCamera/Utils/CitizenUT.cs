using UnityEngine;

namespace FPSCamMod
{
    public class FPSCitizen
    {
        private readonly static CitizenManager citizenM = CitizenManager.instance;
        public FPSCitizen(CitizenID id)
        {
            _citizen = citizenM.m_citizens.m_buffer[id._id];
            _instance = citizenM.m_instances.m_buffer[_citizen.m_instance];
        }
        public static FPSCitizen Of(CitizenID id) => new FPSCitizen(id);

        public bool exists
            => CitizenInstance.Flags.Created == (_instance.m_flags
               & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted));
        public bool isEnteringVehicle
            => (_instance.m_flags & CitizenInstance.Flags.EnteringVehicle) != 0;
        public VehicleID riddenVehicleID => (VehicleID) _citizen.m_vehicle;
        public BuildingID targetBuildingID => (BuildingID) _instance.m_targetBuilding;

        public Vector3 Position() => _instance.GetSmoothPosition(_citizen.m_instance);
        public void PositionRotation(out Vector3 position, out Quaternion rotation)
        {
            _instance.GetSmoothPosition(_citizen.m_instance, out position, out rotation);
        }
        public Vector3 Velocity() // TODO: improvement
            => _instance.GetLastFrameData().m_velocity;
        public UUID TargetID()
        {
            _instance.Info.m_citizenAI.GetLocalizedStatus(_citizen.m_instance, ref _instance,
                out InstanceID targetID);
            return (UUID) targetID;
        }

        private Citizen _citizen;
        protected CitizenInstance _instance;
    }
}
