using UnityEngine;

namespace FPSCamera
{    
    public struct Service
    {
        public static readonly Service PublicTransport = new Service(ItemClass.Service.PublicTransport);

        public ItemClass.Service service { get; private set; }
        private Service(ItemClass.Service service) { this.service = service; }
    }
    public struct VehicleType
    {
        public static readonly VehicleType Bicycle = new VehicleType(VehicleInfo.VehicleType.Bicycle);

        public VehicleInfo.VehicleType type;
        private VehicleType(VehicleInfo.VehicleType type) { this.type = type; }
    }    

    public class FPSVehicle
    {
        private static readonly VehicleManager vehicleM = VehicleManager.instance;
        public FPSVehicle(VehicleID id)
        {
            this.id = id;
            vehicle = vehicleM.m_vehicles.m_buffer[id.ID];
        }
        public static FPSVehicle Of(VehicleID id) => new FPSVehicle(id);

        public bool Exists() => Vehicle.Flags.Created == (vehicle.m_flags
                                    & (Vehicle.Flags.Created | Vehicle.Flags.Deleted));
        public bool Spawned() => (vehicle.m_flags & Vehicle.Flags.Spawned) != 0;
        public bool IsOfType(VehicleType type)
            => vehicle.Info.m_vehicleType == type.type;
        public bool IsOfService(Service service)
            => vehicle.Info.GetService() == service.service;
        public bool IsReversed()
            => (vehicle.m_flags & Vehicle.Flags.Reversed) != 0;
        public bool IsLeading() => vehicle.m_leadingVehicle == 0;
        public bool IsTrailing() => vehicle.m_trailingVehicle == 0;

        public Vector3 Position() => vehicle.GetSmoothPosition(id.ID);
        public void PositionRotation(out Vector3 position, out Quaternion rotation)
        {
            vehicle.GetSmoothPosition(id.ID, out position, out rotation);
        }
        public Vector3 Velocity() => vehicle.GetSmoothVelocity(id.ID);
        public float AttachOffsetFront() => vehicle.Info.m_attachOffsetFront;
        public VehicleID FrontVehicleID()
            => (VehicleID)(IsReversed() ? vehicle.GetLastVehicle(id.ID)
                                      : vehicle.GetFirstVehicle(id.ID));        
        public UUID OwnerID() => (UUID) vehicle.Info.m_vehicleAI.GetOwnerID(id.ID, ref vehicle);
        public UUID TargetID() => (UUID) vehicle.Info.m_vehicleAI.GetTargetID(id.ID, ref vehicle);

        public string TransportLineName()
            => TransportManager.instance.GetLineName(vehicle.m_transportLine);
        public void GetPassengerSizeCapacity(out int size, out int capacity)
        {
            vehicle.Info.m_vehicleAI.GetBufferStatus(id.ID, ref vehicle, out _,
                    out size, out capacity);
        }

        private VehicleID id;
        private Vehicle vehicle;
    }
}
