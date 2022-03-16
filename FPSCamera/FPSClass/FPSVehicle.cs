using System.Linq;
using UnityEngine;

namespace FPSCamMod
{
    public class FPSVehicle : FPSInstanceToFollow
    {
        public static FPSVehicle Of(VehicleID id) => new FPSVehicle(id);
        public FPSVehicle(VehicleID id) : base((UUID) id)
        {
            this.vid = id;
            _vehicle = Manager.m_vehicles.m_buffer[id._id];
        }

        public override string GetName() => Manager.GetVehicleName(GetHeadVehicleID()._id);

        public override CamSetting GetCamSetting()
        {
            CamSetting setting = new CamSetting();
            _vehicle.GetSmoothPosition(vid._id, out setting.position, out setting.rotation);
            return setting;
        }
        public override float GetSpeed() => _vehicle.GetSmoothVelocity(vid._id).magnitude;
        public override string GetStatus()
        {
            var vehicle = Of(GetHeadVehicleID());
            var status = _GetStatus(out var targetID);
            if (targetID.Building.Exists)
                status += FPSBuilding.Of(targetID.Building).GetName();
            else if (targetID.Citizen.Exists)
                status += FPSCitizen.Of(targetID.Citizen).GetName();

            return status;
        }
        public override Details GetDetails()
        {
            Details details = new Details();
            var vehicle = Of(GetHeadVehicleID());
            if (vehicle.IsOfService(Service.PublicTransport)) {
                var lineID = vehicle.GetTransportLineID();
                details["Public Transport"] = lineID.Exists ?
                        FPSTransportLine.Of(lineID).GetName() : "(non-regular)";

                vehicle.GetLoadAndCapacity(out int load, out int capacity);
                if (capacity > 0) {
                    if (vehicle.IsCargoTrain)
                        details["Load"] = ((float) load / capacity).ToString("P1");
                    else details["Passenger"] = $"{load,4} /{capacity,4}";
                }
            }
            else {
                var ownerID = vehicle.GetOwnerID();
                if (ownerID.Building.Exists)
                    details["Owner"] = FPSBuilding.Of(ownerID.Building).GetName();
                else if (ownerID.Citizen.Exists)
                    details["Owner"] = FPSCitizen.Of(ownerID.Citizen).GetName();

                vehicle.GetLoadAndCapacity(out int load, out int capacity);
                if (capacity > 0) details["Load"] = ((float) load / capacity).ToString("P1");
            }
            return details;
        }

        public bool IsSpawned => (_vehicle.m_flags & Vehicle.Flags.Spawned) != 0;
        public bool IsReversed => (_vehicle.m_flags & Vehicle.Flags.Reversed) != 0;
        public bool IsLeading => _vehicle.m_leadingVehicle == 0;
        public bool IsTrailing => _vehicle.m_trailingVehicle == 0;
        public bool IsCargoTrain => ((VehicleID) _vehicle.m_firstCargo).Exists;

        public bool IsOfType(VehicleType type) => _vehicle.Info.m_vehicleType == type._type;
        public bool IsOfService(Service service) => _vehicle.Info.GetService() == service._service;

        public float GetAttachOffsetFront() => _vehicle.Info.m_attachOffsetFront;
        public VehicleID GetFrontVehicleID() => (VehicleID) (IsReversed ?
                            _vehicle.GetLastVehicle(vid._id) : _vehicle.GetFirstVehicle(vid._id));
        public VehicleID GetHeadVehicleID() => (VehicleID) _vehicle.GetFirstVehicle(vid._id);
        public UUID GetOwnerID()
                        => (UUID) _vehicle.Info.m_vehicleAI.GetOwnerID(vid._id, ref _vehicle);
        public TLineID GetTransportLineID() => (TLineID) _vehicle.m_transportLine;
        // capacity == 0: invalid
        public void GetLoadAndCapacity(out int load, out int capacity)
        {
            _vehicle.Info.m_vehicleAI.GetBufferStatus(vid._id, ref _vehicle, out _,
                    out load, out capacity);
        }

