namespace FPSCamera.Util
{
    public static class NetUtil
    {
        public static NetManager netMan = NetManager.instance;
        private static NetNode[] nodeBuffer_ = netMan.m_nodes.m_buffer;
        private static NetSegment[] segmentBuffer_ = netMan.m_segments.m_buffer;
        private static NetLane[] laneBuffer_ = netMan.m_lanes.m_buffer;

        public static ref NetNode ToNode(this ushort id) => ref nodeBuffer_[id];
        public static ref NetSegment ToSegment(this ushort id) => ref segmentBuffer_[id];
        public static ref NetLane ToLane(this uint id) => ref laneBuffer_[id];

        public static bool IsStartNode(ushort segmentId, ushort nodeId) =>
            segmentId.ToSegment().m_startNode == nodeId;
        public static bool IsStartNode(this ref NetSegment segment, ushort nodeId) =>
            segment.m_startNode == nodeId;
    }
}
