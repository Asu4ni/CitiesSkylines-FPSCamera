using System.Collections.Generic;

namespace FPSCamMod
{
    public class FPSInstance
    {
        public static FPSInstance Of(UUID id) => new FPSInstance(id);
        public FPSInstance() { uuid = UUID.Empty; }
        public FPSInstance(UUID id) { uuid = id; }

        public virtual bool IsValid => InstanceManager.IsValid((InstanceID) uuid);

        public virtual string GetName()
        {
            var name = InstanceManager.instance.GetName((InstanceID) uuid);
            return string.IsNullOrEmpty(name) ? "(unknown)" : name;
        }

        public UUID uuid { get; protected set; }
    }
    public abstract class FPSInstanceToFollow : FPSInstance
    {
        public FPSInstanceToFollow() : base() { }
        public FPSInstanceToFollow(UUID id) : base(id) { }

        public abstract CamSetting GetCamSetting();
        public abstract float GetSpeed();
        public abstract string GetStatus();
        // Key: attribute name, Value: attribute value
        public abstract Details GetDetails();

        public class Details : List<Detail>
        {
            public string this[string field] { set => Add(new Detail(field, value)); }
        }
        public struct Detail
        {
            public readonly string field, text;
            public Detail(string field, string text)
            {
                this.field = field;
                this.text = text;
            }
        }
    }

    public class FPSBuilding : FPSInstance
    {
        public static FPSBuilding Of(BuildingID id) => new FPSBuilding(id);
        public FPSBuilding(BuildingID id) : base((UUID) id) { }

        public override string GetName()
            => BuildingManager.instance.GetBuildingName(uuid.Building._id, (InstanceID) uuid);
    }
    public class FPSTransportLine : FPSInstance
    {
        public static FPSTransportLine Of(TLineID id) => new FPSTransportLine(id);
        public FPSTransportLine(TLineID id) : base((UUID) id) { }

        public override string GetName()
            => TransportManager.instance.GetLineName(uuid.TLine._id);
    }
    public class FPSNode : FPSInstance
    {
        public static FPSNode Of(NodeID id) => new FPSNode(id);
        public FPSNode(NodeID id) : base((UUID) id) { }

        public TLineID GetTransportLineID() => (TLineID) NetManager.instance.m_nodes.
                                                         m_buffer[uuid.Node._id].m_transportLine;
    }
}
