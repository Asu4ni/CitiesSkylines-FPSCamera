namespace FPSCamera.Wrapper
{
    public interface IObject
    {
        string Name { get; }
        bool IsValid { get; }
    }
    public interface IObjectToFollow : IObject
    {
        float GetSpeed();
        string GetStatus();
        Transform.Positioning GetCamPositioning();
        Cam.Details GetDetails();
    }

    public abstract class Object : IObject
    {
        public abstract string Name { get; }
        public abstract bool IsValid { get; }

        public static Object Of(ID id)
        {
            switch (id) {
            case HumanID hid: return Human._Of(hid);
            case PedestrianID pid: return Pedestrian._Of(pid);
            case VehicleID vid: return Vehicle._Of(vid);
            case BuildingID bid: return Building._Of(bid);
            // case ParkedCarID pcid: return .Of(pcid);
            // case NodeID nid: return .Of(nid);
            // case TransitID tid: return .Of(tid);
            default: return null;
            }
        }

        public static Object OfIfValid(ID id)
            => Of(id) is Object obj && obj.IsValid ? obj : null;
    }

    public abstract class Object<ObjectID> : Object where ObjectID : ID
    {
        public override bool IsValid => InstanceManager.IsValid(id.implID);
        public override string Name {
            get {
                var name = InstanceManager.instance.GetName(id.implID);
                return string.IsNullOrEmpty(name) ? "(unknown)" : name;
            }
        }
        protected Object(ObjectID id) { this.id = id; }

        public readonly ObjectID id;
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
