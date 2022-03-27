namespace CSkyL.Game.Object
{
    using CSkyL.Game.ID;

    public class CargoVehicle : Vehicle
    {
        public CargoVehicle(VehicleID id) : base(id) { }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            GetLoadAndCapacity(out int load, out int capacity);
            details["Load"] = capacity > 0 ? ((float) load / capacity).ToString("P1")
                                         : "(invalid)";
        }
    }

    public class TransitVehicle : Vehicle
    {
        public TransitVehicle(VehicleID id, string typeName) : base(id) { _typeName = typeName; }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            details["Transit/" + _typeName] = GetTransitLineID() is TransitID id ?
                    TransitLine.GetName(id) : "(irregular)";

            GetLoadAndCapacity(out int load, out int capacity);
            details["Passenger"] = $"{load,4} /{capacity,4}";

        }

        private readonly string _typeName;
    }

    public class ServiceVehicle : Vehicle
    {
        public ServiceVehicle(VehicleID id, string typeName) : base(id) { _typeName = typeName; }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            details["Service"] = _typeName;

            GetLoadAndCapacity(out int load, out int capacity);
            if (capacity > 0) details["Load"] = ((float) load / capacity).ToString("P1");
        }
        private readonly string _typeName;
    }
    public class Taxi : ServiceVehicle
    {
        public Taxi(VehicleID id) : base(id, "taxi") { }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            base._MoreDetails(ref details);

            if (details.Find((_info) => _info.field == "Load") is Utils.Info info)
                info = new Utils.Info("Work Shift", info.text);
        }
    }

    public class PersonalVehicle : Vehicle
    {
        public PersonalVehicle(VehicleID id) : base(id) { }
        public HumanID GetDriverID() => GetOwnerID() as HumanID;
    }
    public class Bicycle : PersonalVehicle
    {
        public Bicycle(VehicleID id) : base(id) { }
        public override string GetStatus()
            => GetOwnerID() is HumanID hid ?
                   (Of(hid) as Pedestrian)?.GetStatus() : null;
    }

    public class MissionVehicle : Vehicle
    {
        public MissionVehicle(VehicleID id) : base(id) { }
    }
}
