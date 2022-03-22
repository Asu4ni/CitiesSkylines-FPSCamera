namespace FPSCamera.Wrapper
{
    public interface IObject
    {
        string Name { get; }
        ID ObjectID { get; }
    }
    public interface IObjectToFollow : IObject
    {
        float GetSpeed();
        Transform.Positioning GetPositioning();
        Utils.Infos GetInfos();
        string GetStatus();
    }

    public abstract class Object : IObject
    {
        public abstract string Name { get; }

        public static Object Of(ID id)
        {
            if (!(id?.IsValid ?? false)) return null;
            switch (id) {
            case HumanID hid: return Human._Of(hid);
            case PedestrianID pid: return Pedestrian._Of(pid);
            case VehicleID vid: return Vehicle._Of(vid);
            case BuildingID bid: return Building._Of(bid);
            default: return null;
            }
        }
        public abstract ID ObjectID { get; }
    }

    public abstract class Object<IDType> : Object where IDType : ID
    {
        public override ID ObjectID => id;
        public override string Name {
            get {
                var name = InstanceManager.instance.GetName(id.implID);
                return string.IsNullOrEmpty(name) ? "(unknown)" : name;
            }
        }
        protected Object(IDType id) { this.id = id; }

        public readonly IDType id;
    }

    public class Building : Object<BuildingID>
    {
        public override string Name => GetName(id);
        public static string GetName(BuildingID id)
            => BuildingManager.instance.GetBuildingName(id.implIndex, id.implID);

        internal static Building _Of(BuildingID id)
            => new Building(id);
        private Building(BuildingID id) : base(id) { }
    }
    public class TransitLine : Object<TransitID>
    {
        public override string Name => GetName(id);
        public static string GetName(TransitID id)
            => TransportManager.instance.GetLineName(id.implIndex);

        private TransitLine(TransitID id) : base(id) { }
    }
    public class Node : Object<NodeID>
    {
        public TransitID TransitLineID => GetTransitLineID(id);
        public static TransitID GetTransitLineID(NodeID id)
            => TransitID.FromGame(NetManager.instance.m_nodes
                            .m_buffer[id.implIndex].m_transportLine);

        private Node(NodeID id) : base(id) { }
    }
}
