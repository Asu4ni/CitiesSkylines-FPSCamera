namespace FPSCamera.Wrapper
{
    using System.Linq;
    using Transform;

    public abstract class Vehicle : Object<VehicleID>, IObjectToFollow
    {
        public override string Name => manager.GetVehicleName(GetHeadVehicleID().implIndex);

        public Positioning GetPositioning()
        {
            _vehicle.GetSmoothPosition(_vid.implIndex, out var position, out var rotation);
            return new Positioning(Position.FromGame(position), Angle.FromGame(rotation));
        }
        public float GetSpeed() => _vehicle.GetSmoothVelocity(_vid.implIndex).magnitude;

        public virtual string GetStatus()
        {
            var vehicle = _Of(GetHeadVehicleID());
            var status = vehicle._VAI.GetLocalizedStatus(
                                vehicle._vid.implIndex, ref vehicle._vehicle, out var implID);
            switch (ID.FromGame(implID)) {
            case BuildingID bid: status += Building.GetName(bid); break;
            case HumanID hid: status += Of(hid).Name; break;
            }
            return status;
        }

        public Utils.Infos GetInfos()
        {
            Utils.Infos details = new Utils.Infos();

            var vehicle = _Of(GetHeadVehicleID());
            switch (vehicle.GetOwnerID()) {
            case BuildingID buildingID:
                details["Owner"] = Building.GetName(buildingID); break;
            case HumanID humanID:
                details["Owner"] = Of(humanID).Name; break;
            }

            vehicle._MoreDetails(ref details);
            return details;
        }
        public virtual void _MoreDetails(ref Utils.Infos details) { }

        public bool IsSpawned => _Is(global::Vehicle.Flags.Spawned);
        public bool IsReversed => _Is(global::Vehicle.Flags.Reversed);
        public bool IsLeading => _vehicle.m_leadingVehicle == 0;
        public bool IsTrailing => _vehicle.m_trailingVehicle == 0;
        public bool IsMiddle => !(IsLeading || IsTrailing);

        public float GetAttachOffsetFront() => _VInfo.m_attachOffsetFront;
        public VehicleID GetFrontVehicleID()
            => VehicleID.FromGame(IsReversed ? _vehicle.GetLastVehicle(_vid.implIndex) :
                                               _vehicle.GetFirstVehicle(_vid.implIndex));

        public static VehicleID GetHeadVehicleIDof(VehicleID id)
            => VehicleID.FromGame(_GetVehicle(id).GetFirstVehicle(id.implIndex));
        public VehicleID GetHeadVehicleID()
            => VehicleID.FromGame(_vehicle.GetFirstVehicle(_vid.implIndex));
        public ID GetOwnerID() => ID.FromGame(_VAI.GetOwnerID(_vid.implIndex, ref _vehicle));
        public TransitID GetTransitLineID() => TransitID.FromGame(_vehicle.m_transportLine);
        // capacity == 0: invalid
        public void GetLoadAndCapacity(out int load, out int capacity)
            => _VAI.GetBufferStatus(_vid.implIndex, ref _vehicle, out _, out load, out capacity);

        public static VehicleID GetRandomID()
        {
            var indices = Enumerable.Range(0, manager.m_vehicleCount)
                    .Select(n => (ushort) n).Where(
                        i => OfIfValid(VehicleID.FromGame(i)) is Vehicle v
                    );
            return VehicleID.FromGame(indices.GetRandomOne());
        }

        internal static Vehicle _Of(VehicleID id)
        {
            var ai = _GetVehicle(GetHeadVehicleIDof(id)).Info.m_vehicleAI;
            switch (ai) {
            case BusAI busAi_________________: return new TransitVehicle(id, "Bus");
            case TramAI tramAi_______________: return new TransitVehicle(id, "Tram");
            case MetroTrainAI metroTrainAi___: return new TransitVehicle(id, "Metro");
            case PassengerTrainAI pTrainAi___: return new TransitVehicle(id, "Train");
            case PassengerPlaneAI pPlaneAi___: return new TransitVehicle(id, "Flight");
            case PassengerBlimpAI pBlimpAi___: return new TransitVehicle(id, "Blimp");
            case CableCarAI cableCarAi_______: return new TransitVehicle(id, "Gondola");
            case TaxiAI taxiAi_______________: return new TransitVehicle(id, "Taxi");
            case TrolleybusAI trolleybusAi___: return new TransitVehicle(id, "Trolleybus");
            case PassengerFerryAI pFerryAi___: return new TransitVehicle(id, "Ferry");
            case PassengerShipAI pShipAi_____: return new TransitVehicle(id, "Ship");
            case PassengerHelicopterAI phAi__: return new TransitVehicle(id, "Helicopter");

            case CargoTruckAI cargoTruckAi:
            case CargoTrainAI cargoTrainAi:
            case CargoShipAI cargoShipAi:
            case CargoPlaneAI cargoPlaneAi___: return new CargoVehicle(id);

            case AmbulanceAI ambulanceAi:
            case AmbulanceCopterAI aCopterAi_: return new ServiceVehicle(id, "Medical");
            case DisasterResponseVehicleAI dr:
            case DisasterResponseCopterAI drc: return new ServiceVehicle(id, "Disaster Response");
            case FireCopterAI fireCopterAi:
            case FireTruckAI fireTruckAi_____: return new ServiceVehicle(id, "Firefighting");
            case PoliceCopterAI pCopterAi:
            case PoliceCarAI policeCarAi_____: return new ServiceVehicle(id, "Police");
            case GarbageTruckAI gTruckAi_____: return new ServiceVehicle(id, "Garbage");
            case HearseAI hearseAi___________: return new ServiceVehicle(id, "Deathcare");
            case MaintenanceTruckAI mTruckAi_:
            case ParkMaintenanceVehicleAI pm_: return new ServiceVehicle(id, "Maintenance");
            case PostVanAI postVanAi_________: return new ServiceVehicle(id, "Postal");
            case SnowTruckAI snowTruckAi_____: return new ServiceVehicle(id, "Snow Plowing");
            case WaterTruckAI waterTruckAi___: return new ServiceVehicle(id, "Water Pumping");

            case PrivatePlaneAI pPlaneAi:
            case CarTrailerAI carTrailerAi:
            case PassengerCarAI pCarAi_______: return new PersonalVehicle(id);
            case BicycleAI bicycleAi_________: return new Bicycle(id);

            case BalloonAI balloonAi:
            case FishingBoatAI fishingBoatAi_: return new MissionVehicle(id);

            default:
                Log.Warn($"Vehicle(ID:{id} of type [{ai.GetType().Name}] is not recognized.");
                return null;
            }
        }
        protected Vehicle(VehicleID id) : base(id)
        {
            _vid = id;
            _vehicle = _GetVehicle(id);
        }

        private static global::Vehicle _GetVehicle(VehicleID id)
            => manager.m_vehicles.m_buffer[id.implIndex];
        private bool _Is(global::Vehicle.Flags flags) => (_vehicle.m_flags & flags) != 0;
        private bool _IsOfService(ItemClass.Service service)
            => _vehicle.Info.GetService() == service;

        private VehicleInfo _VInfo => _vehicle.Info;
        private VehicleAI _VAI => _VInfo.m_vehicleAI;

        private readonly VehicleID _vid;
        private global::Vehicle _vehicle;

        private static readonly VehicleManager manager = VehicleManager.instance;
    }
}
