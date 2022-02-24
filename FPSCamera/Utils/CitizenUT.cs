﻿using UnityEngine;

namespace FPSCamera
{
    public class FPSCitizen
    {
        private readonly static CitizenManager citizenM = CitizenManager.instance;
        public FPSCitizen(CitizenID id)
        {
            citizen = citizenM.m_citizens.m_buffer[id.ID];
            instance = citizenM.m_instances.m_buffer[citizen.m_instance];
        }
        public static FPSCitizen Of(CitizenID id) => new FPSCitizen(id);

        public bool Exists()
            => CitizenInstance.Flags.Created == (instance.m_flags
               & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted));
        public bool IsEnteringVehicle()
            => (instance.m_flags & CitizenInstance.Flags.EnteringVehicle) != 0;
        public Vector3 Velocity() // TODO: improvement
            => instance.GetLastFrameData().m_velocity;

        public Vector3 Position() => instance.GetSmoothPosition(citizen.m_instance);
        public void PositionRotation(out Vector3 position, out Quaternion rotation)
        {
            instance.GetSmoothPosition(citizen.m_instance, out position, out rotation);
        }
        public VehicleID RiddenVehicleID() => (VehicleID)citizen.m_vehicle;
        public BuildingID TargetBuildingID() => (BuildingID)instance.m_targetBuilding;
        public UUID TargetID()
        {
            instance.Info.m_citizenAI.GetLocalizedStatus(citizen.m_instance, ref instance,
                out InstanceID targetID);
            return (UUID)targetID;
        }


        private Citizen citizen;
        protected CitizenInstance instance;
    }
}