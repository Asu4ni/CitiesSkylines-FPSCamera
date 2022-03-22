namespace FPSCamera.Wrapper
{
    public abstract class ID
    {
        public static ID FromGame(InstanceID implID)
        {
            switch (implID.Type) {
            case InstanceType.Building: return BuildingID.FromGame(implID.Building);
            case InstanceType.Vehicle: return VehicleID.FromGame(implID.Vehicle);
            case InstanceType.Citizen: return HumanID.FromGame(implID.Citizen);
            case InstanceType.NetNode: return NodeID.FromGame(implID.NetNode);
            case InstanceType.ParkedVehicle: return ParkedCarID.FromGame(implID.ParkedVehicle);
            case InstanceType.TransportLine: return TransitID.FromGame(implID.TransportLine);
            case InstanceType.CitizenInstance: return PedestrianID.FromGame(implID.NetNode);
            default: return null;
            }
        }
        public override string ToString() => implID.ToString();
        public bool IsValid => InstanceManager.IsValid(implID);

        public readonly InstanceID implID;
        protected ID(InstanceID id) { implID = id; }
    }

    public abstract class BaseID<T> : ID where T : struct, System.IComparable<T>
    {
        public override string ToString() => $"{implIndex}/{base.ToString()}";

        public readonly T implIndex;
        protected BaseID(T index, InstanceID implID) : base(implID) { implIndex = index; }

        protected static Derived NullIfInvalid<Derived>(Derived id) where Derived : BaseID<T>
            => id.implIndex.CompareTo(default) == 0 ? null : id;
    }

    public class HumanID : BaseID<uint>
    {
        public static HumanID FromGame(uint index) => NullIfInvalid(new HumanID(index,
                new InstanceID { Citizen = index }));
        private HumanID(uint index, InstanceID implID) : base(index, implID) { }
    }
    public class PedestrianID : BaseID<ushort>
    {
        public static PedestrianID FromGame(ushort index) => NullIfInvalid(new PedestrianID(index,
                new InstanceID { CitizenInstance = index }));
        private PedestrianID(ushort index, InstanceID implID) : base(index, implID) { }
    }
    public class VehicleID : BaseID<ushort>
    {
        public static VehicleID FromGame(ushort index) => NullIfInvalid(new VehicleID(index,
                new InstanceID { Vehicle = index }));
        private VehicleID(ushort index, InstanceID implID) : base(index, implID) { }
    }
    public class ParkedCarID : BaseID<ushort>
    {
        public static ParkedCarID FromGame(ushort index) => NullIfInvalid(new ParkedCarID(index,
                new InstanceID { ParkedVehicle = index }));
        private ParkedCarID(ushort index, InstanceID implID) : base(index, implID) { }
    }
    public class BuildingID : BaseID<ushort>
    {
        public static BuildingID FromGame(ushort index) => NullIfInvalid(new BuildingID(index,
                new InstanceID { Building = index }));
        private BuildingID(ushort index, InstanceID implID) : base(index, implID) { }
    }
    public class TransitID : BaseID<ushort>
    {
        public static TransitID FromGame(ushort index) => NullIfInvalid(new TransitID(index,
                new InstanceID { TransportLine = index }));
        private TransitID(ushort index, InstanceID implID) : base(index, implID) { }
    }
    public class NodeID : BaseID<ushort>
    {
        public static NodeID FromGame(ushort index) => NullIfInvalid(new NodeID(index,
                new InstanceID { NetNode = index }));
        private NodeID(ushort index, InstanceID implID) : base(index, implID) { }
    }
}
