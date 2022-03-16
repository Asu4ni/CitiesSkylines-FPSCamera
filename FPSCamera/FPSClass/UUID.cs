
namespace FPSCamMod
{
    public struct ObjectType
    {
        public static readonly ObjectType Citizen = new ObjectType(InstanceType.Citizen);
        public static readonly ObjectType CInstance = new ObjectType(InstanceType.CitizenInstance);
        public static readonly ObjectType Vehicle = new ObjectType(InstanceType.Vehicle);
        public static readonly ObjectType Building = new ObjectType(InstanceType.Building);
        public static readonly ObjectType TLine = new ObjectType(InstanceType.TransportLine);
        public static readonly ObjectType Node = new ObjectType(InstanceType.NetNode);

        public InstanceType _type { get; private set; }
        private ObjectType(InstanceType _type) { this._type = _type; }
        public static explicit operator ObjectType(InstanceType _type) => new ObjectType(_type);

        public byte switchValue => (byte) _type;
        public const byte sCitizen = (byte) InstanceType.Citizen;
        public const byte sCInstance = (byte) InstanceType.CitizenInstance;
        public const byte sVehicle = (byte) InstanceType.Vehicle;
    }

    public struct UUID
    {
        public CitizenID Citizen {
            get => (CitizenID) _id.Citizen;
            set => _id.Citizen = value._id;
        }
        public static explicit operator UUID(CitizenID id)
        { UUID uid = default; uid.Citizen = id; return uid; }

        public CInstanceID CInstance {
            get => (CInstanceID) _id.CitizenInstance;
            set => _id.CitizenInstance = value._id;
        }
        public static explicit operator UUID(CInstanceID id)
        { UUID uid = default; uid.CInstance = id; return uid; }

        public VehicleID Vehicle {
            get => (VehicleID) _id.Vehicle;
            set => _id.Vehicle = value._id;
        }
        public static explicit operator UUID(VehicleID id)
        { UUID uid = default; uid.Vehicle = id; return uid; }

        public BuildingID Building {
            get => (BuildingID) _id.Building;
            set => _id.Building = value._id;
        }
        public static explicit operator UUID(BuildingID id)
        { UUID uid = default; uid.Building = id; return uid; }

        public TLineID TLine {
            get => (TLineID) _id.TransportLine;
            set => _id.TransportLine = value._id;
        }
        public static explicit operator UUID(TLineID id)
        { UUID uid = default; uid.TLine = id; return uid; }

        public NodeID Node {
            get => (NodeID) _id.NetNode;
            set => _id.NetNode = value._id;
        }
        public static explicit operator UUID(NodeID id)
        { UUID uid = default; uid.Node = id; return uid; }

        public static readonly UUID Empty = new UUID(InstanceID.Empty);
        public bool Exists => !_id.IsEmpty;
        public ObjectType Type => (ObjectType) _id.Type;
        public override string ToString() => _id.ToString();

        private InstanceID _id;
        private UUID(InstanceID id) { this._id = id; }
        public static explicit operator UUID(InstanceID _id) => new UUID(_id);
        public static explicit operator InstanceID(UUID id) => id._id;
    }

    public class BaseID<T> where T : System.IComparable<T>
    {
        public bool Exists => _id.CompareTo(default) != 0;

        public T _id { get; private set; }
        protected BaseID(T id) { this._id = id; }
        public override string ToString() => _id.ToString();
    }
    public class CitizenID : BaseID<uint>
    {
        private CitizenID(uint id) : base(id) { }
        public static explicit operator CitizenID(uint id) => new CitizenID(id);
    }
    public class CInstanceID : BaseID<ushort>
    {
        private CInstanceID(ushort id) : base(id) { }
        public static explicit operator CInstanceID(ushort id) => new CInstanceID(id);
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
    public class TLineID : BaseID<ushort>
    {
        private TLineID(ushort id) : base(id) { }
        public static explicit operator TLineID(ushort id) => new TLineID(id);
    }
    public class NodeID : BaseID<ushort>
    {
        private NodeID(ushort id) : base(id) { }
        public static explicit operator NodeID(ushort id) => new NodeID(id);
    }
}
