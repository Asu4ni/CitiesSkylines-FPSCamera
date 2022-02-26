using UnityEngine;

namespace FPSCamera
{
    public struct UUID
    {
        public CitizenID Citizen { get => (CitizenID)id.Citizen; set => id.Citizen = value.ID; }
        public static implicit operator UUID(CitizenID id)
        { UUID uid = default; uid.Citizen = id; return uid; }
        public VehicleID Vehicle { get => (VehicleID)id.Vehicle; set => id.Vehicle = value.ID; }
        public static implicit operator UUID(VehicleID id)
        { UUID uid = default; uid.Vehicle = id; return uid; }
        public BuildingID Building
        {
            get => (BuildingID)id.Building;
            set => id.Building = value.ID;
        }
        public static implicit operator UUID(BuildingID id)
        { UUID uid = default; uid.Building = id; return uid; }

        private InstanceID id;
        private UUID(InstanceID id) { this.id = id; }
        public InstanceID ID { get => id; }
        public static explicit operator UUID(InstanceID id) => new UUID(id);
    }

    public class BaseID<T> where T : System.IComparable<T>
    {
        public T ID { get => id; }
        public bool exists => id.CompareTo(default) != 0;

        private T id;
        protected BaseID(T id) { this.id = id; }
    }
    public class CitizenID : BaseID<uint>
    {
        private CitizenID(uint id) : base(id) { }
        public static explicit operator CitizenID(uint id) => new CitizenID(id);
    }
    public class VehicleID : BaseID<ushort>
    {
        private VehicleID(ushort id) : base(id) { }
        public static explicit operator VehicleID(ushort id) => new VehicleID(id);
    }
    public class BuildingID : BaseID<ushort>
    {
        private BuildingID(ushort id) : base(id) { }
        public static explicit operator BuildingID(ushort id) => new BuildingID(id);
    }

    public static class GeneralUT
    {
        public static string GetBuildingName(BuildingID id)
            => BuildingManager.instance.GetBuildingName(id.ID, default);

        public static string RaycastRoad(Vector3 position)
        {
            return Tool.RaycastRoad(position);
        }

        private class Tool : ToolBase
        {
            public static string RaycastRoad(Vector3 position)
            {
                RaycastInput raycastInput = new RaycastInput(new Ray(position, new Vector3(0, -1, 0)), 1000f);
                raycastInput.m_netService.m_service = ItemClass.Service.Road;
                raycastInput.m_netService.m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels;
                raycastInput.m_ignoreSegmentFlags = NetSegment.Flags.None;
                raycastInput.m_ignoreNodeFlags = NetNode.Flags.None;
                raycastInput.m_ignoreTerrain = true;

                if (ToolBase.RayCast(raycastInput, out RaycastOutput result))
                {
                    return NetManager.instance.GetSegmentName(result.m_netSegment);
                }
                return null;
            }
        }
    }
}
