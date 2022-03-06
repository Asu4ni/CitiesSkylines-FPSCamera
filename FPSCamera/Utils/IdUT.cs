
namespace FPSCamMod
{
    public struct ObjectType
    {
        public static readonly ObjectType Citizen = new ObjectType(InstanceType.Citizen);
        public static readonly ObjectType Vehicle = new ObjectType(InstanceType.Vehicle);
        public static readonly ObjectType Building = new ObjectType(InstanceType.Building);

        public InstanceType _type { get; private set; }
        private ObjectType(InstanceType _type) { this._type = _type; }
        public static explicit operator ObjectType(InstanceType _type) => new ObjectType(_type);

        public byte switchValue => (byte) _type;
        public const byte sCitizen = (byte) InstanceType.Citizen;
        public const byte sVehicle = (byte) InstanceType.Vehicle;
        public const byte sBuilding = (byte) InstanceType.Building;
    }

    public struct UUID
    {
        public CitizenID Citizen { get => (CitizenID) _id.Citizen; set => _id.Citizen = value._id; }
        public static implicit operator UUID(CitizenID id)
        { UUID uid = default; uid.Citizen = id; return uid; }

        public VehicleID Vehicle { get => (VehicleID) _id.Vehicle; set => _id.Vehicle = value._id; }
        public static implicit operator UUID(VehicleID id)
        { UUID uid = default; uid.Vehicle = id; return uid; }

        public BuildingID Building {
            get => (BuildingID) _id.Building;
            set => _id.Building = value._id;
        }
        public static implicit operator UUID(BuildingID id)
        { UUID uid = default; uid.Building = id; return uid; }

        public static readonly UUID Empty = new UUID(InstanceID.Empty);
        public bool exists => !_id.IsEmpty;
        public ObjectType Type => (ObjectType) _id.Type;
        public override string ToString() => _id.ToString();

        private InstanceID _id;
        private UUID(InstanceID id) { this._id = id; }
        public static explicit operator UUID(InstanceID _id) => new UUID(_id);
    }

    public class BaseID<T> where T : System.IComparable<T>
    {
        public bool exists => _id.CompareTo(default) != 0;

        public T _id { get; private set; }
        protected BaseID(T id) { this._id = id; }
        public override string ToString() => _id.ToString();
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
}
