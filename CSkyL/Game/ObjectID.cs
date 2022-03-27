namespace CSkyL.Game.ID
{
    public abstract class ObjectID
    {
        public static ObjectID _FromIID(InstanceID implID)
        {
            switch (implID.Type) {
            case InstanceType.Building: return BuildingID._FromIndex(implID.Building);
            case InstanceType.Vehicle: return VehicleID._FromIndex(implID.Vehicle);
            case InstanceType.Citizen: return HumanID._FromIndex(implID.Citizen);
            case InstanceType.NetNode: return NodeID._FromIndex(implID.NetNode);
            case InstanceType.ParkedVehicle: return ParkedCarID._FromIndex(implID.ParkedVehicle);
            case InstanceType.TransportLine: return TransitID._FromIndex(implID.TransportLine);
            case InstanceType.CitizenInstance: return PedestrianID._FromIndex(implID.NetNode);
            default: return null;
            }
        }
        public override string ToString() => implID.ToString();
        public bool IsValid => InstanceManager.IsValid(implID);

        public readonly InstanceID implID;
        protected ObjectID(InstanceID id) { implID = id; }
    }

    public abstract class BaseID<T> : ObjectID where T : struct, System.IComparable<T>
    {
        public override string ToString() => $"{implIndex}/{base.ToString()}";

        public readonly T implIndex;
        protected BaseID(T index, InstanceID implID) : base(implID) { implIndex = index; }

        protected static Derived NullIfInvalid<Derived>(Derived id) where Derived : BaseID<T>
            => id.implIndex.CompareTo(default) == 0 ? null : id;
    }

    public class HumanID : BaseID<uint>
    {
        internal static HumanID _FromIndex(uint index) => NullIfInvalid(
                new HumanID(index, new InstanceID { Citizen = index }));
        private HumanID(uint index, InstanceID implID) : base(index, implID) { }
    }
    public class PedestrianID : BaseID<ushort>
    {
        internal static PedestrianID _FromIndex(ushort index) => NullIfInvalid(
                new PedestrianID(index, new InstanceID { CitizenInstance = index }));
        private PedestrianID(ushort index, InstanceID implID) : base(index, implID) { }
    }
    public class VehicleID : BaseID<ushort>
    {
        internal static VehicleID _FromIndex(ushort index) => NullIfInvalid(
                new VehicleID(index, new InstanceID { Vehicle = index }));
        private VehicleID(ushort index, InstanceID implID) : base(index, implID) { }
    }
    public class ParkedCarID : BaseID<ushort>
    {
        internal static ParkedCarID _FromIndex(ushort index) => NullIfInvalid(
                new ParkedCarID(index, new InstanceID { ParkedVehicle = index }));
        private ParkedCarID(ushort index, InstanceID implID) : base(index, implID) { }
    }
    public class BuildingID : BaseID<ushort>
    {
        internal static BuildingID _FromIndex(ushort index) => NullIfInvalid(
                new BuildingID(index, new InstanceID { Building = index }));
        private BuildingID(ushort index, InstanceID implID) : base(index, implID) { }
    }
    public class TransitID : BaseID<ushort>
    {
        internal static TransitID _FromIndex(ushort index) => NullIfInvalid(
                new TransitID(index, new InstanceID { TransportLine = index }));
        private TransitID(ushort index, InstanceID implID) : base(index, implID) { }
    }
    public class NodeID : BaseID<ushort>
    {
        internal static NodeID _FromIndex(ushort index) => NullIfInvalid(
                new NodeID(index, new InstanceID { NetNode = index }));
        private NodeID(ushort index, InstanceID implID) : base(index, implID) { }
    }
}
