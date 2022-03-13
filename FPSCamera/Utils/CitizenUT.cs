using System.Linq;
using UnityEngine;

namespace FPSCamMod
{
    public class FPSCitizen : FPSInstance
    {
        private readonly static CitizenManager Manager = CitizenManager.instance;

        public FPSCitizen(CitizenID id)
        {
            _citizen = Manager.m_citizens.m_buffer[id._id];
            _instance = Manager.m_instances.m_buffer[_citizen.m_instance];
            ID = (CInstanceID) _citizen.m_instance;
        }
        public static FPSCitizen Of(CitizenID id) => new FPSCitizen(id);
        public FPSCitizen(CInstanceID id)
        {
            _instance = Manager.m_instances.m_buffer[id._id];
            _citizen = Manager.m_citizens.m_buffer[_instance.m_citizen];
            ID = id;
        }
        public static FPSCitizen Of(CInstanceID id) => new FPSCitizen(id);

        public bool isEnteringVehicle
            => (_instance.m_flags & CitizenInstance.Flags.EnteringVehicle) != 0;
        public VehicleID riddenVehicleID => (VehicleID) _citizen.m_vehicle;
        public BuildingID targetBuildingID => (BuildingID) _instance.m_targetBuilding;

        public Vector3 Position() => _instance.GetSmoothPosition(_citizen.m_instance);
        public void PositionRotation(out Vector3 position, out Quaternion rotation)
        {
            _instance.GetSmoothPosition(_citizen.m_instance, out position, out rotation);
        }
        public Vector3 Velocity() => _instance.GetLastFrameData().m_velocity;
        public string Name() => Manager.GetInstanceName(_citizen.m_instance);
        public UUID TargetID()
        {
            _instance.Info.m_citizenAI.GetLocalizedStatus(_citizen.m_instance, ref _instance,
                out InstanceID targetID);
            return (UUID) targetID;
        }

        public static CitizenID GetRandomID()
        {
            var indices = Enumerable.Range(0, Manager.m_instanceCount).Where(
                i => {
                    var c = Of((CitizenID) Manager.m_instances.m_buffer[i].m_citizen);
                    return c.isValid && ((BuildingID) c._instance.m_targetBuilding).exists &&
                           // TODO: investigate
                           ((CitizenInstance.Flags.WaitingTransport | CitizenInstance.Flags.RidingBicycle)
                                & c._instance.m_flags) == 0;
                }
            );

            return indices.Count() == 0 ?
                        default : (CitizenID)
                        Manager.m_instances.m_buffer[
                            indices.ElementAt(Random.Range(0, indices.Count()))].m_citizen;
        }

        private Citizen _citizen;
        protected CitizenInstance _instance;
    }
}
