using System.Linq;
using UnityEngine;

namespace FPSCamMod
{
    public class FPSVehicle
    {
        private readonly static VehicleManager vehicleM = VehicleManager.instance;
        public FPSVehicle(VehicleID id)
        {
            this.id = id;
            _vehicle = vehicleM.m_vehicles.m_buffer[id._id];
        }
        public static FPSVehicle Of(VehicleID id) => new FPSVehicle(id);

        public bool exists => Vehicle.Flags.Created == (_vehicle.m_flags
                                    & (Vehicle.Flags.Created | Vehicle.Flags.Deleted));
        public bool spawned => (_vehicle.m_flags & Vehicle.Flags.Spawned) != 0;
        public bool isReversed
            => (_vehicle.m_flags & Vehicle.Flags.Reversed) != 0;
        public bool isLeading => _vehicle.m_leadingVehicle == 0;
        public bool isTrailing => _vehicle.m_trailingVehicle == 0;
        public bool IsOfType(VehicleType type)
            => _vehicle.Info.m_vehicleType == type._type;
        public bool IsOfService(Service service)
            => _vehicle.Info.GetService() == service._service;

        public Vector3 Position() => _vehicle.GetSmoothPosition(id._id);
        public void PositionRotation(out Vector3 position, out Quaternion rotation)
        {
            _vehicle.GetSmoothPosition(id._id, out position, out rotation);
        }
        public Vector3 Velocity() => _vehicle.GetSmoothVelocity(id._id);
        public float AttachOffsetFront() => _vehicle.Info.m_attachOffsetFront;
        public VehicleID FrontVehicleID()
            => (VehicleID) (isReversed ? _vehicle.GetLastVehicle(id._id)
                                      : _vehicle.GetFirstVehicle(id._id));
        public UUID OwnerID() => (UUID) _vehicle.Info.m_vehicleAI.GetOwnerID(id._id, ref _vehicle);
        public UUID TargetID() => (UUID) _vehicle.Info.m_vehicleAI.GetTargetID(id._id, ref _vehicle);

        public string TransportLineName()
            => TransportManager.instance.GetLineName(_vehicle.m_transportLine);
        public void GetPassengerSizeCapacity(out int size, out int capacity)
        {
            _vehicle.Info.m_vehicleAI.GetBufferStatus(id._id, ref _vehicle, out _,
                    out size, out capacity);
        }

        public static VehicleID GetRandomID()
        {
            var indices = Enumerable.Range(0, vehicleM.m_vehicles.m_buffer.Length).Where(i => {
                var v = Of((VehicleID) i);
                return v.exists && (
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

        private VehicleID id;
        private Vehicle _vehicle;
    }

    public struct Service
    {
        public static readonly Service PublicTransport = new Service(ItemClass.Service.PublicTransport);

        public ItemClass.Service _service { get; private set; }
        private Service(ItemClass.Service service) { this._service = service; }
    }
    public struct VehicleType
    {
        public static readonly VehicleType Car = new VehicleType(VehicleInfo.VehicleType.Car);
        public static readonly VehicleType Metro = new VehicleType(VehicleInfo.VehicleType.Metro);
        public static readonly VehicleType Train = new VehicleType(VehicleInfo.VehicleType.Train);
        public static readonly VehicleType Ship = new VehicleType(VehicleInfo.VehicleType.Ship);
        public static readonly VehicleType Plane = new VehicleType(VehicleInfo.VehicleType.Plane);
        public static readonly VehicleType Bicycle = new VehicleType(VehicleInfo.VehicleType.Bicycle);
        public static readonly VehicleType Tram = new VehicleType(VehicleInfo.VehicleType.Tram);
        public static readonly VehicleType Helicopter = new VehicleType(VehicleInfo.VehicleType.Helicopter);
        public static readonly VehicleType Ferry = new VehicleType(VehicleInfo.VehicleType.Ferry);
        public static readonly VehicleType Monorail = new VehicleType(VehicleInfo.VehicleType.Monorail);
        public static readonly VehicleType CableCar = new VehicleType(VehicleInfo.VehicleType.CableCar);
        public static readonly VehicleType Blimp = new VehicleType(VehicleInfo.VehicleType.Blimp);
        public static readonly VehicleType Balloon = new VehicleType(VehicleInfo.VehicleType.Balloon);
        public static readonly VehicleType Trolleybus = new VehicleType(VehicleInfo.VehicleType.Trolleybus);

        public VehicleInfo.VehicleType _type;
        private VehicleType(VehicleInfo.VehicleType type) { this._type = type; }
    }
}