        public static VehicleID GetRandomID()
        {
            var indices = Enumerable.Range(0, Manager.m_vehicleCount).Where(i => {
                var v = Of((VehicleID) i);
                return v.IsValid && (
                         v.IsOfType(VehicleType.Car) || v.IsOfType(VehicleType.Bicycle) ||
                         v.IsOfType(VehicleType.Metro) || v.IsOfType(VehicleType.Train) ||
                         v.IsOfType(VehicleType.Tram) || v.IsOfType(VehicleType.Monorail) ||
                         v.IsOfType(VehicleType.Ship) || v.IsOfType(VehicleType.Plane) ||
                         v.IsOfType(VehicleType.Trolleybus) || v.IsOfType(VehicleType.CableCar) ||
                         v.IsOfType(VehicleType.Helicopter) || v.IsOfType(VehicleType.Ferry) ||
                         v.IsOfType(VehicleType.Blimp) || v.IsOfType(VehicleType.Balloon));
            });
            return indices.Count() == 0 ?
                        default : (VehicleID)
                        indices.ElementAt(Random.Range(0, indices.Count()));
        }

        private string _GetStatus(out UUID targetID)
        {
            var status = _vehicle.Info.m_vehicleAI.GetLocalizedStatus(
                                vid._id, ref _vehicle, out var _targetID);
            targetID = (UUID) _targetID;
            return status;
        }

        private readonly VehicleID vid;
        private Vehicle _vehicle;

        private static readonly VehicleManager Manager = VehicleManager.instance;
    }

    public struct Service
    {
        public static readonly Service PublicTransport
            = new Service(ItemClass.Service.PublicTransport, "public transport");

        public ItemClass.Service _service { get; private set; }
        public override string ToString() => _name;
        private string _name;
        private Service(ItemClass.Service service, string name)
        { _service = service; _name = name; }
    }
    public struct VehicleType
    {
        public static readonly VehicleType Car = new VehicleType(VehicleInfo.VehicleType.Car);
        public static readonly VehicleType Metro = new VehicleType(VehicleInfo.VehicleType.Metro);
        public static readonly VehicleType Train = new VehicleType(VehicleInfo.VehicleType.Train);
        public static readonly VehicleType Ship = new VehicleType(VehicleInfo.VehicleType.Ship);
        public static readonly VehicleType Plane = new VehicleType(VehicleInfo.VehicleType.Plane);
        public static readonly VehicleType Bicycle
            = new VehicleType(VehicleInfo.VehicleType.Bicycle);
        public static readonly VehicleType Tram = new VehicleType(VehicleInfo.VehicleType.Tram);
        public static readonly VehicleType Helicopter
            = new VehicleType(VehicleInfo.VehicleType.Helicopter);
        public static readonly VehicleType Ferry = new VehicleType(VehicleInfo.VehicleType.Ferry);
        public static readonly VehicleType Monorail
            = new VehicleType(VehicleInfo.VehicleType.Monorail);
        public static readonly VehicleType CableCar
            = new VehicleType(VehicleInfo.VehicleType.CableCar);
        public static readonly VehicleType Blimp = new VehicleType(VehicleInfo.VehicleType.Blimp);
        public static readonly VehicleType Balloon
            = new VehicleType(VehicleInfo.VehicleType.Balloon);
        public static readonly VehicleType Trolleybus
            = new VehicleType(VehicleInfo.VehicleType.Trolleybus);


        // Type which Walk-Through Mode won't select
        public static readonly VehicleType Rocket = new VehicleType(VehicleInfo.VehicleType.Rocket);

        public VehicleInfo.VehicleType _type;
        private VehicleType(VehicleInfo.VehicleType type) { _type = type; }
    }
}
