namespace FPSCamera.Wrapper
{
    using System.Linq;
    using Transform;

    public class Human : Object<HumanID>
    {
        public override string Name => manager.GetCitizenName(id.implIndex);

        public bool IsTourist => _Is(Citizen.Flags.Tourist);
        public bool IsStudent => _Is(Citizen.Flags.Student);
        public VehicleID RiddenVehicleID => VehicleID.FromGame(_citizen.m_vehicle);
        public BuildingID WorkBuildingID => BuildingID.FromGame(_citizen.m_workBuilding);
        public BuildingID HomeBuildingID => BuildingID.FromGame(_citizen.m_homeBuilding);

        protected bool _Is(Citizen.Flags flags) => (_citizen.m_flags & flags) != 0;

        internal static Human _Of(HumanID id)
            => Of(_GetPedestrianID(id)) is Pedestrian ped ? ped : new Human(id);

        protected Human(HumanID id) : base(id) => _citizen = _GetCitizen(id);

        protected readonly Citizen _citizen;

        private static Citizen _GetCitizen(HumanID hid)
            => manager.m_citizens.m_buffer[hid.implIndex];
        private static PedestrianID _GetPedestrianID(HumanID hid)
            => PedestrianID.FromGame(_GetCitizen(hid).m_instance);
        private static readonly CitizenManager manager = CitizenManager.instance;
    }

    public class Pedestrian : Human, IObjectToFollow
    {
        public override string Name => manager.GetCitizenName(id.implIndex);

        public Positioning GetPositioning()
        {
            _instance.GetSmoothPosition(pedestrianID.implIndex,
                                        out var position, out var rotation);
            return new Positioning(Position.FromGame(position), Angle.FromGame(rotation));
        }
        public float GetSpeed() => _instance.GetLastFrameData().m_velocity.magnitude;
        public string GetStatus()
        {
            var _c = _citizen;
            var status = _instance.Info.m_citizenAI.GetLocalizedStatus(
                                pedestrianID.implIndex, ref _c, out var implID);
            switch (ID.FromGame(implID)) {
            case BuildingID bid: status += Building.GetName(bid); break;
            case NodeID nid:
                if (Node.GetTransitLineID(nid) is TransitID tid)
                    status += TransitLine.GetName(tid);
                break;
            }
            return status;
        }
        public Utils.Infos GetInfos()
        {
            Utils.Infos details = new Utils.Infos();

            string occupation;
            if (IsTourist) occupation = "(tourist)";
            else {
                occupation = OfIfValid(WorkBuildingID) is Building workBuilding ?
                                 (IsStudent ? "student at: " : "worker at: ") + workBuilding.Name :
                                 "(unemployed)";

                details["Home"] = OfIfValid(HomeBuildingID) is Building homeBuilding ?
                                      homeBuilding.Name : "(homeless)";
            }
            details["Occupation"] = occupation;

            return details;
        }

        public static PedestrianID GetRandomID()
        {
            var indices = Enumerable.Range(0, manager.m_instanceCount)
                                    .Select(n => (ushort) n).Where(i =>
                                        OfIfValid(PedestrianID.FromGame(i)) is Pedestrian p &&
                                        !p._Is(CitizenInstance.Flags.HangAround) &&
                                        OfIfValid(p.RiddenVehicleID) is null);

            return PedestrianID.FromGame(indices.GetRandomOne());
        }

        private static CitizenInstance _GetCitizenInstance(PedestrianID pid)
            => manager.m_instances.m_buffer[pid.implIndex];
        private static HumanID _GetHumanID(PedestrianID pid)
            => HumanID.FromGame(_GetCitizenInstance(pid).m_citizen);

        internal static Pedestrian _Of(PedestrianID id)
            => _GetHumanID(id) is HumanID hid ? new Pedestrian(id, hid) : null;
        private Pedestrian(PedestrianID pid, HumanID hid) : base(hid)
        {
            pedestrianID = pid;
            _instance = _GetCitizenInstance(pedestrianID);
        }

        private bool _Is(CitizenInstance.Flags flags) => (_instance.m_flags & flags) != 0;

        public readonly PedestrianID pedestrianID;
        private readonly CitizenInstance _instance;

        private static readonly CitizenManager manager = CitizenManager.instance;
    }
}
