using System.Linq;
using UnityEngine;

namespace FPSCamMod
{
    public class FPSCitizen : FPSInstanceToFollow
    {
        public static FPSCitizen Of(CitizenID id) => new FPSCitizen(id);
        public static FPSCitizen Of(CInstanceID id) => new FPSCitizen(id);
        public FPSCitizen(CitizenID id)
        {
            _citizen = Manager.m_citizens.m_buffer[id._id];
            _instance = Manager.m_instances.m_buffer[_citizen.m_instance];
            uuid = (UUID) (CInstanceID) _citizen.m_instance;
        }
        public FPSCitizen(CInstanceID id)
        {
            _instance = Manager.m_instances.m_buffer[id._id];
            _citizen = Manager.m_citizens.m_buffer[_instance.m_citizen];
            uuid = (UUID) id;
        }

        public override string GetName() => Manager.GetInstanceName(_citizen.m_instance);

        public override CamSetting GetCamSetting()
        {
            CamSetting setting = new CamSetting();
            _instance.GetSmoothPosition(_citizen.m_instance,
                out setting.position, out setting.rotation);
            return setting;
        }
        public override float GetSpeed() => _instance.GetLastFrameData().m_velocity.magnitude;
        public override string GetStatus()
        {
            var status = _GetStatus(out var targetID);
            if (targetID.Building.Exists)
                status += FPSBuilding.Of(targetID.Building).GetName();
            else if (targetID.Node.Exists) {
                var tLineID = FPSNode.Of(targetID.Node).GetTransportLineID();
                if (tLineID.Exists) {
                    status += Of((UUID) tLineID).GetName();
                }
            }
            return status;
        }
        public override Details GetDetails()
        {
            Details details = new Details();

            string occupation;
            if (_is(Citizen.Flags.Tourist)) occupation = "<tourist>";
            else {
                var workBuilding = FPSBuilding.Of((BuildingID) _citizen.m_workBuilding);
                if (_citizen.GetCurrentSchoolLevel(uuid.Citizen._id) != ItemClass.Level.None) {
                    occupation = "<student> at " +
                                (workBuilding.IsValid ? workBuilding.GetName() : "(unknown)");
                }
                else occupation = workBuilding.IsValid ? "<worker> at " + workBuilding.GetName() :
                                                        "<unemployed>";

                var homeBuilding = FPSBuilding.Of((BuildingID) _citizen.m_homeBuilding);
                details["Home"] = homeBuilding.IsValid ? homeBuilding.GetName() : "(homeless)";
            }
            details["Occupation"] = occupation;

            return details;
        }

        private bool _is(Citizen.Flags flags) => (_citizen.m_flags & flags) != 0;
        private bool _is(CitizenInstance.Flags flags) => (_instance.m_flags & flags) != 0;

        public VehicleID RiddenVehicleID => (VehicleID) _citizen.m_vehicle;
        public BuildingID TargetBuildingID => (BuildingID) _instance.m_targetBuilding;

        public static CitizenID GetRandomID()
        {
            var indices = Enumerable.Range(0, Manager.m_instanceCount).Where(
                i => {
                    var c = Of((CitizenID) Manager.m_instances.m_buffer[i].m_citizen);
                    return c.IsValid &&
                           !c._is(CitizenInstance.Flags.HangAround) &&
                           !FPSVehicle.Of(c.RiddenVehicleID).IsValid;
                }
            );

            return indices.Count() == 0 ?
                        default : (CitizenID)
                        Manager.m_instances.m_buffer[
                            indices.ElementAt(Random.Range(0, indices.Count()))].m_citizen;
        }

        private string _GetStatus(out UUID targetID)
        {
            var status = _instance.Info.m_citizenAI.GetLocalizedStatus(
                                _citizen.m_instance, ref _citizen, out var _targetID);
            targetID = (UUID) _targetID;
            return status;
        }

        private Citizen _citizen;
        private CitizenInstance _instance;

        private static readonly CitizenManager Manager = CitizenManager.instance;
    }
}
